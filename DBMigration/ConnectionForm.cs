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

        private void btnConnect_Click(object sender, EventArgs e)
        {
            string ip = txtIp.Text;
            string port = txtPort.Text;
            string dbName = txtDbName.Text;
            string user = txtUser.Text;
            string password = txtPassword.Text;

            MainForm form2 = new MainForm(ip, port, dbName, user, password);
            form2.Show();
        }
    }
}
