using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SqlClient;

namespace DBMigration
{
    public partial class AnalizForm : Form
    {
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

        private List<string> selectedItems;

        private Dictionary<string, string> originalScripts;
        private string connectionString;

        public AnalizForm(Dictionary<string, string> scripts, string connStr)
        {
            InitializeComponent();
            originalScripts = scripts;
            connectionString = connStr;

            foreach (var key in originalScripts.Keys)
            {
                listBox1.Items.Add(key);
            }
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBox1.SelectedItem != null)
            {
                string selectedItem = listBox1.SelectedItem.ToString();

                // Здесь можно подгрузить или сформировать информацию об изменениях
                string info = GetItemChangeInfo(selectedItem);

                // Показываем окно с информацией
                MessageBox.Show(info, "Информация об изменении", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private string GetPostgresScript(string tableName)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    // Предположим, метод GetPostgresTableScript уже есть и принимает SqlConnection
                    MainForm dummy = new MainForm("", "", "", "", ""); // или вынести метод в утилиту
                    var postgresScript = dummy.GetType()
                        .GetMethod("GetPostgresTableScript", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                        .Invoke(dummy, new object[] { connection, tableName })?.ToString();

                    return postgresScript ?? "-- Не удалось получить PostgreSQL скрипт.";
                }
            }
            catch (Exception ex)
            {
                return $"Ошибка получения PostgreSQL скрипта: {ex.Message}";
            }
        }


        // Пример метода, возвращающего текст с информацией
        private string GetItemChangeInfo(string itemName)
        {
            string originalScript = originalScripts[itemName];
            string postgresScript = GetPostgresScript(itemName);

            // Простейшее сравнение
            if (originalScript == postgresScript)
                return $"Элемент {itemName} не изменён.";

            return $"Изменения для {itemName}:\n\n" +
                   $"[Оригинал SQL Server]:\n{originalScript}\n\n" +
                   $"[PostgreSQL]:\n{postgresScript}";
        }

        private void convertToPostgresButton_Click(object sender, EventArgs e)
        {

        }

        private void exportButton_Click(object sender, EventArgs e)
        {
            try
            {
                //ExportEachObjectToFolder();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при экспорте: " + ex.Message);
            }
        }



    }
}
