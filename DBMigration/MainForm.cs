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
using System.Threading.Tasks;


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

        private async void MainForm_Load(object sender, EventArgs e)
        {
            progressBar.Visible = true;
            lblProgress.Visible = true;
            treeView.Visible = true;
            btnExport.Visible = false;


            await LoadTreeViewAsync();

            progressBar.Visible = false;
            lblProgress.Visible = false;
            btnExport.Visible = true;
            //treeView.Visible = true;
        }

        private int totalItems = 0;
        private int loadedItems = 0;

        private async Task LoadTreeViewAsync()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    totalItems = await GetTotalItemsAsync(connection);
                    loadedItems = 0;

                    TreeNode tablesRoot = new TreeNode("Таблицы");
                    treeView.Nodes.Add(tablesRoot); // Сразу добавляем корневой узел
                    treeView.Refresh();

                    // --- Загружаем таблицы и триггеры ---
                    var tableNames = new List<string>();
                    SqlCommand tableCommand = new SqlCommand(
                        "SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE';",
                        connection);
                    SqlDataReader tableReader = await tableCommand.ExecuteReaderAsync();
                    while (await tableReader.ReadAsync())
                    {
                        tableNames.Add(tableReader["TABLE_NAME"].ToString());
                    }
                    tableReader.Close();

                    foreach (string table in tableNames)
                    {
                        TreeNode tableNode = new TreeNode(table);
                        tablesRoot.Nodes.Add(tableNode);
                        IncrementProgress();
                        treeView.Refresh();
                        //await Task.Delay(0); // небольшая задержка для отрисовки

                        SqlCommand triggerCommand = new SqlCommand(@"
                    SELECT name FROM sys.triggers
                    WHERE parent_id = OBJECT_ID(@tableName);", connection);
                        triggerCommand.Parameters.AddWithValue("@tableName", table);

                        SqlDataReader triggerReader = await triggerCommand.ExecuteReaderAsync();
                        while (await triggerReader.ReadAsync())
                        {
                            string triggerName = triggerReader["name"].ToString();
                            TreeNode triggerNode = new TreeNode(triggerName);
                            tableNode.Nodes.Add(triggerNode);
                            IncrementProgress();
                            treeView.Refresh();
                            //await Task.Delay(1);
                        }
                        triggerReader.Close();
                    }

                    // --- Скалярные функции ---
                    TreeNode scalarFuncsRoot = new TreeNode("Скалярные функции");
                    treeView.Nodes.Add(scalarFuncsRoot);
                    treeView.Refresh();
                    //await Task.Delay(1);

                    SqlCommand scalarFuncCmd = new SqlCommand(@"
                SELECT SCHEMA_NAME(schema_id) AS schema_name, name
                FROM sys.objects
                WHERE type = 'FN';", connection);
                    SqlDataReader scalarFuncReader = await scalarFuncCmd.ExecuteReaderAsync();
                    while (await scalarFuncReader.ReadAsync())
                    {
                        string funcName = $"{scalarFuncReader["schema_name"]}.{scalarFuncReader["name"]}";
                        scalarFuncsRoot.Nodes.Add(new TreeNode(funcName));
                        IncrementProgress();
                        treeView.Refresh();
                        //await Task.Delay(1);
                    }
                    scalarFuncReader.Close();

                    // --- Табличные функции ---
                    TreeNode tableFuncsRoot = new TreeNode("Табличные функции");
                    treeView.Nodes.Add(tableFuncsRoot);
                    treeView.Refresh();
                    //await Task.Delay(1);

                    SqlCommand tableFuncCmd = new SqlCommand(@"
                SELECT SCHEMA_NAME(schema_id) AS schema_name, name
                FROM sys.objects
                WHERE type IN ('TF', 'IF');", connection);
                    SqlDataReader tableFuncReader = await tableFuncCmd.ExecuteReaderAsync();
                    while (await tableFuncReader.ReadAsync())
                    {
                        string funcName = $"{tableFuncReader["schema_name"]}.{tableFuncReader["name"]}";
                        tableFuncsRoot.Nodes.Add(new TreeNode(funcName));
                        IncrementProgress();
                        treeView.Refresh();
                        //await Task.Delay(1);
                    }
                    tableFuncReader.Close();
                    // --- Хранимые процедуры ---
                    TreeNode proceduresRoot = new TreeNode("Хранимые процедуры");
                    treeView.Nodes.Add(proceduresRoot);
                    treeView.Refresh();

                    SqlCommand procedureCmd = new SqlCommand(@"
                SELECT SCHEMA_NAME(schema_id) AS schema_name, name
                FROM sys.objects
                WHERE type = 'P';", connection);
                    SqlDataReader procedureReader = await procedureCmd.ExecuteReaderAsync();
                    while (await procedureReader.ReadAsync())
                    {
                        string procName = $"{procedureReader["schema_name"]}.{procedureReader["name"]}";
                        proceduresRoot.Nodes.Add(new TreeNode(procName));
                        IncrementProgress();
                        treeView.Refresh();
                    }
                    procedureReader.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки: " + ex.Message);
            }
        }



        private void IncrementProgress()
        {
            loadedItems++;
            int percent = (int)((loadedItems / (float)totalItems) * 100);
            progressBar.Value = Math.Min(percent, 100);
            lblProgress.Text = $"Загрузка {percent}%... {loadedItems}/{totalItems} ";
        }

        private async Task<int> GetTotalItemsAsync(SqlConnection connection)
        {
            int count = 0;

            // Таблицы
            SqlCommand tableCountCmd = new SqlCommand(
                "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE';", connection);
            count += (int)await tableCountCmd.ExecuteScalarAsync();

            // Триггеры
            SqlCommand triggerCountCmd = new SqlCommand(
                "SELECT COUNT(*) FROM sys.triggers;", connection);
            count += (int)await triggerCountCmd.ExecuteScalarAsync();

            // Скалярные функции
            SqlCommand scalarFuncCountCmd = new SqlCommand(
                "SELECT COUNT(*) FROM sys.objects WHERE type = 'FN';", connection);
            count += (int)await scalarFuncCountCmd.ExecuteScalarAsync();

            // Табличные функции
            SqlCommand tableFuncCountCmd = new SqlCommand(
                "SELECT COUNT(*) FROM sys.objects WHERE type IN ('TF', 'IF');", connection);
            count += (int)await tableFuncCountCmd.ExecuteScalarAsync();

            SqlCommand procedureCountCmd = new SqlCommand(
                "SELECT COUNT(*) FROM sys.objects WHERE type = 'P';", connection);
            count += (int)await procedureCountCmd.ExecuteScalarAsync();

            return count;
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

        public class DbObject
        {
            public string Name { get; set; }
            public string Script { get; set; }
            public string Type { get; set; }
        }

        private void btnExport_Click(object sender, EventArgs e)
        {
            List<DbObject> selectedScripts = new();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                foreach (TreeNode rootNode in treeView.Nodes)
                {
                    /*
                     
                    if (rootNode.Text == "Таблицы")
                    {
                        foreach (TreeNode tableNode in rootNode.Nodes)
                        {
                            if (tableNode.Checked)
                            {
                                string tableName = tableNode.Text;
                                string originalScript = GetTableCreationScript(connection, tableName);
                                selectedScripts.Add(new DbObject
                                {
                                    Name = tableName,
                                    Script = originalScript,
                                    Type = "Таблица"
                                });
                            }
                        }
                    }
                   */
                    if (rootNode.Text == "Таблицы")
                    {
                        foreach (TreeNode tableNode in rootNode.Nodes)
                        {
                            if (tableNode.Checked)
                            {
                                string tableName = tableNode.Text;
                                string originalScript = GetTableCreationScript(connection, tableName);
                                selectedScripts.Add(new DbObject
                                {
                                    Name = tableName,
                                    Script = originalScript,
                                    Type = "Таблица"
                                });
                            }

                            // Обработка дочерних узлов таблицы — например, триггеров
                            foreach (TreeNode childNode in tableNode.Nodes)
                            {
                                if (childNode.Checked)
                                {
                                    string triggerName = childNode.Text;
                                    string triggerScript = GetTriggerCreationScript(connection, triggerName);
                                    selectedScripts.Add(new DbObject
                                    {
                                        Name = triggerName,
                                        Script = triggerScript,
                                        Type = "Триггер"
                                    });
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
                                string funcName = funcNode.Text;
                                string originalScript = GetFunctionScript(connection, funcName);
                                selectedScripts.Add(new DbObject
                                {
                                    Name = funcName,
                                    Script = originalScript,
                                    Type = "Функция"
                                });
                            }
                        }
                    }
                    else if (rootNode.Text == "Хранимые процедуры")
                    {
                        foreach (TreeNode procNode in rootNode.Nodes)
                        {
                            if (procNode.Checked)
                            {
                                string procName = procNode.Text;
                                string originalScript = GetProcedureScript(connection, procName);
                                selectedScripts.Add(new DbObject
                                {
                                    Name = procName,
                                    Script = originalScript,
                                    Type = "Процедура"
                                });
                            }
                        }
                    }
                }
            }

            AnalizForm formAn = new AnalizForm(selectedScripts, connectionString);
            formAn.Show();
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


        /*
        private List<string> GetCheckedItems(TreeNodeCollection nodes)
        {
            List<string> checkedItems = new List<string>();

            foreach (TreeNode node in nodes)
            {
                if (node.Checked)
                    checkedItems.Add(node.Text);

                if (node.Nodes.Count > 0)
                    checkedItems.AddRange(GetCheckedItems(node.Nodes));
            }

            return checkedItems;
        }
         */


    }
}