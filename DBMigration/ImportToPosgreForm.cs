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
using Npgsql;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace DBMigration
{
    public partial class ImportToPosgreForm : Form
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
        private string connectionString;

        public ImportToPosgreForm(string connStr)
        {
            InitializeComponent();
            connectionString = connStr;
            LoadDatabaseObjects();
        }

        private void ImportToPostgreForm_Load(object sender, EventArgs e)
        {
            LoadDatabaseObjects();
        }

        private void LoadDatabaseObjects()
        {
            try
            {
                using (var conn = new NpgsqlConnection(connectionString))
                {
                    conn.Open();

                    treeView.Nodes.Clear();

                    // Получаем таблицы
                    var tables = new Dictionary<string, TreeNode>();
                    using (var cmd = new NpgsqlCommand("SELECT table_name FROM information_schema.tables WHERE table_schema = 'public';", conn))
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string tableName = reader.GetString(0);
                            var tableNode = new TreeNode(tableName);
                            tableNode.Checked = false;
                            tables[tableName] = tableNode;
                        }
                    }

                    // Добавляем триггеры под таблицы
                    using (var cmd = new NpgsqlCommand("SELECT event_object_table, trigger_name FROM information_schema.triggers WHERE trigger_schema = 'public';", conn))
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string tableName = reader.GetString(0);
                            string triggerName = reader.GetString(1);

                            if (tables.TryGetValue(tableName, out TreeNode tableNode))
                            {
                                var triggerNode = new TreeNode("Триггер: " + triggerName);
                                triggerNode.Checked = false;
                                tableNode.Nodes.Add(triggerNode);
                            }
                        }
                    }

                    // Добавляем таблицы с триггерами в дерево
                    TreeNode tablesRoot = new TreeNode("Таблицы");
                    foreach (var tableNode in tables.Values)
                    {
                        tablesRoot.Nodes.Add(tableNode);
                    }
                    treeView.Nodes.Add(tablesRoot);

                    // Функции
                    TreeNode functionsNode = new TreeNode("Функции");
                    using (var cmd = new NpgsqlCommand(@"
                SELECT routine_name 
                FROM information_schema.routines 
                WHERE routine_schema = 'public' AND routine_type = 'FUNCTION';
            ", conn))
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string functionName = reader.GetString(0);
                            var funcNode = new TreeNode(functionName);
                            funcNode.Checked = false;
                            functionsNode.Nodes.Add(funcNode);
                        }
                    }
                    treeView.Nodes.Add(functionsNode);

                    // Процедуры (PostgreSQL 11+)
                    TreeNode proceduresNode = new TreeNode("Процедуры");
                    using (var cmd = new NpgsqlCommand(@"
                SELECT proname 
                FROM pg_proc 
                JOIN pg_namespace ns ON ns.oid = pg_proc.pronamespace 
                WHERE ns.nspname = 'public' AND prokind = 'p';
            ", conn))
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string procName = reader.GetString(0);
                            var procNode = new TreeNode(procName);
                            procNode.Checked = false;
                            proceduresNode.Nodes.Add(procNode);
                        }
                    }
                    treeView.Nodes.Add(proceduresNode);
                }

                //treeView.ExpandAll(); // Чтобы сразу развернуть дерево
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при загрузке объектов БД:\n" + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private List<string> sqlScriptPaths = new List<string>();
        private string selectedFolderPath = "";

        private void btnSelectFolder_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog folderDialog = new FolderBrowserDialog())
            {
                if (folderDialog.ShowDialog() == DialogResult.OK)
                {
                    selectedFolderPath = folderDialog.SelectedPath;
                    var files = Directory.GetFiles(selectedFolderPath, "*.sql");

                    sqlScriptPaths.Clear();
                    checkedListBoxScripts.Items.Clear();

                    foreach (var file in files)
                    {
                        sqlScriptPaths.Add(file);
                        checkedListBoxScripts.Items.Add(Path.GetFileName(file), true);
                    }

                    MessageBox.Show($"Загружено скриптов: {files.Length}", "Успешно", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        private void btnExecuteScripts_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(connectionString))
            {
                MessageBox.Show("Строка подключения не задана.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            List<string> skippedScripts = new List<string>();

            try
            {
                using (var conn = new Npgsql.NpgsqlConnection(connectionString))
                {
                    conn.Open();

                    for (int i = 0; i < checkedListBoxScripts.Items.Count; i++)
                    {
                        if (checkedListBoxScripts.GetItemChecked(i))
                        {
                            string filePath = sqlScriptPaths[i];
                            string scriptName = Path.GetFileName(filePath);
                            string script = File.ReadAllText(filePath);

                            try
                            {
                                using (var cmd = new Npgsql.NpgsqlCommand(script, conn))
                                {
                                    cmd.ExecuteNonQuery();
                                }
                            }
                            catch (PostgresException pgEx) when (pgEx.SqlState == "42P07")
                            {
                                // Ошибка: отношение уже существует — пропускаем
                                skippedScripts.Add(scriptName);
                                continue;
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show($"Ошибка при выполнении скрипта {scriptName}:\n{ex.Message}",
                                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                return; // Можно не прерывать весь процесс, если хочешь — убери return
                            }
                        }
                    }

                    string message = "Скрипты выполнены успешно.";
                    if (skippedScripts.Count > 0)
                    {
                        message += "\n\nСледующие объекты уже существовали и не были пересозданы:\n" +
                                   string.Join("\n", skippedScripts);
                    }

                    MessageBox.Show(message, "Результат", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при подключении:\n" + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

    }
}
