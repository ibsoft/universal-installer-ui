using Installer.App.Controls;
using Installer.App.Theming;

namespace Installer.App
{
    partial class MainForm
    {
        private System.ComponentModel.IContainer components = null!;
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.pnlHeader = new Panel();
            this.picLogo = new PictureBox();
            this.lblTitle = new Label();
            this.lblSubtitle = new Label();
            this.linkSupport = new LinkLabel();
            this.pnlSteps = new FlowLayoutPanel();
            this.badgeDetect = new StepBadge(){Text="Detect"};
            this.badgePre = new StepBadge(){Text="Pre-Checks"};
            this.badgeInstall = new StepBadge(){Text="Install"};
            this.badgePost = new StepBadge(){Text="Post-Steps"};
            this.badgeDone = new StepBadge(){Text="Done"};
            this.progressOverall = new ModernProgressBar();
            this.progressTask = new ModernProgressBar();
            this.lblTask = new Label();
            this.barsPanel = new Panel();
            this.contentPanel = new Panel();
                        this.txtLog = new TextBox();
            this.chkDetails = new CheckBox();
            this.btnInstall = new Button();
            this.btnUninstall = new Button();
            this.btnRepair = new Button();
            this.btnExit = new Button();
            this.lblStatus = new Label();
            this.SuspendLayout();

            // Header (gradient simulated with paint)
            pnlHeader.Dock = DockStyle.Top;
            pnlHeader.Height = 110;
            pnlHeader.Padding = new Padding(16);
            pnlHeader.Paint += (s,e)=> { var g = e.Graphics; var rect = pnlHeader.ClientRectangle; using var b = new System.Drawing.Drawing2D.LinearGradientBrush(rect, _headerStart, _headerEnd, 90f); g.FillRectangle(b, rect); };

            picLogo.Size = new Size(64,64);
            picLogo.Location = new Point(16, 18);
            picLogo.SizeMode = PictureBoxSizeMode.Zoom;

            lblTitle.AutoSize = true;
            lblTitle.Font = new Font("Segoe UI", 18F, FontStyle.Bold);
            lblTitle.ForeColor = Color.White;
            lblTitle.Location = new Point(96, 20);
            lblTitle.Text = "Universal Installer";
            lblTitle.BackColor = Color.Transparent;

            lblSubtitle.AutoSize = true;
            lblSubtitle.Font = new Font("Segoe UI", 10);
            lblSubtitle.ForeColor = Color.White;
            lblSubtitle.Location = new Point(98, 56);
            lblSubtitle.Text = "Setup Wizard";

            linkSupport.AutoSize = true;
            linkSupport.LinkColor = Color.White;
            linkSupport.Location = new Point(98, 80);
            linkSupport.Text = "Support";
            linkSupport.Visible = false;
            linkSupport.ActiveLinkColor = Color.White;
            linkSupport.VisitedLinkColor = Color.White;
            linkSupport.DisabledLinkColor = Color.White;
            linkSupport.LinkClicked += (s,e)=> { if (!string.IsNullOrEmpty(_supportUrl)) System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo{ FileName=_supportUrl, UseShellExecute=true }); };

            pnlHeader.Controls.AddRange(new Control[]{picLogo,lblTitle,lblSubtitle,linkSupport});

            // Steps
            pnlSteps.Dock = DockStyle.Top;
            pnlSteps.Height = 42;
            pnlSteps.FlowDirection = FlowDirection.LeftToRight;
            pnlSteps.Padding = new Padding(12,6,12,6);
            pnlSteps.BackColor = Color.FromArgb(22, 24, 28);
            foreach (var b in new StepBadge[]{badgeDetect,badgePre,badgeInstall,badgePost,badgeDone})
            {
                pnlSteps.Controls.Add(b);
            }

            // Progress + task label
            lblTask.Dock = DockStyle.Top;
            lblTask.Height = 22;
            lblTask.ForeColor = Color.Gainsboro;
            lblTask.Padding = new Padding(12,2,12,2);
            lblTask.Text = "Ready";

            barsPanel.Dock = DockStyle.Fill;
            barsPanel.Padding = new Padding(16, 8, 16, 12);
            barsPanel.Height = 90;
            barsPanel.BackColor = Color.FromArgb(250,250,250);

            progressOverall.Dock = DockStyle.Top;
            progressOverall.Height = 22;
            progressTask.Dock = DockStyle.Top;
            progressTask.Height = 18;

            // add to barsPanel (top stack)
            lblTask.Dock = DockStyle.Top;
            progressOverall.Dock = DockStyle.Top;
            progressTask.Dock = DockStyle.Top;
            progressOverall.Margin = new Padding(0,6,0,6);
            progressTask.Margin = new Padding(0,6,0,0);
            barsPanel.Controls.Add(progressTask);
            barsPanel.Controls.Add(progressOverall);
            barsPanel.Controls.Add(lblTask);

            btnInstall.Text = "Install";
            btnUninstall.Text = "Uninstall";
            btnRepair.Text = "Repair";
            btnExit.Text = "Exit";
            foreach (var b in new[]{btnInstall,btnUninstall,btnRepair,btnExit})
            {
                b.Height = 36; b.Width = 120; b.FlatStyle = FlatStyle.Flat;
                b.BackColor = Color.FromArgb(59,130,246);
                b.ForeColor = Color.White;
                b.Margin = new Padding(8);
                ThemeHelper.MakeRound(b, 10);
            }
            btnUninstall.BackColor = Color.FromArgb(36, 38, 44);
            btnRepair.BackColor = Color.FromArgb(36, 38, 44);
            btnExit.BackColor = Color.FromArgb(36, 38, 44);

            pnlButtons = new FlowLayoutPanel(){Dock=DockStyle.Bottom, Height=76, Padding=new Padding(12,8,12,12)};
            pnlButtons.Controls.AddRange(new Control[]{btnInstall,btnUninstall,btnRepair,btnExit});

            chkDetails.Text = "Show details";
            chkDetails.Checked = true;
            chkDetails.ForeColor = Color.Gainsboro;
            chkDetails.Dock = DockStyle.Top;
            chkDetails.CheckedChanged += (s,e)=> txtLog.Visible = chkDetails.Checked;

            txtLog.Multiline = true;
            txtLog.ScrollBars = ScrollBars.None;
            txtLog.ReadOnly = true;
            txtLog.Dock = DockStyle.Top;
            txtLog.Visible = true;
            txtLog.BackColor = Color.FromArgb(12,14,18);
            txtLog.ForeColor = Color.White;
            txtLog.Font = new Font("Consolas", 9);
            txtLog.BorderStyle = BorderStyle.None;
            txtLog.Height = 220;

            lblStatus.Dock = DockStyle.Top;
            lblStatus.Height = 26;
            lblStatus.ForeColor = Color.Gainsboro;
            lblStatus.Padding = new Padding(12,4,0,4);
            lblStatus.Text = "Detecting configuration…";

            contentPanel.Dock = DockStyle.Fill;
            contentPanel.BackColor = Color.FromArgb(18,20,24);

            this.Text = "Universal Installer";
            this.BackColor = Color.FromArgb(18, 20, 24);
            this.ClientSize = new Size(980, 640);
            contentPanel.Controls.Add(barsPanel);
            contentPanel.Controls.Add(txtLog);
            contentPanel.Controls.Add(chkDetails);
            this.Controls.Add(contentPanel);
            this.Controls.Add(chkDetails);
            this.Controls.Add(progressTask);
            this.Controls.Add(lblTask);
            this.Controls.Add(progressOverall);
            this.Controls.Add(pnlSteps);
            this.Controls.Add(lblStatus);
            this.Controls.Add(pnlHeader);
            this.Controls.Add(pnlButtons);
            this.MinimumSize = new Size(900, 600);

            this.ResumeLayout(false);
        }

        private Panel pnlHeader;
        private PictureBox picLogo;
        private Label lblTitle;
        private Label lblSubtitle;
        private LinkLabel linkSupport;
        private FlowLayoutPanel pnlSteps;
        private StepBadge badgeDetect;
        private StepBadge badgePre;
        private StepBadge badgeInstall;
        private StepBadge badgePost;
        private StepBadge badgeDone;
        private ModernProgressBar progressOverall;
        private ModernProgressBar progressTask;
        private Label lblTask;
        private TextBox txtLog;
        private CheckBox chkDetails;
        private Button btnInstall;
        private Button btnUninstall;
        private Button btnRepair;
        private Button btnExit;
        private Label lblStatus;
        private FlowLayoutPanel pnlButtons;
        private Panel contentPanel;
        private Panel barsPanel;
    }
}
