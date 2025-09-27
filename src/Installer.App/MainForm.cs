using Installer.Core.Engine;
using Installer.Core.Util;
using Installer.Model.Config;
using Installer.Model.Config.Validation;
using Installer.App.Controls;
using Installer.App.Theming;

namespace Installer.App
{
    public partial class MainForm : Form
    {
        private InstallerConfig? _config;
        private InstallerEngine? _engine;
        private readonly FileLogger _logger;
        private string _configPath = Path.Combine(AppContext.BaseDirectory, "config.min.json");
        private string? _supportUrl;

        // Header gradient colors (theme-driven)
        private Color _headerStart = Color.FromArgb(12,16,24);
        private Color _headerEnd = Color.FromArgb(6,8,12);

        public MainForm()
        {
            InitializeComponent();
            _logger = new FileLogger(Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                "UniversalInstaller", "logs", "install.log"), 10, 5);
            HookEvents();
            LoadConfigAndApplyUi();
            if (chkDetails.Checked) txtLog.Visible = true;

            var iconPath = Path.Combine(AppContext.BaseDirectory, "assets", "App.ico");
            if (File.Exists(iconPath))
            {
                this.Icon = new Icon(iconPath); // window & taskbar icon
            }

            this.StartPosition = FormStartPosition.CenterScreen;
        }

        private void HookEvents()
        {
            btnExit.Click += (s, e) => Close();
            chkDetails.CheckedChanged += (s,e)=> txtLog.Visible = chkDetails.Checked;
            btnInstall.Click += async (s, e) => await RunWorkflowAsync(Mode.Install);
            btnUninstall.Click += async (s, e) => await RunWorkflowAsync(Mode.Uninstall);
            btnRepair.Click += async (s, e) => await RunWorkflowAsync(Mode.Repair);
        }

        private void AppendLog(string msg)
        {
            if (InvokeRequired) { BeginInvoke(new Action(() => AppendLog(msg))); return; }
            txtLog.AppendText($"[{DateTime.Now:HH:mm:ss}] {msg}{Environment.NewLine}");
        }

        private void SetStatus(string msg)
        {
            if (InvokeRequired) { BeginInvoke(new Action(() => SetStatus(msg))); return; }
            lblStatus.Text = msg;
        }

        private void SetTask(string msg)
        {
            if (InvokeRequired) { BeginInvoke(new Action(() => SetTask(msg))); return; }
            lblTask.Text = msg;
        }

        private void SetStepActive(StepBadge badge)
        {
            foreach (var b in new[] { badgeDetect, badgePre, badgeInstall, badgePost, badgeDone })
                b.State = StepBadge.StepState.Pending;
            badge.State = StepBadge.StepState.Active;
        }

        private void SetStepDone(StepBadge badge, bool error = false)
        {
            badge.State = error ? StepBadge.StepState.Error : StepBadge.StepState.Done;
        }

        private void ApplyTheme(InstallerConfig cfg)
        {
            try
            {
                _supportUrl = cfg.Meta?.SupportUrl;
                linkSupport.Visible = !string.IsNullOrWhiteSpace(_supportUrl);

                if (!string.IsNullOrWhiteSpace(cfg.Ui?.LogoPath))
                {
                    var logoPath = Path.IsPathRooted(cfg.Ui.LogoPath) ? cfg.Ui.LogoPath : Path.Combine(AppContext.BaseDirectory, cfg.Ui.LogoPath);
                    if (File.Exists(logoPath)) picLogo.Image = Image.FromFile(logoPath);
                }

                lblTitle.Text = cfg.Ui?.Title ?? "Universal Installer";
                lblSubtitle.Text = cfg.Ui?.Subtitle ?? "Setup Wizard";

                // THEME PALETTE
                var primary = ThemeHelper.ParseHex(cfg.Ui?.Theme?.PrimaryColor ?? "#3B82F6", Color.FromArgb(56, 132, 255));
                bool dark = string.Equals(cfg.Ui?.Theme?.Mode, "dark", StringComparison.OrdinalIgnoreCase);

                // global font size
                var baseSize = (cfg.Ui?.Theme?.FontSize ?? 9);
                if (baseSize < 8) baseSize = 9;
                this.Font = new Font("Segoe UI", baseSize, FontStyle.Regular);
                lblTitle.Font = new Font("Segoe UI", Math.Max(baseSize + 8, 16), FontStyle.Bold);
                lblSubtitle.Font = new Font("Segoe UI", Math.Max(baseSize - 0, 10), FontStyle.Regular);
                txtLog.Font = new Font("Consolas", Math.Max(baseSize - 1, 9), FontStyle.Regular);

                // palette per mode
                var bg = dark ? Color.FromArgb(18, 20, 24) : Color.White;
                var stepBg = dark ? Color.FromArgb(22, 24, 28) : Color.FromArgb(238, 242, 246);
                var textPri = dark ? Color.White : Color.FromArgb(23, 23, 23);
                var textSec = dark ? Color.Gainsboro : Color.FromArgb(82, 82, 91);
                _headerStart = dark ? Color.FromArgb(12, 16, 24) : Color.FromArgb(243, 244, 246);
                _headerEnd = dark ? Color.FromArgb(6, 8, 12) : Color.FromArgb(229, 231, 235);

                this.BackColor = bg;
                contentPanel.BackColor = bg;
                pnlSteps.BackColor = stepBg;
                barsPanel.BackColor = bg;
                pnlButtons.BackColor = Color.FromArgb(Math.Max(bg.R-4,0), Math.Max(bg.G-4,0), Math.Max(bg.B-4,0));
                lblTitle.ForeColor = textPri; // white in dark
                lblSubtitle.ForeColor = textSec;
                linkSupport.LinkColor = dark ? Color.FromArgb(94, 234, 212) : primary;
                linkSupport.ActiveLinkColor = linkSupport.VisitedLinkColor = linkSupport.DisabledLinkColor = linkSupport.LinkColor;

                // primary on controls
                btnInstall.BackColor = primary;
                try
                {
                    progressOverall.ColorPrimary = primary;
                    progressTask.ColorPrimary = primary;
                }
                catch { }

                // step badges active color = primary
                foreach (var b in new[] { badgeDetect, badgePre, badgeInstall, badgePost, badgeDone })
                {
                    b.ActiveBorderColor = primary;
                    b.ActiveFillColor = dark ? ControlPaint.Dark(primary) : ControlPaint.Light(primary);
                    b.ForeColor = textPri;
                    b.Invalidate();
                }

                // window sizing/topMost
                if (cfg.Ui?.Window != null)
                {
                    Width = Math.Max(860, cfg.Ui.Window.Width);
                    Height = Math.Max(560, cfg.Ui.Window.Height);
                    TopMost = cfg.Ui.Window.TopMost;
                }

                // repaint header with new gradient
                pnlHeader.Invalidate();
            }
            catch { }
        }

        private void LoadConfigAndApplyUi()
        {
            SetStatus("Detecting configuration…");
            badgeDetect.State = StepBadge.StepState.Active;
            try
            {
                var cfg = JsonConfigLoader.Load(_configPath, out _);
                var validation = JsonSchemaValidator.Validate(cfg);
                if (!validation.IsValid)
                {
                    MessageBox.Show($"Config validation failed:\n{string.Join("\n", validation.Errors)}", "Config Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    SetStepDone(badgeDetect, error: true);
                    return;
                }
                _config = cfg;
                ApplyTheme(cfg);
                _engine = new InstallerEngine(cfg, _logger);
                _engine.Log += (s, msg) => AppendLog(msg);
                _engine.ProgressChanged += (s, p) => progressOverall.Value = Math.Clamp(p, 0, 100);
                _engine.TaskStarted += (s, name) => { progressTask.Indeterminate = true; SetTask($"Running: {name}"); progressTask.Value = 0; };
                _engine.TaskCompleted += (s, name) => { progressTask.Indeterminate = false; progressTask.Value = 100; };
                _engine.TaskProgressChanged += (s, p) => { progressTask.Indeterminate = false; progressTask.Value = Math.Clamp(p, 0, 100); };
                _engine.StatusChanged += (s, m) => SetStatus(m);
                _engine.PhaseChanged += (s, ph) =>
                {
                    switch (ph)
                    {
                        case Installer.Core.Engine.Phase.PreChecks: SetStepActive(badgePre); break;
                        case Installer.Core.Engine.Phase.Install: SetStepActive(badgeInstall); break;
                        case Installer.Core.Engine.Phase.Post: SetStepActive(badgePost); break;
                        case Installer.Core.Engine.Phase.Done: SetStepDone(badgeDone); break;
                    }
                };
                btnInstall.Enabled = btnUninstall.Enabled = btnRepair.Enabled = true;
                SetStepDone(badgeDetect);
                SetStatus("Ready");
            }
            catch (Exception ex)
            {
                SetStepDone(badgeDetect, error: true);
                MessageBox.Show($"Failed to load config: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task RunWorkflowAsync(Mode mode)
        {
            if (_engine == null || _config == null) return;
            btnInstall.Enabled = btnUninstall.Enabled = btnRepair.Enabled = false;
            progressOverall.Value = 0; progressTask.Value = 0;

            try
            {
                Result result;
                switch (mode)
                {
                    case Mode.Install:
                        SetStepActive(badgePre);
                        result = await _engine.RunInstallAsync(CancellationToken.None);
                        break;
                    case Mode.Uninstall:
                        SetStepActive(badgeInstall); // reuse lane
                        result = await _engine.RunUninstallAsync(CancellationToken.None);
                        break;
                    case Mode.Repair:
                        SetStepActive(badgePre);
                        result = await _engine.RunRepairAsync(CancellationToken.None);
                        break;
                    default:
                        result = Result.Fail("Unknown mode"); break;
                }

                if (!result.Success)
                {
                    SetStepDone(badgeDone, error: true);
                    MessageBox.Show($"Operation failed: {result.Error}", "Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    SetStepDone(badgePre);
                    SetStepDone(badgeInstall);
                    SetStepDone(badgePost);
                    SetStepDone(badgeDone);
                    MessageBox.Show("Operation completed successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            finally
            {
                btnInstall.Enabled = btnUninstall.Enabled = btnRepair.Enabled = true;
            }
        }

        enum Mode { Install, Uninstall, Repair }
    }
}