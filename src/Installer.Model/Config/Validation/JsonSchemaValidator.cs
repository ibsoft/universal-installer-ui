namespace Installer.Model.Config.Validation
{
    public sealed class ValidationResult
    {
        public bool IsValid => Errors.Count == 0;
        public List<string> Errors { get; } = new();
    }

    public static class JsonSchemaValidator
    {
        public static ValidationResult Validate(Installer.Model.Config.InstallerConfig cfg)
        {
            var res = new ValidationResult();
            if (cfg.Meta == null || string.IsNullOrWhiteSpace(cfg.Meta.Name)) res.Errors.Add("meta.name is required");
            if (cfg.Artifacts == null) cfg.Artifacts = new();
            if (cfg.Tasks == null) cfg.Tasks = new();
            var ids = cfg.Artifacts.Select(a=> a.Id).ToHashSet();
            foreach (var t in cfg.Tasks.Concat(cfg.PostTasks ?? new()).Concat(cfg.Uninstall?.Tasks ?? new()))
            {
                if (string.Equals(t?.Type, "runartifact", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(t?.Type, "unzip", StringComparison.OrdinalIgnoreCase))
                {
                    if (t?.Ref == null || !ids.Contains(t.Ref)) res.Errors.Add($"task.ref '{t?.Ref}' not found in artifacts");
                }
            }
            return res;
        }
    }
}
