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
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using System.IO;
using static DBMigration.MainForm;

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

        private List<DbObject> originalScripts;
        private string connectionString;

        public AnalizForm(List<DbObject> scripts, string connStr)
        {
            InitializeComponent();
            originalScripts = scripts;
            connectionString = connStr;

            // Заполняем listBoxTables только таблицами
            foreach (var obj in originalScripts.Where(o => o.Type == "Таблица"))
            {
                listBoxTables.Items.Add(obj.Name);
            }

            // Заполняем listBoxFunctions только функциями
            foreach (var obj in originalScripts.Where(o => o.Type == "Функция"))
            {
                listBoxFunctions.Items.Add(obj.Name);
            }

            // Заполняем listBoxProcedures только процедурами
            foreach (var obj in originalScripts.Where(o => o.Type == "Процедура"))
            {
                listBoxProcedures.Items.Add(obj.Name);
            }
        }



        private void listBoxTables_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBoxTables.SelectedItem != null)
            {
                string selectedItem = listBoxTables.SelectedItem.ToString();

                // Здесь можно подгрузить или сформировать информацию об изменениях
                string info = GetItemChangeInfo(selectedItem);

                // Показываем окно с информацией
                MessageBox.Show(info, "Информация об изменении", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
        private void listBoxFunctions_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBoxFunctions.SelectedItem != null)
            {
                string selectedItem = listBoxFunctions.SelectedItem.ToString();
                string info = GetItemChangeInfo(selectedItem);
                MessageBox.Show(info, "Информация об изменении (Функция)", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
        private void listBoxProcedures_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBoxProcedures.SelectedItem != null)
            {
                string selectedItem = listBoxProcedures.SelectedItem.ToString();
                string info = GetItemChangeInfo(selectedItem);
                MessageBox.Show(info, "Информация об изменении (Процедура)", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }



        private string GetPostgresScript(string itemName, string itemType)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    switch (itemType)
                    {
                        case "Таблица":
                            return GetPostgresTableScript(connection, itemName);
                        case "Функция":
                            return GetPostgresFunctionScript(connection, itemName);
                        case "Процедура":
                            return GetPostgresProcedureScript(connection, itemName);
                        default:
                            return "-- Тип объекта не поддерживается для конвертации в PostgreSQL";
                    }
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
            var dbObject = originalScripts.FirstOrDefault(o => o.Name == itemName);
            if (dbObject == null)
                return "Элемент не найден.";

            string originalScript = dbObject.Script;
            string postgresScript = GetPostgresScript(itemName, dbObject.Type);

            return $"Изменения для {itemName} ({dbObject.Type}):\n\n" +
                   $"[Оригинал SQL Server]:\n{originalScript}\n\n" +
                   $"[PostgreSQL]:\n{postgresScript}";
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

                // Обработка таблиц
                foreach (var item in listBoxTables.Items)
                {
                    string tableName = item.ToString();
                    var script = $"-- Таблица: {tableName}\n" + GetPostgresTableScript(connection, tableName);
                    grouped["Таблицы"].Add((tableName, script));
                    tablesCount++;
                }

                // Обработка функций
                foreach (var item in listBoxFunctions.Items)
                {
                    string funcName = item.ToString();
                    var funcScript = $"-- Функция: {funcName}\n" + GetPostgresFunctionScript(connection, funcName);
                    grouped["Функции"].Add((funcName, funcScript));
                    funcsCount++;
                }

                // Обработка процедур
                foreach (var item in listBoxProcedures.Items)
                {
                    string procName = item.ToString();
                    var procScript = $"-- Процедура: {procName}\n" + GetPostgresProcedureScript(connection, procName);
                    grouped["Процедуры"].Add((procName, procScript));
                    procsCount++;
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

                MessageBox.Show($"Экспорт в PostgreSQL завершён!\nСоздано: {exportFolderPath}", "Успешно");
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

        // Добавьте эти методы для функций и процедур
        private string GetPostgresFunctionScript(SqlConnection connection, string functionName)
        {
            try
            {
                string script = "-- PostgreSQL скрипт функции\n";
                SqlCommand command = new SqlCommand($"SELECT OBJECT_DEFINITION(OBJECT_ID(@funcName))", connection);
                command.Parameters.AddWithValue("@funcName", functionName);
                SqlDataReader reader = command.ExecuteReader();

                if (reader.Read())
                {
                    string originalScript = reader.GetString(0);
                    // Простая конвертация - в реальном проекте нужно более сложное преобразование
                    script += originalScript
                        .Replace("[", "\"")
                        .Replace("]", "\"")
                        .Replace("dbo.", "")
                        .Replace("GETDATE()", "CURRENT_TIMESTAMP");
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
                return $"-- Ошибка генерации скрипта для функции {functionName}: {ex.Message}\n";
            }
        }

        private string GetPostgresProcedureScript(SqlConnection connection, string procedureName)
        {
            try
            {
                string script = "-- PostgreSQL скрипт процедуры\n";
                SqlCommand command = new SqlCommand($"SELECT OBJECT_DEFINITION(OBJECT_ID(@procName))", connection);
                command.Parameters.AddWithValue("@procName", procedureName);
                SqlDataReader reader = command.ExecuteReader();

                if (reader.Read())
                {
                    string originalScript = reader.GetString(0);
                    // Простая конвертация - в реальном проекте нужно более сложное преобразование
                    script += originalScript
                        .Replace("[", "\"")
                        .Replace("]", "\"")
                        .Replace("dbo.", "")
                        .Replace("GETDATE()", "CURRENT_TIMESTAMP")
                        .Replace("CREATE PROCEDURE", "CREATE OR REPLACE FUNCTION")
                        .Replace("AS BEGIN", "RETURNS VOID AS $$ BEGIN")
                        .Replace("END", "END; $$ LANGUAGE plpgsql;");
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
                return $"-- Ошибка генерации скрипта для процедуры {procedureName}: {ex.Message}\n";
            }
        }
        /*

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
         */

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

        private void ExportEachObjectToFolder()
        {
            FolderBrowserDialog folderDialog = new FolderBrowserDialog
            {
                Description = "Выберите папку для экспорта SQL-объектов"
            };

            if (folderDialog.ShowDialog() != DialogResult.OK)
                return;

            string basePath = folderDialog.SelectedPath;
            string date = DateTime.Now.ToString("dd-MM-yyyy");

            int exportCount = 0;

            // Название родительской папки
            string folderName = $"Экспорт_{date}";
            string exportFolderPath = Path.Combine(basePath, folderName);
            Directory.CreateDirectory(exportFolderPath);

            foreach (var item in listBoxTables.Items)
            {
                string objectName = item.ToString();

                // Ищем объект в списке
                var dbObject = originalScripts.FirstOrDefault(o => o.Name == objectName);
                if (dbObject != null)
                {
                    string safeName = string.Concat(objectName.Split(Path.GetInvalidFileNameChars()));
                    string filePath = Path.Combine(exportFolderPath, $"{safeName}__{date}.sql");
                    File.WriteAllText(filePath, dbObject.Script);
                    exportCount++;
                }
            }

            MessageBox.Show($"Экспорт завершён!\nСоздано файлов: {exportCount}\nПуть: {exportFolderPath}", "Успешно");
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

        private void btnTableShow_Click(object sender, EventArgs e)
        {
            listBoxTables.Visible = true;
            listBoxProcedures.Visible = false;
            listBoxFunctions.Visible = false;
        }

        private void btnFuncShow_Click(object sender, EventArgs e)
        {
            listBoxTables.Visible = false;
            listBoxProcedures.Visible = false;
            listBoxFunctions.Visible = true;
        }

        private void btnProcShow_Click(object sender, EventArgs e)
        {
            listBoxTables.Visible = false;
            listBoxProcedures.Visible = true;
            listBoxFunctions.Visible = false;
        }
    }
}
