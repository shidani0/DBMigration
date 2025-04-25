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

        private void exportButton_Click(object sender, EventArgs e)
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


        private void ConvertToPostgres()
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                FolderBrowserDialog folderDialog = new FolderBrowserDialog
                {
                    Description = "Выберите папку для экспорта SQL-объектов для PostgreSQL"
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
                                             GetPostgresTableScript(connection, tableNode.Text);
                                grouped["Таблицы"].Add((tableNode.Text, script));
                                tablesCount++;

                                foreach (TreeNode triggerNode in tableNode.Nodes)
                                {
                                    if (triggerNode.Checked)
                                    {
                                        var triggerScript = $"-- Триггер: {triggerNode.Text}\n" +
                                                            GetPostgresTriggerScript(connection, triggerNode.Text);
                                        grouped["Таблицы"].Add((triggerNode.Text, triggerScript));
                                    }
                                }
                            }
                        }
                    }
                    /*
                    else if (rootNode.Text == "Скалярные функции" || rootNode.Text == "Табличные функции")
                    {
                        foreach (TreeNode funcNode in rootNode.Nodes)
                        {
                            if (funcNode.Checked)
                            {
                                var funcScript = GetPostgresFunctionScript(connection, funcNode.Text);
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
                                var procScript = GetPostgresProcedureScript(connection, procNode.Text);
                                grouped["Процедуры"].Add((procNode.Text, procScript));
                                procsCount++;
                            }
                        }
                    }
                    */
                }

                string folderName = $"Таблицы_{tablesCount}_Функции_{funcsCount}_Процедуры_{procsCount}__{date}";
                string exportFolderPath = Path.Combine(basePath, folderName);
                Directory.CreateDirectory(exportFolderPath);

                foreach (var group in grouped)
                {
                    foreach (var (name, script) in group.Value)
                    {
                        string safeName = string.Concat(name.Split(Path.GetInvalidFileNameChars()));
                        string filePath = Path.Combine(exportFolderPath, $"PG_{safeName}__{date}.sql");
                        File.WriteAllText(filePath, script);
                    }
                }

                MessageBox.Show($"Экспорт завершён!\nСоздано: {exportFolderPath}", "Успешно");
            }
        }


        private string GetPostgresTableScript(SqlConnection connection, string tableName)
        {
            try
            {
                string script = "-- PostgreSQL скрипт создания таблицы\n";

                // Получаем данные о колонках таблицы
                string tableScriptQuery = @"
        SELECT 
            COLUMN_NAME, DATA_TYPE, CHARACTER_MAXIMUM_LENGTH, 
            IS_NULLABLE, NUMERIC_PRECISION, NUMERIC_SCALE 
        FROM INFORMATION_SCHEMA.COLUMNS 
        WHERE TABLE_NAME = @tableName";

                using (SqlCommand cmd = new SqlCommand(tableScriptQuery, connection))
                {
                    cmd.Parameters.AddWithValue("@tableName", tableName);
                    SqlDataReader reader = cmd.ExecuteReader();

                    var columns = new List<string>();
                    while (reader.Read())
                    {
                        string columnDef = $"{reader["COLUMN_NAME"]} {ConvertToPostgresType(reader)}";
                        if (reader["IS_NULLABLE"].ToString() == "NO")
                        {
                            columnDef += " NOT NULL";
                        }
                        columns.Add(columnDef);
                    }
                    reader.Close();

                    script += $"CREATE TABLE {tableName} (\n" + string.Join(",\n", columns) + "\n);";
                }

                return script;
            }
            catch (Exception ex)
            {
                return $"-- Ошибка генерации скрипта для таблицы {tableName}: {ex.Message}\n";
            }
        }

        private string ConvertToPostgresType(SqlDataReader reader)
        {
            string dataType = reader["DATA_TYPE"].ToString();

            switch (dataType)
            {
                case "varchar":
                case "nvarchar":
                    return $"{dataType}({reader["CHARACTER_MAXIMUM_LENGTH"]})";
                case "decimal":
                case "numeric":
                    return $"{dataType}({reader["NUMERIC_PRECISION"]},{reader["NUMERIC_SCALE"]})";
                default:
                    return dataType;
            }
        }

        private string GetPostgresTriggerScript(SqlConnection connection, string triggerName)
        {
            try
            {
                string script = "-- PostgreSQL скрипт триггера\n";
                SqlCommand command = new SqlCommand($"SELECT OBJECT_DEFINITION(OBJECT_ID('{triggerName}'))", connection);
                SqlDataReader reader = command.ExecuteReader();

                if (reader.Read())
                {
                    // Конвертация текста триггера в PostgreSQL формат
                    script += reader.GetString(0).Replace("INSERTED", "NEW")
                                                .Replace("DELETED", "OLD");
                }
                reader.Close();

                return script;
            }
            catch (Exception ex)
            {
                return $"-- Ошибка генерации скрипта для триггера {triggerName}: {ex.Message}\n";
            }
        }



        private void convertToPostgresButton_Click(object sender, EventArgs e)
        {
            try
            {
                ConvertToPostgres();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при конвертации: " + ex.Message);
            }
        }

    }
}