using System;
using System.Drawing;
using System.Windows.Forms;
using Npgsql;

namespace DBMigration
{
    public partial class PosgreConnectionForm : Form
    {
        public PosgreConnectionForm()
        {
            InitializeComponent();
            AddPgConnectionControls();
        }

        private void panelResizeMenu_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                isResizing = true;
                lastMousePosition = e.Location;
            }
        }

        private void panelResizeMenu_MouseMove(object sender, MouseEventArgs e)
        {
            if (isResizing)
            {
                int deltaX = e.X - lastMousePosition.X;
                int newWidth = panelMenu.Width + deltaX;

                if (newWidth >= 100 && newWidth <= 400)
                    panelMenu.Width = newWidth;
            }
        }

        private void panelResizeMenu_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
                isResizing = false;
        }

        private void AddPgConnectionControls()
        {
            //Label lblHost = new Label { Text = "Host:", Location = new Point(20, 20), AutoSize = true };
            //txtHost = new TextBox { Location = new Point(120, 20), Width = 200, Text = "localhost" };
            //
            //Label lblPort = new Label { Text = "Port:", Location = new Point(20, 60), AutoSize = true };
            //txtPort = new TextBox { Location = new Point(120, 60), Width = 200, Text = "5432" };
            //
            //Label lblDatabase = new Label { Text = "Database:", Location = new Point(20, 100), AutoSize = true };
            //txtDatabase = new TextBox { Location = new Point(120, 100), Width = 200, Text = "STC_DB_DEMO" };
            //
            //Label lblUser = new Label { Text = "User:", Location = new Point(20, 140), AutoSize = true };
            //txtUser = new TextBox { Location = new Point(120, 140), Width = 200 , Text = "postgres" };
            //
            //Label lblPassword = new Label { Text = "Password:", Location = new Point(20, 180), AutoSize = true };
            //txtPassword = new TextBox { Location = new Point(120, 180), Width = 200, UseSystemPasswordChar = true, Text = "1" };

            btnConnect = new Button
            {
                Text = "Подключиться",
                Location = new Point(120, 220),
                Width = 200
            };
            btnConnect.Click += BtnConnect_Click;

            //panelContent.Controls.AddRange(new Control[]
            //{
            //    lblHost, txtHost,
            //    lblPort, txtPort,
            //    lblDatabase, txtDatabase,
            //    lblUser, txtUser,
            //    lblPassword, txtPassword,
            //    btnConnect
            //});
        }

        private void BtnConnect_Click(object sender, EventArgs e)
        {
            string connStr = $"Host={txtHost.Text};Port={txtPort.Text};Username={txtUser.Text};Password={txtPassword.Text};Database={txtDatabase.Text}";

            try
            {
                using (var conn = new NpgsqlConnection(connStr))
                {
                    conn.Open();
                    MessageBox.Show("Успешное подключение к PostgreSQL!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка подключения:\n" + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

    }
}
