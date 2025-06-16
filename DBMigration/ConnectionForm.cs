using System;
using System.Windows.Forms;

namespace DBMigration
{
    public partial class ConnectionForm : Form
    {
        public ConnectionForm()
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

                // Ограничение по минимальному и максимальному размеру меню
                if (newWidth >= 100 && newWidth <= 400)
                {
                    panelMenu.Width = newWidth;
                }
            }
        }

        private void panelResizeMenu_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                isResizing = false;
            }
        }



        private void btnConnect_Click(object sender, EventArgs e)
        {
            string ip = txtIp.Text;
            string port = txtPort.Text;
            string dbName = txtDbName.Text;
            string user = txtUser.Text;
            string password = txtPassword.Text;

            MainForm form2 = new MainForm(ip, port, dbName, user, password);

            form2.StartPosition = FormStartPosition.Manual;
            form2.Location = this.Location;

            // Показ новой формы
            form2.Show();

            // Скрыть текущую форму, если нужно
            this.Hide();
        }

        private void label3_Click(object sender, EventArgs e)
        {
            PosgreConnectionForm formPosgr = new PosgreConnectionForm();
            formPosgr.Show();
        }
    }
}
