using System.Drawing;
using System.Windows.Forms;

namespace DBMigration
{
    partial class test
    {
        private System.ComponentModel.IContainer components = null;

        private Panel panelResizeMenu;
        private bool isResizing = false;
        private Point lastMousePosition;

        private System.Windows.Forms.Panel panelMenu;
        private System.Windows.Forms.Panel panelContent;
        private System.Windows.Forms.Button btnAction;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this.panelMenu = new System.Windows.Forms.Panel();
            this.panelContent = new System.Windows.Forms.Panel();
            this.btnAction = new System.Windows.Forms.Button();
            this.panelResizeMenu = new System.Windows.Forms.Panel();
            this.SuspendLayout();
            // 
            // panelMenu
            // 
            this.panelMenu.BackColor = System.Drawing.Color.LightGray;
            this.panelMenu.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelMenu.Dock = System.Windows.Forms.DockStyle.Left;
            this.panelMenu.Location = new System.Drawing.Point(0, 0);
            this.panelMenu.Name = "panelMenu";
            this.panelMenu.Size = new System.Drawing.Size(256, 544);
            this.panelMenu.TabIndex = 2;
            // 
            // panelContent
            // 
            this.panelContent.BackColor = System.Drawing.Color.White;
            this.panelContent.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelContent.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelContent.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.panelContent.Location = new System.Drawing.Point(256, 0);
            this.panelContent.Name = "panelContent";
            this.panelContent.Size = new System.Drawing.Size(497, 544);
            this.panelContent.TabIndex = 1;
            // 
            // btnAction
            // 
            this.btnAction.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnAction.Location = new System.Drawing.Point(653, 494);
            this.btnAction.Name = "btnAction";
            this.btnAction.Size = new System.Drawing.Size(80, 30);
            this.btnAction.TabIndex = 0;
            this.btnAction.Text = "OK";
            this.btnAction.UseVisualStyleBackColor = true;
            // 
            // panelResizeMenu
            // 
            this.panelResizeMenu.BackColor = System.Drawing.Color.DarkGray;
            this.panelResizeMenu.Cursor = System.Windows.Forms.Cursors.VSplit;
            this.panelResizeMenu.Dock = System.Windows.Forms.DockStyle.Left;
            this.panelResizeMenu.Location = new System.Drawing.Point(256, 0);
            this.panelResizeMenu.Name = "panelResizeMenu";
            this.panelResizeMenu.Size = new System.Drawing.Size(5, 544);
            this.panelResizeMenu.TabIndex = 0;
            this.panelResizeMenu.MouseDown += new System.Windows.Forms.MouseEventHandler(this.panelResizeMenu_MouseDown);
            this.panelResizeMenu.MouseMove += new System.Windows.Forms.MouseEventHandler(this.panelResizeMenu_MouseMove);
            this.panelResizeMenu.MouseUp += new System.Windows.Forms.MouseEventHandler(this.panelResizeMenu_MouseUp);
            // 
            // test
            // 
            this.ClientSize = new System.Drawing.Size(753, 544);
            this.Controls.Add(this.panelResizeMenu);
            this.Controls.Add(this.btnAction);
            this.Controls.Add(this.panelContent);
            this.Controls.Add(this.panelMenu);
            this.Name = "test";
            this.Text = "Main Layout";
            this.ResumeLayout(false);

        }


        #endregion
    }
}