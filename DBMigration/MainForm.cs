using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Windows.Forms;
using System.Threading.Tasks;
using System.Threading;
using System.Drawing;
using System.Text;
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

        private TreeNode[] originalNodes;


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
                    treeView.Nodes.Add(tablesRoot);
                    treeView.Refresh();

                    // Получаем список таблиц
                    var tableNames = new List<string>();
                    SqlCommand tableCommand = new SqlCommand(
                        "SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE' ORDER BY TABLE_NAME ASC;",
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

                        // Структура таблицы (поля) - без CheckBox
                        TreeNode structureNode = CreateNonCheckableNode("Структура");
                        tableNode.Nodes.Add(structureNode);

                        // Запрос для получения информации о столбцах
                        SqlCommand columnCommand = new SqlCommand(@"
                            SELECT 
                                c.COLUMN_NAME, 
                                c.DATA_TYPE, 
                                c.IS_NULLABLE, 
                                c.CHARACTER_MAXIMUM_LENGTH,
                                CASE 
                                    WHEN pk.CONSTRAINT_NAME IS NOT NULL THEN 'PK'
                                    ELSE ''
                                END AS IsPrimaryKey,
                                CASE 
                                    WHEN fk.CONSTRAINT_NAME IS NOT NULL THEN 'FK'
                                    ELSE ''
                                END AS IsForeignKey
                            FROM INFORMATION_SCHEMA.COLUMNS c
                            LEFT JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE pk 
                                ON c.TABLE_NAME = pk.TABLE_NAME 
                                AND c.COLUMN_NAME = pk.COLUMN_NAME 
                                AND pk.CONSTRAINT_NAME LIKE 'PK_%'
                            LEFT JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE fk 
                                ON c.TABLE_NAME = fk.TABLE_NAME 
                                AND c.COLUMN_NAME = fk.COLUMN_NAME 
                                AND fk.CONSTRAINT_NAME LIKE 'FK_%'
                            WHERE c.TABLE_NAME = @tableName;", connection);
                        columnCommand.Parameters.AddWithValue("@tableName", table);

                        SqlDataReader columnReader = await columnCommand.ExecuteReaderAsync();
                        while (await columnReader.ReadAsync())
                        {
                            string columnName = columnReader["COLUMN_NAME"].ToString();
                            string dataType = columnReader["DATA_TYPE"].ToString();
                            string isNullable = columnReader["IS_NULLABLE"].ToString();
                            string maxLength = columnReader["CHARACTER_MAXIMUM_LENGTH"] != DBNull.Value
                                ? $"({columnReader["CHARACTER_MAXIMUM_LENGTH"]})"
                                : "";
                            string isPrimaryKey = columnReader["IsPrimaryKey"].ToString();
                            string isForeignKey = columnReader["IsForeignKey"].ToString();

                            // Формирование строки с информацией о столбце, включая PK/FK
                            string columnInfo = $"{columnName} - {dataType}{maxLength} {(isNullable == "YES" ? "NULL" : "NOT NULL")}" +
                                               $"{(isPrimaryKey == "PK" ? " PK" : "")}{(isForeignKey == "FK" ? " FK" : "")}";
                            TreeNode columnNode = CreateNonCheckableNode(columnInfo);
                            structureNode.Nodes.Add(columnNode);
                        }
                        columnReader.Close();

                        // Внешние ключи (таблицы, на которые ссылается) - без CheckBox
                        TreeNode referencesNode = CreateNonCheckableNode("Ссылается на");
                        tableNode.Nodes.Add(referencesNode);

                        SqlCommand fkCommand = new SqlCommand(@"
                    SELECT 
                        OBJECT_NAME(fk.referenced_object_id) AS referenced_table,
                        c1.name AS referencing_column,
                        c2.name AS referenced_column
                    FROM 
                        sys.foreign_keys fk
                        INNER JOIN sys.foreign_key_columns fkc ON fk.object_id = fkc.constraint_object_id
                        INNER JOIN sys.columns c1 ON fkc.parent_object_id = c1.object_id AND fkc.parent_column_id = c1.column_id
                        INNER JOIN sys.columns c2 ON fkc.referenced_object_id = c2.object_id AND fkc.referenced_column_id = c2.column_id
                    WHERE 
                        OBJECT_NAME(fk.parent_object_id) = @tableName
                    ORDER BY 
                        referenced_table;", connection);
                        fkCommand.Parameters.AddWithValue("@tableName", table);

                        SqlDataReader fkReader = await fkCommand.ExecuteReaderAsync();
                        while (await fkReader.ReadAsync())
                        {
                            string refTable = fkReader["referenced_table"].ToString();
                            string referencingColumn = fkReader["referencing_column"].ToString();
                            string referencedColumn = fkReader["referenced_column"].ToString();
                            TreeNode refNode = CreateNonCheckableNode($"{refTable} ({referencingColumn} → {referencedColumn})");
                            referencesNode.Nodes.Add(refNode);
                        }
                        fkReader.Close();

                        // Триггеры (с CheckBox)
                        SqlCommand triggerCommand = new SqlCommand(@"
                    SELECT name FROM sys.triggers
                    WHERE parent_id = OBJECT_ID(@tableName);", connection);
                        triggerCommand.Parameters.AddWithValue("@tableName", table);

                        SqlDataReader triggerReader = await triggerCommand.ExecuteReaderAsync();
                        TreeNode triggersNode = new TreeNode("Триггеры");
                        bool hasTriggers = false;
                        while (await triggerReader.ReadAsync())
                        {
                            string triggerName = triggerReader["name"].ToString();
                            triggersNode.Nodes.Add(new TreeNode(triggerName));
                            hasTriggers = true;
                            IncrementProgress();
                        }
                        triggerReader.Close();

                        if (hasTriggers)
                            tableNode.Nodes.Add(triggersNode);

                        treeView.Refresh();
                    }

                    // --- Скалярные функции ---
                    TreeNode scalarFuncsRoot = new TreeNode("Скалярные функции");
                    treeView.Nodes.Add(scalarFuncsRoot);
                    treeView.Refresh();

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
                    }
                    scalarFuncReader.Close();

                    // --- Табличные функции ---
                    TreeNode tableFuncsRoot = new TreeNode("Табличные функции");
                    treeView.Nodes.Add(tableFuncsRoot);
                    treeView.Refresh();

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

                ConnectionForm connect = new ConnectionForm();
                connect.Show();
                this.Hide();
            }

            originalNodes = new TreeNode[treeView.Nodes.Count];
            treeView.Nodes.CopyTo(originalNodes, 0);
        }

        private TreeNode CreateNonCheckableNode(string text)
        {
            var node = new TreeNode(text)
            {
                ForeColor = SystemColors.GrayText,
                Checked = false
            };
            return node;
        }
        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
            string searchText = txtSearch.Text.ToLower();

            // 1. Сохраняем список выбранных элементов
            var selectedNodes = new List<string>();
            if (originalNodes != null)
            {
                SaveSelectedNodes(treeView.Nodes, selectedNodes);
            }

            treeView.BeginUpdate();
            treeView.Nodes.Clear();

            // 2. Применяем фильтрацию
            foreach (TreeNode rootNode in originalNodes)
            {
                TreeNode newNode = FilterNodeBySecondLevel(rootNode, searchText);
                if (newNode != null)
                    treeView.Nodes.Add(newNode);
            }

            // 3. Восстанавливаем выбранные элементы
            if (!string.IsNullOrEmpty(searchText))
            {
                RestoreSelectedNodes(treeView.Nodes, selectedNodes);
            }

            treeView.EndUpdate();
        }

        private TreeNode FilterNodeBySecondLevel(TreeNode originalNode, string searchText)
        {
            // Всегда включаем корневой узел
            TreeNode newNode = new TreeNode(originalNode.Text)
            {
                Tag = originalNode.Tag,
                ImageIndex = originalNode.ImageIndex,
                SelectedImageIndex = originalNode.SelectedImageIndex
            };

            // Если строка поиска пустая, копируем все узлы
            if (string.IsNullOrEmpty(searchText))
            {
                CopyChildNodes(originalNode, newNode);
                return newNode;
            }

            bool hasVisibleChildren = false;

            // Проверяем только узлы второго уровня (детей корневого узла)
            foreach (TreeNode secondLevelNode in originalNode.Nodes)
            {
                bool secondLevelMatches = secondLevelNode.Text.ToLower().Contains(searchText);

                // Если узел второго уровня совпадает, добавляем его и всех его детей
                if (secondLevelMatches)
                {
                    TreeNode newSecondLevelNode = CloneTreeNode(secondLevelNode);
                    newNode.Nodes.Add(newSecondLevelNode);
                    hasVisibleChildren = true;
                }
                else
                {
                    // Если не совпадает, проверяем есть ли совпадения в детях (третьем уровне и ниже)
                    TreeNode filteredNode = FilterChildren(secondLevelNode, searchText);
                    if (filteredNode != null)
                    {
                        newNode.Nodes.Add(filteredNode);
                        hasVisibleChildren = true;
                    }
                }
            }

            // Если у корневого узла есть видимые дети, оставляем его
            if (hasVisibleChildren || originalNode.Text.ToLower().Contains(searchText))
            {
                return newNode;
            }

            return null;
        }

        private TreeNode FilterChildren(TreeNode originalNode, string searchText)
        {
            // Проверяем, есть ли совпадения в текущем узле или его потомках
            bool selfMatches = originalNode.Text.ToLower().Contains(searchText);
            bool childMatches = false;

            TreeNode newNode = new TreeNode(originalNode.Text)
            {
                Tag = originalNode.Tag,
                ImageIndex = originalNode.ImageIndex,
                SelectedImageIndex = originalNode.SelectedImageIndex
            };

            // Рекурсивно проверяем детей
            foreach (TreeNode child in originalNode.Nodes)
            {
                TreeNode filteredChild = FilterChildren(child, searchText);
                if (filteredChild != null)
                {
                    newNode.Nodes.Add(filteredChild);
                    childMatches = true;
                }
            }

            // Если текущий узел или его дети совпадают, возвращаем узел
            if (selfMatches || childMatches)
            {
                return newNode;
            }

            return null;
        }

        private TreeNode CloneTreeNode(TreeNode originalNode)
        {
            TreeNode newNode = new TreeNode(originalNode.Text)
            {
                Tag = originalNode.Tag,
                ImageIndex = originalNode.ImageIndex,
                SelectedImageIndex = originalNode.SelectedImageIndex
            };

            foreach (TreeNode child in originalNode.Nodes)
            {
                newNode.Nodes.Add(CloneTreeNode(child));
            }

            return newNode;
        }

        private void CopyChildNodes(TreeNode sourceNode, TreeNode destNode)
        {
            foreach (TreeNode child in sourceNode.Nodes)
            {
                TreeNode newChild = CloneTreeNode(child);
                destNode.Nodes.Add(newChild);
            }
        }

        // Метод для сохранения выбранных узлов
        private void SaveSelectedNodes(TreeNodeCollection nodes, List<string> selectedNodes)
        {
            foreach (TreeNode node in nodes)
            {
                if (node.Checked)
                {
                    selectedNodes.Add(node.FullPath);
                }
                SaveSelectedNodes(node.Nodes, selectedNodes);
            }
        }

        // Метод для восстановления выбранных узлов
        private void RestoreSelectedNodes(TreeNodeCollection nodes, List<string> selectedNodes)
        {
            foreach (TreeNode node in nodes)
            {
                if (selectedNodes.Contains(node.FullPath))
                {
                    node.Checked = true;
                }
                RestoreSelectedNodes(node.Nodes, selectedNodes);
            }
        }

        // Фильтрация узлов (остаётся без изменений)
        private TreeNode FilterNode(TreeNode node, string searchText)
        {
            TreeNode filteredNode = null;

            // Рекурсивно фильтруем дочерние узлы
            foreach (TreeNode child in node.Nodes)
            {
                TreeNode filteredChild = FilterNode(child, searchText);
                if (filteredChild != null)
                {
                    if (filteredNode == null)
                        filteredNode = new TreeNode(node.Text);

                    filteredNode.Nodes.Add(filteredChild);
                }
            }

            // Добавляем узел, если он сам соответствует фильтру
            if (node.Text.ToLower().Contains(searchText))
            {
                if (filteredNode == null)
                    filteredNode = new TreeNode(node.Text);
            }

            return filteredNode;
        }

        private void IncrementProgress()
        {
            loadedItems++;
            int percent = (int)((loadedItems / (float)totalItems) * 100);
            progressBar.Value = Math.Min(percent, 100);
            lblProgress.Text = $"Загрузка {percent}%... {loadedItems}/{totalItems} ";
            if (percent == 95) Task.Delay(1000);
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
            Dictionary<string, List<string>> missingReferences = new();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                // Собираем список всех выбранных таблиц
                HashSet<string> selectedTables = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                foreach (TreeNode rootNode in treeView.Nodes)
                {
                    if (rootNode.Text == "Таблицы")
                    {
                        foreach (TreeNode tableNode in rootNode.Nodes)
                        {
                            if (tableNode.Checked)
                            {
                                selectedTables.Add(tableNode.Text);
                            }
                        }
                    }
                }

                // Проверяем зависимости только если есть выбранные таблицы
                if (selectedTables.Count > 0)
                {
                    foreach (TreeNode rootNode in treeView.Nodes)
                    {
                        if (rootNode.Text == "Таблицы")
                        {
                            foreach (TreeNode tableNode in rootNode.Nodes)
                            {
                                if (tableNode.Checked)
                                {
                                    string tableName = tableNode.Text;
                                    List<string> references = GetTableReferences(connection, tableName);

                                    // Фильтруем ссылки, оставляем только те, которые не выбраны
                                    List<string> missingRefs = references
                                        .Where(refTable => !selectedTables.Contains(refTable))
                                        .ToList();

                                    if (missingRefs.Count > 0)
                                    {
                                        missingReferences[tableName] = missingRefs;
                                    }
                                }
                            }
                        }
                    }
                }

                // Показываем предупреждение только если есть невыбранные зависимости
                if (missingReferences.Count > 0)
                {
                    StringBuilder warningMessage = new StringBuilder();
                    warningMessage.AppendLine("Следующие выбранные таблицы имеют ссылки на невыбранные таблицы:");
                    warningMessage.AppendLine();

                    foreach (var kvp in missingReferences)
                    {
                        warningMessage.AppendLine($"- {kvp.Key} ссылается на:");
                        foreach (var refTable in kvp.Value)
                        {
                            warningMessage.AppendLine($"  • {refTable}");
                        }
                        warningMessage.AppendLine();
                    }

                    warningMessage.AppendLine("Рекомендуется также выбрать эти таблицы для корректной миграции.");

                    var result = MessageBox.Show(warningMessage.ToString(),
                                               "Предупреждение о зависимостях",
                                               MessageBoxButtons.OKCancel,
                                               MessageBoxIcon.Warning);

                    if (result == DialogResult.Cancel)
                    {
                        return; // Прерываем экспорт, если пользователь нажал Cancel
                    }
                }

                // Продолжаем экспорт
                foreach (TreeNode rootNode in treeView.Nodes)
                {
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

                            foreach (TreeNode childNode in tableNode.Nodes)
                            {
                                if (childNode.Checked && childNode.Text == "Триггеры")
                                {
                                    foreach (TreeNode triggerNode in childNode.Nodes)
                                    {
                                        string triggerName = triggerNode.Text;
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

        // Новый метод для получения списка таблиц, на которые ссылается указанная таблица
        private List<string> GetTableReferences(SqlConnection connection, string tableName)
        {
            List<string> references = new List<string>();

            string query = @"
SELECT DISTINCT 
    OBJECT_NAME(fk.referenced_object_id) AS referenced_table
FROM 
    sys.foreign_keys fk
    INNER JOIN sys.foreign_key_columns fkc ON fk.object_id = fkc.constraint_object_id
    INNER JOIN sys.tables tab ON fk.parent_object_id = tab.object_id
WHERE 
    tab.name = @tableName";

            using (SqlCommand cmd = new SqlCommand(query, connection))
            {
                cmd.Parameters.AddWithValue("@tableName", tableName);
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        references.Add(reader["referenced_table"].ToString());
                    }
                }
            }

            return references;
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

        private void label3_Click(object sender, EventArgs e)
        {
            PosgreConnectionForm formPosgr = new PosgreConnectionForm();
            formPosgr.Show();
        }

    }
}