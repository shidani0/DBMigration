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
    public partial class MainForm : Form
    {
        private string connectionString;

        public MainForm(string ip, string port, string dbName, string user, string password)
        {
            InitializeComponent();
            connectionString = $"Server={ip},{port};Database={dbName};User Id={user};Password={password};MultipleActiveResultSets=True;TrustServerCertificate=True;";
        }

        private void MainForm_Load(object sender, EventArgs e)
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

        private string GetTableCreationScript2(SqlConnection connection, string tableName)
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

        private string GetTableCreationScript(SqlConnection connection, string tableName)
        {
            try
            {
                string script = "";

                // 1. Получение скрипта создания таблицы с PRIMARY KEY
                string tableScriptQuery = @"
WITH ColumnDefinitions AS (
    SELECT 
        TABLE_SCHEMA,
        TABLE_NAME,
        COLUMN_NAME,
        DATA_TYPE,
        CHARACTER_MAXIMUM_LENGTH,
        NUMERIC_PRECISION,
        NUMERIC_SCALE,
        IS_NULLABLE,
        ORDINAL_POSITION
    FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = @tableName
)
SELECT 'CREATE TABLE ' + QUOTENAME(TABLE_SCHEMA) + '.' + QUOTENAME(TABLE_NAME) + ' (' +
    STUFF((
        SELECT ', ' + QUOTENAME(COLUMN_NAME) + ' ' +
               DATA_TYPE +
               CASE 
                   WHEN CHARACTER_MAXIMUM_LENGTH IS NOT NULL AND DATA_TYPE IN ('char', 'varchar', 'nchar', 'nvarchar') 
                       THEN '(' + 
                            CASE 
                                WHEN CHARACTER_MAXIMUM_LENGTH = -1 THEN 'MAX' 
                                ELSE CAST(CHARACTER_MAXIMUM_LENGTH AS VARCHAR) 
                            END + ')'
                   WHEN DATA_TYPE IN ('decimal', 'numeric') 
                       THEN '(' + CAST(NUMERIC_PRECISION AS VARCHAR) + ',' + CAST(NUMERIC_SCALE AS VARCHAR) + ')'
                   ELSE ''
               END + ' ' +
               CASE 
                   WHEN IS_NULLABLE = 'NO' THEN 'NOT NULL' 
                   ELSE 'NULL' 
               END
        FROM ColumnDefinitions AS C2
        WHERE C2.TABLE_NAME = C1.TABLE_NAME AND C2.TABLE_SCHEMA = C1.TABLE_SCHEMA
        ORDER BY ORDINAL_POSITION
        FOR XML PATH(''), TYPE).value('.', 'NVARCHAR(MAX)'), 1, 2, '') +

    ISNULL((
        SELECT ', CONSTRAINT ' + QUOTENAME(kcu.CONSTRAINT_NAME) + ' PRIMARY KEY (' +
               STUFF((
                   SELECT ', ' + QUOTENAME(kcu2.COLUMN_NAME)
                   FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS tc2
                   JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE kcu2 
                       ON tc2.CONSTRAINT_NAME = kcu2.CONSTRAINT_NAME
                   WHERE tc2.TABLE_NAME = C1.TABLE_NAME 
                     AND tc2.CONSTRAINT_TYPE = 'PRIMARY KEY'
                   ORDER BY kcu2.ORDINAL_POSITION
                   FOR XML PATH(''), TYPE).value('.', 'NVARCHAR(MAX)'), 1, 2, '') + ')'
        FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS tc
        JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE kcu 
            ON tc.CONSTRAINT_NAME = kcu.CONSTRAINT_NAME
        WHERE tc.TABLE_NAME = C1.TABLE_NAME 
          AND tc.CONSTRAINT_TYPE = 'PRIMARY KEY'
          AND tc.TABLE_SCHEMA = C1.TABLE_SCHEMA
        FOR XML PATH(''), TYPE).value('.', 'NVARCHAR(MAX)'), ''
    ) +
    ')' AS CreateTableScript
FROM ColumnDefinitions AS C1
GROUP BY TABLE_SCHEMA, TABLE_NAME;
";

                using (SqlCommand cmd = new SqlCommand(tableScriptQuery, connection))
                {
                    cmd.Parameters.AddWithValue("@tableName", tableName);
                    object result = cmd.ExecuteScalar();
                    if (result != null)
                    {
                        script = result.ToString();
                    }
                }

                // 2. Добавление скриптов внешних ключей
                string fkScriptQuery = @"
SELECT 
    'ALTER TABLE ' + QUOTENAME(sch1.name) + '.' + QUOTENAME(tab1.name) + 
    ' ADD CONSTRAINT ' + QUOTENAME(fk.name) + 
    ' FOREIGN KEY (' + QUOTENAME(col1.name) + ')' +
    ' REFERENCES ' + QUOTENAME(sch2.name) + '.' + QUOTENAME(tab2.name) + 
    ' (' + QUOTENAME(col2.name) + ');' AS ForeignKeyScript
FROM sys.foreign_keys fk
JOIN sys.foreign_key_columns fkc 
    ON fk.object_id = fkc.constraint_object_id
JOIN sys.tables tab1 
    ON fkc.parent_object_id = tab1.object_id
JOIN sys.schemas sch1 
    ON tab1.schema_id = sch1.schema_id
JOIN sys.columns col1 
    ON fkc.parent_object_id = col1.object_id AND fkc.parent_column_id = col1.column_id
JOIN sys.tables tab2 
    ON fkc.referenced_object_id = tab2.object_id
JOIN sys.schemas sch2 
    ON tab2.schema_id = sch2.schema_id
JOIN sys.columns col2 
    ON fkc.referenced_object_id = col2.object_id AND fkc.referenced_column_id = col2.column_id
WHERE tab1.name = @tableName;
";

                using (SqlCommand fkCmd = new SqlCommand(fkScriptQuery, connection))
                {
                    fkCmd.Parameters.AddWithValue("@tableName", tableName);
                    using (SqlDataReader reader = fkCmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            script += "\n" + reader["ForeignKeyScript"].ToString();
                        }
                    }
                }

                return script;
            }
            catch (Exception ex)
            {
                return $"-- Ошибка генерации скрипта для таблицы {tableName}: {ex.Message}";
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

        private string GetFunctionScript(SqlConnection connection, string functionName)
        {
            try
            {
                string script = "";
                SqlCommand command = new SqlCommand($"SELECT OBJECT_DEFINITION(OBJECT_ID(@funcName))", connection);
                command.Parameters.AddWithValue("@funcName", functionName);
                SqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    script += reader.GetString(0);
                }
                reader.Close();

                if (string.IsNullOrEmpty(script))
                {
                    script = $"-- Не удалось получить текст функции: {functionName}\n";
                }

                return script;
            }
            catch (Exception ex)
            {
                return $"-- Ошибка получения функции {functionName}: {ex.Message}";
            }
        }

        private string GetProcedureScript(SqlConnection connection, string procedureName)
        {
            try
            {
                string script = "";
                SqlCommand command = new SqlCommand($"SELECT OBJECT_DEFINITION(OBJECT_ID(@procName))", connection);
                command.Parameters.AddWithValue("@procName", procedureName);
                SqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    script += reader.GetString(0);
                }
                reader.Close();

                if (string.IsNullOrEmpty(script))
                {
                    script = $"-- Не удалось получить текст процедуры: {procedureName}\n";
                }

                return script;
            }
            catch (Exception ex)
            {
                return $"-- Ошибка получения процедуры {procedureName}: {ex.Message}";
            }
        }

        private string GenerateFileNameFromTreeView()
        {
            List<string> selectedItems = new List<string>();

            foreach (TreeNode rootNode in treeView.Nodes)
            {
                foreach (TreeNode node in rootNode.Nodes)
                {
                    if (node.Checked)
                    {
                        selectedItems.Add(node.Text);
                    }
                }
            }

            if (selectedItems.Count == 0)
                return "EmptyExport.sql";

            // Удалим недопустимые символы и ограничим длину
            string fileName = string.Join("_", selectedItems
                .Select(name => string.Concat(name.Split(Path.GetInvalidFileNameChars())))
                .Take(5)); // Ограничим до первых 5 названий

            if (fileName.Length > 50)
                fileName = fileName.Substring(0, 50);

            return fileName + ".sql";
        }


        private void ExportEachObjectToFolder()
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                FolderBrowserDialog folderDialog = new FolderBrowserDialog
                {
                    Description = "Выберите папку для экспорта SQL-объектов"
                };

                if (folderDialog.ShowDialog() != DialogResult.OK)
                    return;

                string basePath = folderDialog.SelectedPath;
                string date = DateTime.Now.ToString("dd-MM-yyyy");

                int tablesCount = 0, funcsCount = 0, procsCount = 0;

                Dictionary<string, List<(string name, string content)>> grouped = new();

                grouped["Таблицы"] = new List<(string, string)>();
                grouped["Функции"] = new List<(string, string)>();
                grouped["Процедуры"] = new List<(string, string)>();

                foreach (TreeNode rootNode in treeView.Nodes)
                {
                    if (rootNode.Text == "Таблицы")
                    {
                        foreach (TreeNode tableNode in rootNode.Nodes)
                        {
                            if (tableNode.Checked)
                            {
                                var script = $"-- Таблица: {tableNode.Text}\n" +
                                             GetTableCreationScript(connection, tableNode.Text);
                                grouped["Таблицы"].Add((tableNode.Text, script));
                                tablesCount++;

                                foreach (TreeNode triggerNode in tableNode.Nodes)
                                {
                                    if (triggerNode.Checked)
                                    {
                                        var triggerScript = $"-- Триггер: {triggerNode.Text}\n" +
                                                            GetTriggerCreationScript(connection, triggerNode.Text);
                                        grouped["Таблицы"].Add((triggerNode.Text, triggerScript));
                                    }
                                }
                            }
                        }
                    }
                    else if (rootNode.Text == "Скалярные функции" || rootNode.Text == "Табличные функции")
                    {
                        foreach (TreeNode funcNode in rootNode.Nodes)
                        {
                            if (funcNode.Checked)
                            {
                                var funcScript = GetFunctionScript(connection, funcNode.Text);
                                grouped["Функции"].Add((funcNode.Text, funcScript));
                                funcsCount++;
                            }
                        }
                    }
                    else if (rootNode.Text == "Процедуры")
                    {
                        foreach (TreeNode procNode in rootNode.Nodes)
                        {
                            if (procNode.Checked)
                            {
                                var procScript = GetProcedureScript(connection, procNode.Text);
                                grouped["Процедуры"].Add((procNode.Text, procScript));
                                procsCount++;
                            }
                        }
                    }
                }

                // Название родительской папки
                string folderName = $"Таблицы_{tablesCount}_Функции_{funcsCount}_Процедуры_{procsCount}__{date}";
                string exportFolderPath = Path.Combine(basePath, folderName);
                Directory.CreateDirectory(exportFolderPath);

                foreach (var group in grouped)
                {
                    foreach (var (name, script) in group.Value)
                    {
                        string safeName = string.Concat(name.Split(Path.GetInvalidFileNameChars()));
                        string filePath = Path.Combine(exportFolderPath, $"{safeName}__{date}.sql");
                        File.WriteAllText(filePath, script);
                    }
                }

                MessageBox.Show($"Экспорт завершён!\nСоздано: {exportFolderPath}", "Успешно");
            }
        }

        private void ExportButton_Click(object sender, EventArgs e)
        {
            try
            {
                ExportEachObjectToFolder();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при экспорте: " + ex.Message);
            }
        }


        private void ExportButton_Click1(object sender, EventArgs e)
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

        private void ExportButton_Click2(object sender, EventArgs e)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    StringBuilder sqlScripts = new StringBuilder();

                    foreach (TreeNode rootNode in treeView.Nodes)
                    {
                        // --- ТАБЛИЦЫ И ТРИГГЕРЫ ---
                        if (rootNode.Text == "Таблицы")
                        {
                            foreach (TreeNode tableNode in rootNode.Nodes)
                            {
                                if (tableNode.Checked)
                                {
                                    sqlScripts.AppendLine($"-- Таблица: {tableNode.Text}");
                                    sqlScripts.AppendLine(GetTableCreationScript(connection, tableNode.Text));
                                    sqlScripts.AppendLine();

                                    foreach (TreeNode triggerNode in tableNode.Nodes)
                                    {
                                        if (triggerNode.Checked)
                                        {
                                            sqlScripts.AppendLine($"-- Триггер: {triggerNode.Text}");
                                            sqlScripts.AppendLine(GetTriggerCreationScript(connection, triggerNode.Text));
                                            sqlScripts.AppendLine();
                                        }
                                    }
                                }
                            }
                        }

                        // --- СКАЛЯРНЫЕ ФУНКЦИИ ---
                        else if (rootNode.Text == "Скалярные функции")
                        {
                            foreach (TreeNode funcNode in rootNode.Nodes)
                            {
                                if (funcNode.Checked)
                                {
                                    sqlScripts.AppendLine($"-- Скалярная функция: {funcNode.Text}");
                                    sqlScripts.AppendLine(GetFunctionScript(connection, funcNode.Text));
                                    sqlScripts.AppendLine();
                                }
                            }
                        }

                        // --- ТАБЛИЧНЫЕ ФУНКЦИИ ---
                        else if (rootNode.Text == "Табличные функции")
                        {
                            foreach (TreeNode funcNode in rootNode.Nodes)
                            {
                                if (funcNode.Checked)
                                {
                                    sqlScripts.AppendLine($"-- Табличная функция: {funcNode.Text}");
                                    sqlScripts.AppendLine(GetFunctionScript(connection, funcNode.Text));
                                    sqlScripts.AppendLine();
                                }
                            }
                        }

                        // --- ПРОЦЕДУРЫ ---
                        else if (rootNode.Text == "Процедуры")
                        {
                            foreach (TreeNode procNode in rootNode.Nodes)
                            {
                                if (procNode.Checked)
                                {
                                    sqlScripts.AppendLine($"-- Процедура: {procNode.Text}");
                                    sqlScripts.AppendLine(GetProcedureScript(connection, procNode.Text));
                                    sqlScripts.AppendLine();
                                }
                            }
                        }
                    }

                    string defaultFileName = GenerateFileNameFromTreeView();

                    SaveFileDialog saveFileDialog = new SaveFileDialog
                    {
                        Filter = "SQL Files|*.sql",
                        Title = "Save SQL Script",
                        FileName = defaultFileName
                    };

                    if (saveFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        File.WriteAllText(saveFileDialog.FileName, sqlScripts.ToString());
                        MessageBox.Show("SQL скрипт сохранен!");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при экспорте: " + ex.Message);
            }
        }
    }
}