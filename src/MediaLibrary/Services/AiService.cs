using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using MediaLibrary.Models;

namespace MediaLibrary.Services;

public partial class AiService(IHttpClientFactory httpFactory, IConfiguration config, ILogger<AiService> logger)
{
    public async Task<string> AnalyzeAsync(Connection connection, string prompt, MediaItem item)
    {
        var client = httpFactory.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", connection.ApiKey);

        object contentBlock = item.IsVideo
            ? await BuildVideoBlockAsync(item)
            : await BuildImageBlockAsync(item);

        var body = new
        {
            model    = connection.Model,
            messages = new[]
            {
                new
                {
                    role    = "user",
                    content = new object[]
                    {
                        contentBlock,
                        new { type = "text", text = prompt }
                    }
                }
            }
        };

        var url = BuildEndpointUrl(connection.Url);
        var json    = JsonSerializer.Serialize(body);
        var request = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };

        var response = await client.SendAsync(request);
        var raw      = await response.Content.ReadAsStringAsync();

        logger.LogDebug("AI API response: {Body}", raw);

        try
        {
            using var doc = JsonDocument.Parse(raw);
            var root = doc.RootElement;

            // Some providers return HTTP 200 with an error body
            if (root.TryGetProperty("error", out var errorEl))
            {
                var msg = errorEl.TryGetProperty("message", out var m) ? m.GetString() : raw;
                logger.LogWarning("AI API error in body: {Error}", msg);
                throw new InvalidOperationException(msg);
            }

            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning("AI API returned {Status}: {Body}", (int)response.StatusCode, raw);
                throw new InvalidOperationException($"API error {(int)response.StatusCode}: {raw}");
            }

            var full = root
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString() ?? "";

            return ExtractFirstParagraph(full);
        }
        catch (InvalidOperationException)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to parse AI response: {Body}", raw);
            throw new InvalidOperationException($"Unexpected response format: {raw}");
        }
    }

    private static string ExtractFirstParagraph(string text)
    {
        if (text.Length <= 350)
            return MultipleNewlines.Replace(text.Trim(), "\n");

        var paragraphs = text.Split(["\r\n\r\n", "\n\n"], StringSplitOptions.RemoveEmptyEntries);
        return paragraphs.FirstOrDefault(p => !string.IsNullOrWhiteSpace(p))?.Trim() ?? text.Trim();
    }

    private static string BuildEndpointUrl(string savedUrl)
    {
        var trimmed = savedUrl.TrimEnd('/');
        // If the user already typed the full endpoint path, use it as-is
        if (trimmed.EndsWith("/chat/completions", StringComparison.OrdinalIgnoreCase))
            return trimmed;
        return trimmed + "/chat/completions";
    }

    public async Task<(List<DetectedObject> Objects, string RawText)> DetectAsync(Connection connection, string researchWord, MediaItem item)
    {
        var client = httpFactory.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", connection.ApiKey);

        var contentBlock = await BuildImageBlockAsync(item);

        var body = new
        {
            model    = connection.Model,
            messages = new[]
            {
                new
                {
                    role    = "user",
                    content = new object[]
                    {
                        contentBlock,
                        new { type = "text", text = $"Detect: {researchWord}" }
                    }
                }
            }
        };

        var url     = BuildEndpointUrl(connection.Url);
        var json    = JsonSerializer.Serialize(body);
        var request = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };

        var response = await client.SendAsync(request);
        var raw      = await response.Content.ReadAsStringAsync();

        logger.LogDebug("AI detect response: {Body}", raw);

        string content;
        try
        {
            using var doc = JsonDocument.Parse(raw);
            var root = doc.RootElement;

            if (root.TryGetProperty("error", out var errorEl))
            {
                var msg = errorEl.TryGetProperty("message", out var m) ? m.GetString() : raw;
                throw new InvalidOperationException(msg);
            }

            if (!response.IsSuccessStatusCode)
                throw new InvalidOperationException($"API error {(int)response.StatusCode}: {raw}");

            content = root
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString() ?? "";
        }
        catch (InvalidOperationException)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to parse detect response: {Body}", raw);
            throw new InvalidOperationException($"Unexpected response format: {raw}");
        }

        return (ParseDetections(content), content);
    }

    private static List<DetectedObject> ParseDetections(string content)
    {
        // Truncate at first <sep> — model may hallucinate conversation after the structured block
        var sepIndex = content.IndexOf("<sep>", StringComparison.Ordinal);
        if (sepIndex >= 0)
            content = content[..sepIndex];

        var results = new List<DetectedObject>();
        // Match pairs of <ref>label</ref><bbox>coords</bbox>
        var refBboxPattern = new System.Text.RegularExpressions.Regex(
            @"<ref>(.*?)</ref><bbox>(.*?)</bbox>",
            System.Text.RegularExpressions.RegexOptions.Singleline);

        foreach (System.Text.RegularExpressions.Match match in refBboxPattern.Matches(content))
        {
            var label  = match.Groups[1].Value.Trim();
            var bboxes = match.Groups[2].Value
                .Split(';', StringSplitOptions.RemoveEmptyEntries)
                .Select(b => b.Trim())
                .Where(b => !string.IsNullOrEmpty(b))
                .ToList();

            if (!string.IsNullOrEmpty(label) && bboxes.Count > 0)
                results.Add(new DetectedObject { Ref = label, Bboxes = bboxes });
        }

        return results;
    }

    private async Task<object> BuildVideoBlockAsync(MediaItem item)
    {
        var mediaRoot = config["Media:StoragePath"] ?? "/data/media";
        var filePath  = Path.Combine(mediaRoot, item.FileName);
        var bytes     = await File.ReadAllBytesAsync(filePath);
        var base64    = Convert.ToBase64String(bytes);
        var extension = Path.GetExtension(item.FileName).TrimStart('.').ToLower();
        var mimeType  = extension switch
        {
            "mp4"  => "video/mp4",
            "webm" => "video/webm",
            "avi"  => "video/x-msvideo",
            "mov"  => "video/quicktime",
            "mkv"  => "video/x-matroska",
            _      => "video/mp4"
        };

        return new
        {
            type      = "video_url",
            video_url = new { url = $"data:{mimeType};base64,{base64}" }
        };
    }

    private async Task<object> BuildImageBlockAsync(MediaItem item)
    {
        var mediaRoot = config["Media:StoragePath"] ?? "/data/media";
        var filePath  = Path.Combine(mediaRoot, item.FileName);
        var bytes     = await File.ReadAllBytesAsync(filePath);
        var base64    = Convert.ToBase64String(bytes);
        var extension = Path.GetExtension(item.FileName).TrimStart('.').ToLower();
        var mimeType  = extension switch
        {
            "jpg" or "jpeg" => "image/jpeg",
            "png"           => "image/png",
            "gif"           => "image/gif",
            "webp"          => "image/webp",
            _               => "image/jpeg"
        };

        return new
        {
            type      = "image_url",
            image_url = new { url = $"data:{mimeType};base64,{base64}" }
        };
    }

    private static readonly Regex MultipleNewlines = new(@"(\r?\n){2,}", RegexOptions.Compiled);
}
