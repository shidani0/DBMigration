using System;
using System.Windows.Forms;
using DBMigration;

namespace BDMigration
{
    public partial class Form1 : Form
    {
        public Form1()
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

            Form2 form2 = new Form2(ip, port, dbName, user, password);
            form2.Show();
        }
    }
}
