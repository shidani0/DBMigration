using System.Windows.Forms;

namespace DBMigration
{
    partial class MainForm
    {
        private System.ComponentModel.IContainer components = null;
        private TreeView treeView;
        private Button exportButton;
        private Button convertToPostgresButton;

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
            this.treeView = new System.Windows.Forms.TreeView();
            this.exportButton = new System.Windows.Forms.Button();
            this.convertToPostgresButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // treeView
            // 
            this.treeView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.treeView.CheckBoxes = true;
            this.treeView.Location = new System.Drawing.Point(10, 10);
            this.treeView.Name = "treeView";
            this.treeView.Size = new System.Drawing.Size(656, 678);
            this.treeView.TabIndex = 0;
            this.treeView.AfterCheck += new System.Windows.Forms.TreeViewEventHandler(this.TreeView_AfterCheck);
            // 
            // exportButton
            // 
            this.exportButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.exportButton.Location = new System.Drawing.Point(10, 694);
            this.exportButton.Name = "exportButton";
            this.exportButton.Size = new System.Drawing.Size(252, 38);
            this.exportButton.TabIndex = 1;
            this.exportButton.Text = "Экспортировать SQL скрипты";
            this.exportButton.UseVisualStyleBackColor = true;
            this.exportButton.Click += new System.EventHandler(this.exportButton_Click);
            // 
            // convertToPostgresButton
            // 
            this.convertToPostgresButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.convertToPostgresButton.Location = new System.Drawing.Point(414, 694);
            this.convertToPostgresButton.Name = "convertToPostgresButton";
            this.convertToPostgresButton.Size = new System.Drawing.Size(252, 38);
            this.convertToPostgresButton.TabIndex = 2;
            this.convertToPostgresButton.Text = "Конвертировать в PG";
            this.convertToPostgresButton.UseVisualStyleBackColor = true;
            this.convertToPostgresButton.Click += new System.EventHandler(this.convertToPostgresButton_Click);
            // 
            // MainForm
            // 
            this.ClientSize = new System.Drawing.Size(684, 761);
            this.Controls.Add(this.treeView);
            this.Controls.Add(this.exportButton);
            this.Controls.Add(this.convertToPostgresButton);
            this.MinimumSize = new System.Drawing.Size(700, 800);
            this.Name = "MainForm";
            this.Text = "Выбор таблиц";
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.ResumeLayout(false);

        }
    }
}
