using System.Windows.Forms;

namespace DBMigration
{
    partial class MainForm
    {
        private System.ComponentModel.IContainer components = null;
        private TreeView treeView;
        private Button exportButton;

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
            this.SuspendLayout();
            // 
            // treeView
            // 
            this.treeView.CheckBoxes = true;
            this.treeView.Location = new System.Drawing.Point(10, 10);
            this.treeView.Name = "treeView";
            this.treeView.Size = new System.Drawing.Size(500, 400);
            this.treeView.TabIndex = 0;
            this.treeView.AfterCheck += new System.Windows.Forms.TreeViewEventHandler(this.TreeView_AfterCheck);
            // 
            // exportButton
            // 
            this.exportButton.Location = new System.Drawing.Point(10, 420);
            this.exportButton.Name = "exportButton";
            this.exportButton.Size = new System.Drawing.Size(150, 30);
            this.exportButton.TabIndex = 1;
            this.exportButton.Text = "Экспортировать SQL";
            this.exportButton.UseVisualStyleBackColor = true;
            this.exportButton.Click += new System.EventHandler(this.ExportButton_Click);
            // 
            // Form2
            // 
            this.ClientSize = new System.Drawing.Size(800, 470);
            this.Controls.Add(this.treeView);
            this.Controls.Add(this.exportButton);
            this.Name = "Form2";
            this.Text = "Выбор таблиц";
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.ResumeLayout(false);
        }
    }
}
