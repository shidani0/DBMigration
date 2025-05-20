using System.Drawing;
using System.Windows.Forms;
using Npgsql;


namespace DBMigration
{
    partial class PosgreConnectionForm
    {
        private System.ComponentModel.IContainer components = null;

        private System.Windows.Forms.Panel panelMenu;
        private System.Windows.Forms.Panel panelResizeMenu;

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label lblPanelMenuConnectionFrom;

        private bool isResizing = false;
        private System.Drawing.Point lastMousePosition;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.panelMenu = new System.Windows.Forms.Panel();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.lblPanelMenuConnectionFrom = new System.Windows.Forms.Label();
            this.panelResizeMenu = new System.Windows.Forms.Panel();
            this.txtPassword = new System.Windows.Forms.TextBox();
            this.txtUser = new System.Windows.Forms.TextBox();
            this.txtDatabase = new System.Windows.Forms.TextBox();
            this.txtPort = new System.Windows.Forms.TextBox();
            this.txtHost = new System.Windows.Forms.TextBox();
            this.panelContent = new System.Windows.Forms.Panel();
            this.btnConnect = new System.Windows.Forms.Button();
            this.lblIp = new System.Windows.Forms.Label();
            this.lblPort = new System.Windows.Forms.Label();
            this.lblDbName = new System.Windows.Forms.Label();
            this.lblUser = new System.Windows.Forms.Label();
            this.lblPassword = new System.Windows.Forms.Label();
            this.panelMenu.SuspendLayout();
            this.panelContent.SuspendLayout();
            this.SuspendLayout();
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
            this.label4.Location = new System.Drawing.Point(3, 164);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(190, 40);
            this.label4.TabIndex = 16;
            this.label4.Text = "4.1. Выгрузка объектов \n в целевую БД";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Underline))));
            this.label3.Location = new System.Drawing.Point(11, 128);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(149, 36);
            this.label3.TabIndex = 0;
            this.label3.Text = "4. Подключение к\nцелевой БД";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            this.label2.Location = new System.Drawing.Point(3, 68);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(186, 60);
            this.label2.TabIndex = 1;
            this.label2.Text = "3.Сравнение структуры\nобъектов исходной\nи целевой БД";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            this.label1.Location = new System.Drawing.Point(3, 28);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(223, 40);
            this.label1.TabIndex = 2;
            this.label1.Text = "2. Выбор и анализ объектов\nисходной БД";
            // 
            // lblPanelMenuConnectionFrom
            // 
            this.lblPanelMenuConnectionFrom.AutoSize = true;
            this.lblPanelMenuConnectionFrom.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            this.lblPanelMenuConnectionFrom.Location = new System.Drawing.Point(3, 8);
            this.lblPanelMenuConnectionFrom.Name = "lblPanelMenuConnectionFrom";
            this.lblPanelMenuConnectionFrom.Size = new System.Drawing.Size(247, 20);
            this.lblPanelMenuConnectionFrom.TabIndex = 3;
            this.lblPanelMenuConnectionFrom.Text = "1. Подключение к исходной БД";
            // 
            // panelResizeMenu
            // 
            this.panelResizeMenu.BackColor = System.Drawing.Color.DarkGray;
            this.panelResizeMenu.Cursor = System.Windows.Forms.Cursors.VSplit;
            this.panelResizeMenu.Dock = System.Windows.Forms.DockStyle.Left;
            this.panelResizeMenu.Location = new System.Drawing.Point(277, 0);
            this.panelResizeMenu.Name = "panelResizeMenu";
            this.panelResizeMenu.Size = new System.Drawing.Size(5, 544);
            this.panelResizeMenu.TabIndex = 1;
            this.panelResizeMenu.MouseDown += new System.Windows.Forms.MouseEventHandler(this.panelResizeMenu_MouseDown);
            this.panelResizeMenu.MouseMove += new System.Windows.Forms.MouseEventHandler(this.panelResizeMenu_MouseMove);
            this.panelResizeMenu.MouseUp += new System.Windows.Forms.MouseEventHandler(this.panelResizeMenu_MouseUp);
            // 
            // txtPassword
            // 
            this.txtPassword.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            this.txtPassword.Location = new System.Drawing.Point(144, 145);
            this.txtPassword.Name = "txtPassword";
            this.txtPassword.Size = new System.Drawing.Size(200, 26);
            this.txtPassword.TabIndex = 9;
            this.txtPassword.Text = "1";
            this.txtPassword.UseSystemPasswordChar = true;
            // 
            // txtUser
            // 
            this.txtUser.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            this.txtUser.Location = new System.Drawing.Point(144, 110);
            this.txtUser.Name = "txtUser";
            this.txtUser.Size = new System.Drawing.Size(200, 26);
            this.txtUser.TabIndex = 7;
            this.txtUser.Text = "postgres";
            // 
            // txtDatabase
            // 
            this.txtDatabase.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            this.txtDatabase.Location = new System.Drawing.Point(144, 78);
            this.txtDatabase.Name = "txtDatabase";
            this.txtDatabase.Size = new System.Drawing.Size(200, 26);
            this.txtDatabase.TabIndex = 5;
            this.txtDatabase.Text = "STC_DB_DEMO";
            // 
            // txtPort
            // 
            this.txtPort.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            this.txtPort.Location = new System.Drawing.Point(144, 49);
            this.txtPort.Name = "txtPort";
            this.txtPort.Size = new System.Drawing.Size(200, 26);
            this.txtPort.TabIndex = 3;
            this.txtPort.Text = "5432";
            // 
            // txtHost
            // 
            this.txtHost.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            this.txtHost.Location = new System.Drawing.Point(144, 17);
            this.txtHost.Name = "txtHost";
            this.txtHost.Size = new System.Drawing.Size(200, 26);
            this.txtHost.TabIndex = 1;
            this.txtHost.Text = "localhost";
            // 
            // panelContent
            // 
            this.panelContent.BackColor = System.Drawing.Color.White;
            this.panelContent.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelContent.Controls.Add(this.btnConnect);
            this.panelContent.Controls.Add(this.lblIp);
            this.panelContent.Controls.Add(this.lblPort);
            this.panelContent.Controls.Add(this.lblDbName);
            this.panelContent.Controls.Add(this.lblUser);
            this.panelContent.Controls.Add(this.lblPassword);
            this.panelContent.Controls.Add(this.txtHost);
            this.panelContent.Controls.Add(this.txtPort);
            this.panelContent.Controls.Add(this.txtDatabase);
            this.panelContent.Controls.Add(this.txtUser);
            this.panelContent.Controls.Add(this.txtPassword);
            this.panelContent.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelContent.Location = new System.Drawing.Point(282, 0);
            this.panelContent.Name = "panelContent";
            this.panelContent.Size = new System.Drawing.Size(471, 544);
            this.panelContent.TabIndex = 0;
            // 
            // btnConnect
            // 
            this.btnConnect.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btnConnect.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            this.btnConnect.Location = new System.Drawing.Point(8, 177);
            this.btnConnect.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.btnConnect.Name = "btnConnect";
            this.btnConnect.Size = new System.Drawing.Size(451, 37);
            this.btnConnect.TabIndex = 16;
            this.btnConnect.Text = "Подключиться";
            this.btnConnect.Click += new System.EventHandler(this.btnConnect_Click);
            // 
            // lblIp
            // 
            this.lblIp.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            this.lblIp.Location = new System.Drawing.Point(4, 20);
            this.lblIp.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblIp.Name = "lblIp";
            this.lblIp.Size = new System.Drawing.Size(135, 23);
            this.lblIp.TabIndex = 11;
            this.lblIp.Text = "IP адрес:";
            // 
            // lblPort
            // 
            this.lblPort.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            this.lblPort.Location = new System.Drawing.Point(4, 53);
            this.lblPort.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblPort.Name = "lblPort";
            this.lblPort.Size = new System.Drawing.Size(135, 22);
            this.lblPort.TabIndex = 12;
            this.lblPort.Text = "Порт:";
            // 
            // lblDbName
            // 
            this.lblDbName.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            this.lblDbName.Location = new System.Drawing.Point(4, 84);
            this.lblDbName.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblDbName.Name = "lblDbName";
            this.lblDbName.Size = new System.Drawing.Size(135, 23);
            this.lblDbName.TabIndex = 13;
            this.lblDbName.Text = "Имя БД:";
            // 
            // lblUser
            // 
            this.lblUser.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            this.lblUser.Location = new System.Drawing.Point(4, 113);
            this.lblUser.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblUser.Name = "lblUser";
            this.lblUser.Size = new System.Drawing.Size(135, 26);
            this.lblUser.TabIndex = 14;
            this.lblUser.Text = "Пользователь:";
            // 
            // lblPassword
            // 
            this.lblPassword.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            this.lblPassword.Location = new System.Drawing.Point(4, 145);
            this.lblPassword.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblPassword.Name = "lblPassword";
            this.lblPassword.Size = new System.Drawing.Size(135, 26);
            this.lblPassword.TabIndex = 15;
            this.lblPassword.Text = "Пароль:";
            // 
            // PosgreConnectionForm
            // 
            this.ClientSize = new System.Drawing.Size(753, 544);
            this.Controls.Add(this.panelContent);
            this.Controls.Add(this.panelResizeMenu);
            this.Controls.Add(this.panelMenu);
            this.Name = "PosgreConnectionForm";
            this.Text = "PostgreSQL Connection";
            this.panelMenu.ResumeLayout(false);
            this.panelMenu.PerformLayout();
            this.panelContent.ResumeLayout(false);
            this.panelContent.PerformLayout();
            this.ResumeLayout(false);

        }
        private TextBox txtPassword;
        private TextBox txtUser;
        private TextBox txtDatabase;
        private TextBox txtPort;
        private TextBox txtHost;
        private Panel panelContent;
        private Label lblIp;
        private Label lblPort;
        private Label lblDbName;
        private Label lblUser;
        private Label lblPassword;
        private Button btnConnect;
        private Label label4;
    }
}
