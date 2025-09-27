namespace Installer.Model.Config
{
    public sealed class InstallerConfig
    {
        public MetaSpec? Meta { get; set; }
        public UiSpec? Ui { get; set; }
        public List<ArtifactSpec>? Artifacts { get; set; }
        public List<PreCheckSpec>? PreChecks { get; set; }
        public List<TaskSpec>? Tasks { get; set; }
        public List<TaskSpec>? PostTasks { get; set; }
        public UninstallSpec? Uninstall { get; set; }
        public LoggingSpec? Logging { get; set; }
        public TelemetrySpec? Telemetry { get; set; }
    }

    public sealed class MetaSpec { public string? Name { get; set; } public string? Version { get; set; } public string? Publisher { get; set; } public string? SupportUrl { get; set; } }

    public sealed class UiSpec
    {
        public string? LogoPath { get; set; }
        public string? Title { get; set; }
        public string? Subtitle { get; set; }
        public ThemeSpec? Theme { get; set; }
        public string? Locale { get; set; }
        public WindowSpec? Window { get; set; }
    }
    public sealed class ThemeSpec { public string? Mode { get; set; } public string? PrimaryColor { get; set; } public int FontSize { get; set; } = 10; }
    public sealed class WindowSpec { public int Width { get; set; } = 900; public int Height { get; set; } = 600; public bool TopMost { get; set; } }

    public sealed class ArtifactSpec
    {
        public string Id { get; set; } = "";
        public string Type { get; set; } = "";
        public string Source { get; set; } = "";
        public string? Sha256 { get; set; }
        public string? TargetDir { get; set; }
        public string? SilentArgs { get; set; }
    }

    public sealed class PreCheckSpec
    {
        public string? Type { get; set; }
        public string? FailMessage { get; set; }
        public string? Min { get; set; }
        public string? Drive { get; set; }
        public int? MinMB { get; set; }
        public int[]? Ports { get; set; }
    }

    public sealed class TaskSpec
    {
        public string? Type { get; set; }
        public string? Ref { get; set; }
        public bool ContinueOnError { get; set; }
        public string? TargetDir { get; set; }
        public string? Source { get; set; }
        public bool Recursive { get; set; }
        public string? Name { get; set; }
        public string? DisplayName { get; set; }
        public string? BinaryPath { get; set; }
        public string? StartMode { get; set; }
        public string? Account { get; set; }
        public string? Path { get; set; }
        public string? Value { get; set; }
        public string? Kind { get; set; }
        public string? EnvName { get; set; }
        public string? Location { get; set; }
        public string? IconPath { get; set; }
        public string? Target { get; set; }
        public string? Script { get; set; }
        public string? Args { get; set; }
        public int? TimeoutSec { get; set; }
        public string? ProductCode { get; set; }
    }

    public sealed class UninstallSpec
    {
        public List<TaskSpec>? PreTasks { get; set; }
        public List<TaskSpec>? Tasks { get; set; }
    }

    public sealed class LoggingSpec { public string? Level { get; set; } public string? File { get; set; } public int MaxSizeMB { get; set; } = 10; public int MaxFiles { get; set; } = 5; }
    public sealed class TelemetrySpec { public bool Enabled { get; set; } public string? Endpoint { get; set; } }
}
