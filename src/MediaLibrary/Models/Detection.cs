namespace MediaLibrary.Models;

public class Detection
{
    public int Id { get; set; }
    public int MediaId { get; set; }
    public string ResearchWord { get; set; } = "";
    public string DetectedJson { get; set; } = "";
    public DateTime CreatedAt { get; set; }
}

public class DetectedObject
{
    public string Ref { get; set; } = "";
    public List<string> Bboxes { get; set; } = [];
}
