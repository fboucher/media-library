using MudBlazor;

namespace MediaLibrary.Services;

public enum AppTheme { Light, Dark, TokyoNight }

public class ThemeService
{
    public AppTheme Current { get; private set; } = AppTheme.Light;

    public bool IsDarkMode => Current != AppTheme.Light;

    public MudTheme Theme => Current switch
    {
        AppTheme.TokyoNight => TokyoNightTheme,
        _                   => DefaultTheme
    };

    public event Action? OnChange;

    public void Set(AppTheme theme)
    {
        Current = theme;
        OnChange?.Invoke();
    }

    // ── Themes ────────────────────────────────────────────────────────────────

    private static readonly MudTheme DefaultTheme = new();

    private static readonly MudTheme TokyoNightTheme = new()
    {
        PaletteDark = new PaletteDark
        {
            Primary           = "#7aa2f7",
            PrimaryDarken     = "#5d86e0",
            PrimaryLighten    = "#a0bcf9",
            Secondary         = "#bb9af7",
            SecondaryDarken   = "#9d7dde",
            SecondaryLighten  = "#cdb4f9",
            Tertiary          = "#2ac3de",
            Info              = "#2ac3de",
            Success           = "#9ece6a",
            Warning           = "#e0af68",
            Error             = "#f7768e",
            Background        = "#1a1b26",
            BackgroundGray    = "#16161e",
            Surface           = "#24283b",
            DrawerBackground  = "#1f2335",
            DrawerText        = "#c0caf5",
            DrawerIcon        = "#a9b1d6",
            AppbarBackground  = "#1f2335",
            AppbarText        = "#c0caf5",
            TextPrimary       = "#c0caf5",
            TextSecondary     = "#a9b1d6",
            TextDisabled      = "#565f89",
            ActionDefault     = "#a9b1d6",
            LinesDefault      = "#3b4261",
            LinesInputs       = "#3b4261",
            Divider           = "#3b4261",
            TableLines        = "#3b4261",
            OverlayDark       = "rgba(26,27,38,0.8)",
        }
    };
}
