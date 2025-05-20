using System.Drawing;
using System.Windows.Forms;

namespace DBMigration
{
    partial class MainForm
    {
        private System.ComponentModel.IContainer components = null;
        private TreeView treeView;

        private Panel panelResizeMenu;
        private bool isResizing = false;
        private Point lastMousePosition;
        private System.Windows.Forms.Panel panelContent;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.panelContent = new System.Windows.Forms.Panel();
            this.lblProgress = new System.Windows.Forms.Label();
            this.progressBar = new System.Windows.Forms.ProgressBar();
            this.btnExport = new System.Windows.Forms.Button();
            this.treeView = new System.Windows.Forms.TreeView();
            this.panelResizeMenu = new System.Windows.Forms.Panel();
            this.process1 = new System.Diagnostics.Process();
            this.lblPanelMenuConnectionFrom = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.panelMenu = new System.Windows.Forms.Panel();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.panelContent.SuspendLayout();
            this.panelMenu.SuspendLayout();
            this.SuspendLayout();
            // 
            // panelContent
            // 
            this.panelContent.BackColor = System.Drawing.Color.White;
            this.panelContent.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelContent.Controls.Add(this.lblProgress);
            this.panelContent.Controls.Add(this.progressBar);
            this.panelContent.Controls.Add(this.btnExport);
            this.panelContent.Controls.Add(this.treeView);
            this.panelContent.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelContent.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.panelContent.Location = new System.Drawing.Point(277, 0);
            this.panelContent.Name = "panelContent";
            this.panelContent.Size = new System.Drawing.Size(476, 544);
            this.panelContent.TabIndex = 1;
            // 
            // lblProgress
            // 
            this.lblProgress.AutoSize = true;
            this.lblProgress.Location = new System.Drawing.Point(24, 485);
            this.lblProgress.Name = "lblProgress";
            this.lblProgress.Size = new System.Drawing.Size(0, 20);
            this.lblProgress.TabIndex = 5;
            // 
            // progressBar
            // 
            this.progressBar.Location = new System.Drawing.Point(19, 508);
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(465, 23);
            this.progressBar.TabIndex = 4;
            // 
            // btnExport
            // 
            this.btnExport.Location = new System.Drawing.Point(315, 464);
            this.btnExport.Name = "btnExport";
            this.btnExport.Size = new System.Drawing.Size(169, 38);
            this.btnExport.TabIndex = 3;
            this.btnExport.Text = "Экспорт";
            this.btnExport.UseVisualStyleBackColor = true;
            this.btnExport.Click += new System.EventHandler(this.btnExport_Click);
            // 
            // treeView
            // 
            this.treeView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.treeView.CheckBoxes = true;
            this.treeView.Location = new System.Drawing.Point(10, 17);
            this.treeView.Name = "treeView";
            this.treeView.Size = new System.Drawing.Size(390, 385);
            this.treeView.TabIndex = 0;
            this.treeView.AfterCheck += new System.Windows.Forms.TreeViewEventHandler(this.TreeView_AfterCheck);
            // 
            // panelResizeMenu
            // 
            this.panelResizeMenu.BackColor = System.Drawing.Color.DarkGray;
            this.panelResizeMenu.Cursor = System.Windows.Forms.Cursors.VSplit;
            this.panelResizeMenu.Dock = System.Windows.Forms.DockStyle.Left;
            this.panelResizeMenu.Location = new System.Drawing.Point(277, 0);
            this.panelResizeMenu.Name = "panelResizeMenu";
            this.panelResizeMenu.Size = new System.Drawing.Size(5, 544);
            this.panelResizeMenu.TabIndex = 0;
            this.panelResizeMenu.MouseDown += new System.Windows.Forms.MouseEventHandler(this.panelResizeMenu_MouseDown);
            this.panelResizeMenu.MouseMove += new System.Windows.Forms.MouseEventHandler(this.panelResizeMenu_MouseMove);
            this.panelResizeMenu.MouseUp += new System.Windows.Forms.MouseEventHandler(this.panelResizeMenu_MouseUp);
            // 
            // process1
            // 
            this.process1.StartInfo.Domain = "";
            this.process1.StartInfo.LoadUserProfile = false;
            this.process1.StartInfo.Password = null;
            this.process1.StartInfo.StandardErrorEncoding = null;
            this.process1.StartInfo.StandardOutputEncoding = null;
            this.process1.StartInfo.UserName = "";
            this.process1.SynchronizingObject = this;
            // 
            // lblPanelMenuConnectionFrom
            // 
            this.lblPanelMenuConnectionFrom.AutoSize = true;
            this.lblPanelMenuConnectionFrom.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.lblPanelMenuConnectionFrom.Location = new System.Drawing.Point(3, 8);
            this.lblPanelMenuConnectionFrom.Name = "lblPanelMenuConnectionFrom";
            this.lblPanelMenuConnectionFrom.Size = new System.Drawing.Size(239, 20);
            this.lblPanelMenuConnectionFrom.TabIndex = 4;
            this.lblPanelMenuConnectionFrom.Text = "1. Подключение к искомой БД";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Underline))));
            this.label1.Location = new System.Drawing.Point(11, 28);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(159, 36);
            this.label1.TabIndex = 5;
            this.label1.Text = "2. Выбор объектов\nисходной БД";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            this.label2.Location = new System.Drawing.Point(3, 68);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(186, 60);
            this.label2.TabIndex = 6;
            this.label2.Text = "3.Сравнения структуры\nобъектов исходной\nи целевой БД";
            // 
            // panelMenu
            // 
            this.panelMenu.BackColor = System.Drawing.Color.LightGray;
            this.panelMenu.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelMenu.Controls.Add(this.label4);
            this.panelMenu.Controls.Add(this.label3);
            this.panelMenu.Controls.Add(this.label2);
            this.panelMenu.Controls.Add(this.label1);
            this.panelMenu.Controls.Add(this.lblPanelMenuConnectionFrom);
            this.panelMenu.Dock = System.Windows.Forms.DockStyle.Left;
            this.panelMenu.Location = new System.Drawing.Point(0, 0);
            this.panelMenu.Name = "panelMenu";
            this.panelMenu.Size = new System.Drawing.Size(277, 544);
            this.panelMenu.TabIndex = 2;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            this.label4.Location = new System.Drawing.Point(3, 148);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(190, 40);
            this.label4.TabIndex = 16;
            this.label4.Text = "4.1. Выгрузка объектов \n в целевую БД";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            this.label3.Location = new System.Drawing.Point(3, 128);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(240, 20);
            this.label3.TabIndex = 9;
            this.label3.Text = "4. Подключение к целевой БД";
            // 
            // MainForm
            // 
            this.ClientSize = new System.Drawing.Size(753, 544);
            this.Controls.Add(this.panelResizeMenu);
            this.Controls.Add(this.panelContent);
            this.Controls.Add(this.panelMenu);
            this.Name = "MainForm";
            this.Text = "Выбор таблиц";
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.panelContent.ResumeLayout(false);
            this.panelContent.PerformLayout();
            this.panelMenu.ResumeLayout(false);
            this.panelMenu.PerformLayout();
            this.ResumeLayout(false);

        }
        private Button btnExport;
        private ProgressBar progressBar;
        private Label lblProgress;
        private System.Diagnostics.Process process1;
        private Panel panelMenu;
        private Label label2;
        private Label label1;
        private Label lblPanelMenuConnectionFrom;
        private Label label3;
        private Label label4;
    }
}
