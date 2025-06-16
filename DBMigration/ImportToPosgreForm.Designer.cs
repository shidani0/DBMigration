using System.Drawing;
using System.Windows.Forms;


namespace DBMigration
{
    partial class ImportToPosgreForm
    {
        private System.ComponentModel.IContainer components = null;

        private Panel panelResizeMenu;
        private bool isResizing = false;
        private Point lastMousePosition;

        private System.Windows.Forms.Panel panelMenu;
        private System.Windows.Forms.Panel panelContent;

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
            this.label2 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.lblPanelMenuConnectionFrom = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.panelContent = new System.Windows.Forms.Panel();
            this.txtSearchLoadScripts = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.txtSearch = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.treeView = new System.Windows.Forms.TreeView();
            this.btnSelectFolder = new System.Windows.Forms.Button();
            this.btnExecuteScripts = new System.Windows.Forms.Button();
            this.checkedListBoxScripts = new System.Windows.Forms.CheckedListBox();
            this.panelResizeMenu = new System.Windows.Forms.Panel();
            this.panelMenu.SuspendLayout();
            this.panelContent.SuspendLayout();
            this.SuspendLayout();
            // 
            // panelMenu
            // 
            this.panelMenu.BackColor = System.Drawing.Color.LightGray;
            this.panelMenu.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelMenu.Controls.Add(this.label2);
            this.panelMenu.Controls.Add(this.label4);
            this.panelMenu.Controls.Add(this.label3);
            this.panelMenu.Controls.Add(this.lblPanelMenuConnectionFrom);
            this.panelMenu.Controls.Add(this.label1);
            this.panelMenu.Dock = System.Windows.Forms.DockStyle.Left;
            this.panelMenu.Location = new System.Drawing.Point(0, 0);
            this.panelMenu.Name = "panelMenu";
            this.panelMenu.Size = new System.Drawing.Size(256, 544);
            this.panelMenu.TabIndex = 2;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            this.label2.Location = new System.Drawing.Point(3, 48);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(232, 40);
            this.label2.TabIndex = 15;
            this.label2.Text = "3. Экспорт и преобразование\nобъектов исходной БД";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Underline))));
            this.label4.Location = new System.Drawing.Point(11, 108);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(183, 36);
            this.label4.TabIndex = 14;
            this.label4.Text = "5. Загрузка объектов \n в целевую БД";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            this.label3.Location = new System.Drawing.Point(3, 88);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(240, 20);
            this.label3.TabIndex = 13;
            this.label3.Text = "4. Подключение к целевой БД";
            // 
            // lblPanelMenuConnectionFrom
            // 
            this.lblPanelMenuConnectionFrom.AutoSize = true;
            this.lblPanelMenuConnectionFrom.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.lblPanelMenuConnectionFrom.Location = new System.Drawing.Point(3, 8);
            this.lblPanelMenuConnectionFrom.Name = "lblPanelMenuConnectionFrom";
            this.lblPanelMenuConnectionFrom.Size = new System.Drawing.Size(247, 20);
            this.lblPanelMenuConnectionFrom.TabIndex = 10;
            this.lblPanelMenuConnectionFrom.Text = "1. Подключение к исходной БД";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            this.label1.Location = new System.Drawing.Point(3, 28);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(254, 20);
            this.label1.TabIndex = 11;
            this.label1.Text = "2. Выбор объектов исходной БД";
            // 
            // panelContent
            // 
            this.panelContent.BackColor = System.Drawing.Color.White;
            this.panelContent.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelContent.Controls.Add(this.txtSearchLoadScripts);
            this.panelContent.Controls.Add(this.label7);
            this.panelContent.Controls.Add(this.txtSearch);
            this.panelContent.Controls.Add(this.label6);
            this.panelContent.Controls.Add(this.label5);
            this.panelContent.Controls.Add(this.treeView);
            this.panelContent.Controls.Add(this.btnSelectFolder);
            this.panelContent.Controls.Add(this.btnExecuteScripts);
            this.panelContent.Controls.Add(this.checkedListBoxScripts);
            this.panelContent.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelContent.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.panelContent.Location = new System.Drawing.Point(256, 0);
            this.panelContent.Name = "panelContent";
            this.panelContent.Size = new System.Drawing.Size(497, 544);
            this.panelContent.TabIndex = 1;
            // 
            // txtSearchLoadScripts
            // 
            this.txtSearchLoadScripts.Location = new System.Drawing.Point(21, 355);
            this.txtSearchLoadScripts.Name = "txtSearchLoadScripts";
            this.txtSearchLoadScripts.Size = new System.Drawing.Size(341, 26);
            this.txtSearchLoadScripts.TabIndex = 19;
            this.txtSearchLoadScripts.TextChanged += new System.EventHandler(this.txtSearchLoadScripts_TextChanged);
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label7.Location = new System.Drawing.Point(12, 331);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(293, 20);
            this.label7.TabIndex = 18;
            this.label7.Text = "Поиск по загруженным SQL скриптам";
            // 
            // txtSearch
            // 
            this.txtSearch.Location = new System.Drawing.Point(16, 52);
            this.txtSearch.Name = "txtSearch";
            this.txtSearch.Size = new System.Drawing.Size(341, 26);
            this.txtSearch.TabIndex = 17;
            this.txtSearch.TextChanged += new System.EventHandler(this.txtSearch_TextChanged);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label6.Location = new System.Drawing.Point(7, 28);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(251, 20);
            this.label6.TabIndex = 16;
            this.label6.Text = "Поиск по объектам целевой БД";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Underline))));
            this.label5.Location = new System.Drawing.Point(112, 8);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(273, 18);
            this.label5.TabIndex = 15;
            this.label5.Text = "Загрузка объектов в целевую БД";
            // 
            // treeView
            // 
            this.treeView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            //this.treeView.CheckBoxes = true;
            this.treeView.Location = new System.Drawing.Point(10, 88);
            this.treeView.Name = "treeView";
            this.treeView.Size = new System.Drawing.Size(474, 209);
            this.treeView.TabIndex = 0;
            // 
            // btnSelectFolder
            // 
            this.btnSelectFolder.Location = new System.Drawing.Point(10, 484);
            this.btnSelectFolder.Name = "btnSelectFolder";
            this.btnSelectFolder.Size = new System.Drawing.Size(172, 55);
            this.btnSelectFolder.TabIndex = 1;
            this.btnSelectFolder.Text = "Выбрать папку c SQL скриптами";
            this.btnSelectFolder.Click += new System.EventHandler(this.btnSelectFolder_Click);
            // 
            // btnExecuteScripts
            // 
            this.btnExecuteScripts.Location = new System.Drawing.Point(279, 484);
            this.btnExecuteScripts.Name = "btnExecuteScripts";
            this.btnExecuteScripts.Size = new System.Drawing.Size(205, 55);
            this.btnExecuteScripts.TabIndex = 2;
            this.btnExecuteScripts.Text = "Выполнить скрипты в целевой БД";
            this.btnExecuteScripts.Click += new System.EventHandler(this.btnExecuteScripts_Click);
            // 
            // checkedListBoxScripts
            // 
            this.checkedListBoxScripts.Location = new System.Drawing.Point(10, 390);
            this.checkedListBoxScripts.Name = "checkedListBoxScripts";
            this.checkedListBoxScripts.Size = new System.Drawing.Size(470, 88);
            this.checkedListBoxScripts.TabIndex = 3;
            this.checkedListBoxScripts.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.checkedListBoxScripts_ItemCheck);
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
            // ImportToPosgreForm
            // 
            this.ClientSize = new System.Drawing.Size(753, 544);
            this.Controls.Add(this.panelResizeMenu);
            this.Controls.Add(this.panelContent);
            this.Controls.Add(this.panelMenu);
            this.Name = "ImportToPosgreForm";
            this.Text = "Main Layout";
            this.panelMenu.ResumeLayout(false);
            this.panelMenu.PerformLayout();
            this.panelContent.ResumeLayout(false);
            this.panelContent.PerformLayout();
            this.ResumeLayout(false);

        }


        #endregion

        private Label label3;
        private Label lblPanelMenuConnectionFrom;
        private Label label1;
        private TreeView treeView;
        private Label label4;
        private CheckedListBox checkedListBoxScripts;
        private Button btnSelectFolder;
        private Button btnExecuteScripts;
        private Label label5;
        private Label label2;
        private TextBox txtSearch;
        private Label label6;
        private TextBox txtSearchLoadScripts;
        private Label label7;
    }
}