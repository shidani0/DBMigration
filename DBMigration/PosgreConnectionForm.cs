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

        /*
        private void btnConnect_Click(object sender, EventArgs e)
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
         */
        private void btnConnect_Click(object sender, EventArgs e)
        {
            string connStr = $"Host={txtHost.Text};Port={txtPort.Text};Username={txtUser.Text};Password={txtPassword.Text};Database={txtDatabase.Text}";

            try
            {
                using (var conn = new NpgsqlConnection(connStr))
                {
                    conn.Open();
                    MessageBox.Show("Успешное подключение к PostgreSQL!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);


                    // Передаём строку подключения во вторую форму
                    ImportToPosgreForm importForm = new ImportToPosgreForm(connStr);
                    importForm.Show();

                    this.Hide(); // или Close(), если форма больше не нужна
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка подключения:\n" + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


    }
}
