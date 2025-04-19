using static System.Net.Mime.MediaTypeNames;
using System.Windows.Forms;
using System.Xml.Linq;
using System.Drawing;

namespace BDMigration
{
    partial class Form1
    {
        private System.ComponentModel.IContainer components = null;

        private Label lblIp;
        private TextBox txtIp;
        private Label lblPort;
        private TextBox txtPort;
        private Label lblDbName;
        private TextBox txtDbName;
        private Label lblUser;
        private TextBox txtUser;
        private Label lblPassword;
        private TextBox txtPassword;
        private Button btnConnect;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            lblIp = new Label();
            txtIp = new TextBox();
            lblPort = new Label();
            txtPort = new TextBox();
            lblDbName = new Label();
            txtDbName = new TextBox();
            lblUser = new Label();
            txtUser = new TextBox();
            lblPassword = new Label();
            txtPassword = new TextBox();
            btnConnect = new Button();

            SuspendLayout();

            // lblIp
            lblIp.Text = "IP адрес:";
            lblIp.Location = new Point(10, 10);
            lblIp.Size = new Size(100, 25);

            // txtIp
            txtIp.Name = "txtIp";
            txtIp.Location = new Point(120, 10);
            txtIp.Size = new Size(200, 31);
            txtIp.Text = "192.168.13.4";

            // lblPort
            lblPort.Text = "Порт:";
            lblPort.Location = new Point(10, 40);
            lblPort.Size = new Size(100, 25);

            // txtPort
            txtPort.Name = "txtPort";
            txtPort.Location = new Point(120, 40);
            txtPort.Size = new Size(200, 31);
            txtPort.Text = "1433";

            // lblDbName
            lblDbName.Text = "Имя БД:";
            lblDbName.Location = new Point(10, 70);
            lblDbName.Size = new Size(100, 25);

            // txtDbName
            txtDbName.Name = "txtDbName";
            txtDbName.Location = new Point(120, 70);
            txtDbName.Size = new Size(200, 31);
            txtDbName.Text = "STC_DB_DEMO";

            // lblUser
            lblUser.Text = "Пользователь:";
            lblUser.Location = new Point(10, 100);
            lblUser.Size = new Size(100, 25);

            // txtUser
            txtUser.Name = "txtUser";
            txtUser.Location = new Point(120, 100);
            txtUser.Size = new Size(200, 31);
            txtUser.Text = "sa";

            // lblPassword
            lblPassword.Text = "Пароль:";
            lblPassword.Location = new Point(10, 130);
            lblPassword.Size = new Size(100, 25);

            // txtPassword
            txtPassword.Name = "txtPassword";
            txtPassword.Location = new Point(120, 130);
            txtPassword.Size = new Size(200, 31);
            txtPassword.PasswordChar = '*';
            txtPassword.Text = "stc12345!";

            // btnConnect
            btnConnect.Text = "Подключиться";
            btnConnect.Location = new Point(120, 170);
            btnConnect.Size = new Size(200, 35);
            btnConnect.Click += btnConnect_Click;

            // Form1
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(400, 230);
            Controls.Add(lblIp);
            Controls.Add(txtIp);
            Controls.Add(lblPort);
            Controls.Add(txtPort);
            Controls.Add(lblDbName);
            Controls.Add(txtDbName);
            Controls.Add(lblUser);
            Controls.Add(txtUser);
            Controls.Add(lblPassword);
            Controls.Add(txtPassword);
            Controls.Add(btnConnect);
            Name = "Form1";
            Text = "Подключение к базе данных";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
    }
}
