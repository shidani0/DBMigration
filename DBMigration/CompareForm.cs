using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Data.SqlClient;

namespace DBMigration
{
    public partial class CompareForm : Form
    {
        private readonly string referenceFilePath;
        private readonly string currentDbConnectionString;

        public CompareForm(string referenceFilePath, string connectionString)
        {
            InitializeComponent();
            this.referenceFilePath = referenceFilePath;
            this.currentDbConnectionString = connectionString;

            CompareDatabases();
        }

        private void CompareDatabases()
        {
            string referenceScript = File.ReadAllText(referenceFilePath);

            // Подключаемся к текущей базе
            using (SqlConnection conn = new SqlConnection(currentDbConnectionString))
            {
                conn.Open();

                // Пример: Получим список таблиц текущей БД
                List<string> currentTables = new List<string>();
                SqlCommand cmd = new SqlCommand("SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE'", conn);
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    currentTables.Add(reader.GetString(0));
                }

                reader.Close();

                // Примитивное сравнение: проверим, содержатся ли эти таблицы в referenceScript
                List<string> missingTables = currentTables
    .Where(table => referenceScript.IndexOf(table, StringComparison.OrdinalIgnoreCase) < 0)
    .ToList();


                // Отобразим результат
                lstDifferences.Items.Clear();
                if (missingTables.Count == 0)
                {
                    lstDifferences.Items.Add("Все таблицы присутствуют в эталонном файле.");
                }
                else
                {
                    lstDifferences.Items.Add("Отсутствующие таблицы в эталоне:");
                    foreach (string table in missingTables)
                        lstDifferences.Items.Add(table);
                }
            }
        }
    }

}
