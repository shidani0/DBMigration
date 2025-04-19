using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Text;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using Microsoft.SqlServer.Management.Smo;
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Sdk.Sfc;
using System.Linq;


namespace DBMigration
{
    public partial class Form2 : Form
    {
        private string connectionString;

        public Form2(string ip, string port, string dbName, string user, string password)
        {
            InitializeComponent();
            connectionString = $"Server={ip},{port};Database={dbName};User Id={user};Password={password};MultipleActiveResultSets=True;TrustServerCertificate=True;";
        }

        private void Form2_Load(object sender, EventArgs e)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    // --- ТАБЛИЦЫ И ТРИГГЕРЫ ---
                    TreeNode tablesRoot = new TreeNode("Таблицы");
                    var tableNames = new List<string>();

                    SqlCommand tableCommand = new SqlCommand(
                        "SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE';",
                        connection);
                    SqlDataReader tableReader = tableCommand.ExecuteReader();
                    while (tableReader.Read())
                    {
                        tableNames.Add(tableReader["TABLE_NAME"].ToString());
                    }
                    tableReader.Close();

                    foreach (string table in tableNames)
                    {
                        TreeNode tableNode = new TreeNode(table);

                        SqlCommand triggerCommand = new SqlCommand(@"
                            SELECT name FROM sys.triggers
                            WHERE parent_id = OBJECT_ID(@tableName);", connection);
                        triggerCommand.Parameters.AddWithValue("@tableName", table);

                        SqlDataReader triggerReader = triggerCommand.ExecuteReader();
                        while (triggerReader.Read())
                        {
                            string triggerName = triggerReader["name"].ToString();
                            TreeNode triggerNode = new TreeNode(triggerName);
                            tableNode.Nodes.Add(triggerNode);
                        }
                        triggerReader.Close();

                        tablesRoot.Nodes.Add(tableNode);
                    }

                    // --- СКАЛЯРНЫЕ ФУНКЦИИ ---
                    TreeNode scalarFuncsRoot = new TreeNode("Скалярные функции");

                    SqlCommand scalarFuncCmd = new SqlCommand(@"
                        SELECT SCHEMA_NAME(schema_id) AS schema_name, name
                        FROM sys.objects
                        WHERE type = 'FN';", connection);
                    SqlDataReader scalarFuncReader = scalarFuncCmd.ExecuteReader();
                    while (scalarFuncReader.Read())
                    {
                        string funcName = $"{scalarFuncReader["schema_name"]}.{scalarFuncReader["name"]}";
                        scalarFuncsRoot.Nodes.Add(new TreeNode(funcName));
                    }
                    scalarFuncReader.Close();

                    // --- ТАБЛИЧНЫЕ ФУНКЦИИ ---
                    TreeNode tableFuncsRoot = new TreeNode("Табличные функции");

                    SqlCommand tableFuncCmd = new SqlCommand(@"
                        SELECT SCHEMA_NAME(schema_id) AS schema_name, name
                        FROM sys.objects
                        WHERE type IN ('TF', 'IF');", connection);
                    SqlDataReader tableFuncReader = tableFuncCmd.ExecuteReader();
                    while (tableFuncReader.Read())
                    {
                        string funcName = $"{tableFuncReader["schema_name"]}.{tableFuncReader["name"]}";
                        tableFuncsRoot.Nodes.Add(new TreeNode(funcName));
                    }
                    tableFuncReader.Close();

                    treeView.Nodes.Add(tablesRoot);
                    treeView.Nodes.Add(scalarFuncsRoot);
                    treeView.Nodes.Add(tableFuncsRoot);

                    treeView.ExpandAll();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка подключения: " + ex.Message);
            }
        }

        private void TreeView_AfterCheck(object sender, TreeViewEventArgs e)
        {
            treeView.AfterCheck -= TreeView_AfterCheck;

            try
            {
                SetCheckedRecursive(e.Node, e.Node.Checked);
            }
            finally
            {
                treeView.AfterCheck += TreeView_AfterCheck;
            }
        }

        private void SetCheckedRecursive(TreeNode node, bool isChecked)
        {
            foreach (TreeNode child in node.Nodes)
            {
                child.Checked = isChecked;
                SetCheckedRecursive(child, isChecked);
            }
        }

        private string GetTableCreationScript(SqlConnection connection, string tableName)
        {
            try
            {
                // Временно возвращаем заглушку для отладки
                string script = $"-- Скрипт создания таблицы {tableName} (заглушка)\n";

                // Проверим через информацию о столбцах
                SqlCommand cmd = new SqlCommand(@"
            SELECT COLUMN_NAME, DATA_TYPE 
            FROM INFORMATION_SCHEMA.COLUMNS 
            WHERE TABLE_NAME = @tableName", connection);
                cmd.Parameters.AddWithValue("@tableName", tableName);

                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    script += $"-- Столбец: {reader["COLUMN_NAME"]}, Тип: {reader["DATA_TYPE"]}\n";
                }
                reader.Close();

                return script;
            }
            catch (Exception ex)
            {
                return $"Ошибка получения скрипта создания таблицы {tableName}: {ex.Message}";
            }
        }

        private string GetTableCreationScriptSMO(Server server, Database db, string tableName)
        {
            try
            {
                Table table = db.Tables[tableName, "dbo"]; // Схему можно сделать параметром, если надо
                if (table == null)
                    return $"-- Таблица '{tableName}' не найдена в базе данных.";

                Scripter scripter = new Scripter(server)
                {
                    Options =
            {
                ScriptDrops = false,
                WithDependencies = false,
                Indexes = true,
                DriAll = true,
                IncludeHeaders = true,
                SchemaQualify = true,
                ScriptSchema = true,
                ScriptData = false
            }
                };

                var script = scripter.Script(new Urn[] { table.Urn });
                return string.Join(Environment.NewLine, script.Cast<string>());
            }
            catch (Exception ex)
            {
                return $"-- Ошибка SMO при генерации скрипта для таблицы {tableName}: {ex.Message}";
            }
        }





        private string GetTriggerCreationScript(SqlConnection connection, string triggerName)
        {
            try
            {
                string script = "";
                SqlCommand command = new SqlCommand($"SELECT OBJECT_DEFINITION(OBJECT_ID('{triggerName}'))", connection);
                SqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    script += reader.GetString(0); // Добавление части скрипта
                }
                reader.Close();

                if (string.IsNullOrEmpty(script))
                {
                    script = $"-- Не удалось получить текст триггера: {triggerName}\n";
                }

                return script;
            }
            catch (Exception ex)
            {
                return $"Ошибка получения скрипта для триггера {triggerName}: {ex.Message}";
            }
        }


        private void ExportButton_Click(object sender, EventArgs e)
        {
            try
            {
                // Получаем параметры подключения
                SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder(connectionString);
                string serverName = builder.DataSource.Split(',')[0]; // IP/имя сервера
                string port = builder.DataSource.Contains(",") ? builder.DataSource.Split(',')[1] : ""; // порт, если есть
                string userName = builder.UserID;
                string password = builder.Password;
                string dbName = builder.InitialCatalog;

                // Создание подключения для SMO
                ServerConnection smoConnection = new ServerConnection($"{serverName},{port}", userName, password);
                string connectionStringSMO = $"Server={serverName},{port};User Id={userName};Password={password};";
                          //connectionString = $"Server={ip},{port};Database={dbName};User Id={user};Password={password};MultipleActiveResultSets=True;TrustServerCertificate=True;";

                Server smoServer = new Server(connectionStringSMO);
                Database smoDatabase = smoServer.Databases[dbName];

                StringBuilder sqlScripts = new StringBuilder();

                foreach (TreeNode rootNode in treeView.Nodes)
                {
                    if (rootNode.Text == "Таблицы")
                    {
                        foreach (TreeNode tableNode in rootNode.Nodes)
                        {
                            if (tableNode.Checked)
                            {
                                sqlScripts.AppendLine($"-- Таблица: {tableNode.Text}");
                                sqlScripts.AppendLine(GetTableCreationScriptSMO(smoServer, smoDatabase, tableNode.Text));
                                sqlScripts.AppendLine();

                                foreach (TreeNode triggerNode in tableNode.Nodes)
                                {
                                    if (triggerNode.Checked)
                                    {
                                        sqlScripts.AppendLine($"-- Триггер: {triggerNode.Text}");
                                        using (SqlConnection connection = new SqlConnection(connectionString))
                                        {
                                            connection.Open();
                                            sqlScripts.AppendLine(GetTriggerCreationScript(connection, triggerNode.Text));
                                        }
                                        sqlScripts.AppendLine();
                                    }
                                }
                            }
                        }
                    }
                }

                SaveFileDialog saveFileDialog = new SaveFileDialog
                {
                    Filter = "SQL Files|*.sql",
                    Title = "Сохранить SQL-скрипт"
                };

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    File.WriteAllText(saveFileDialog.FileName, sqlScripts.ToString());
                    MessageBox.Show("SQL скрипт успешно сохранён!");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при экспорте: " + ex.Message);
            }
        }



    }
}
