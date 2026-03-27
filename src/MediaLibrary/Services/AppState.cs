using MediaLibrary.Models;

namespace MediaLibrary.Services;

public class AppState
{
    public MediaItem? SelectedMedia { get; private set; }
    public Connection? ActiveConnection { get; private set; }
    public string Prompt { get; private set; } = "";

    public event Action? OnChange;

    public void SelectMedia(MediaItem? item)
    {
        SelectedMedia = item;
        OnChange?.Invoke();
    }

    public void SetActiveConnection(Connection? connection)
    {
        ActiveConnection = connection;
        OnChange?.Invoke();
    }

    public void SetPrompt(string prompt)
    {
        Prompt = prompt;
        OnChange?.Invoke();
    }
}
