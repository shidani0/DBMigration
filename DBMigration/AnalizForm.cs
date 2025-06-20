﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Data.SqlClient;
using System.IO;
using static DBMigration.MainForm;
using System.Text.RegularExpressions;

namespace DBMigration
{
    
    public partial class AnalizForm : Form
    {
        private Dictionary<string, TableExportSettings> tableExportSettings = new Dictionary<string, TableExportSettings>();

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
            foreach (var obj in originalScripts.Where(o => o.Type == "Триггер"))
            {
                listBoxTrigger.Items.Add(obj.Name);
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
                string selectedTable = listBoxTables.SelectedItem.ToString();

                // Создаем или получаем настройки для таблицы
                if (!tableExportSettings.TryGetValue(selectedTable, out var settings))
                {
                    settings = new TableExportSettings { TableName = selectedTable };
                    tableExportSettings[selectedTable] = settings;
                }

                using (var percentForm = new DataExportPercentageForm())
                {
                    // Устанавливаем текущий процент для формы
                    percentForm.SelectedPercentage = settings.ExportPercent;

                    if (percentForm.ShowDialog() == DialogResult.OK)
                    {
                        // Сохраняем выбранный процент
                        settings.ExportPercent = percentForm.SelectedPercentage;

                        MessageBox.Show(
                            $"Для таблицы '{selectedTable}' установлен экспорт {settings.ExportPercent}% данных.",
                            "Настройки экспорта",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information
                        );
                    }
                }
            }
        }

        private void listBoxTrigger_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBoxTrigger.SelectedItem != null)
            {
                string selectedItem = listBoxTrigger.SelectedItem.ToString();

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
                        case "Триггер":
                            return GetPostgresTriggerScript(connection, itemName);
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

                int tablesCount = 0, funcsCount = 0, procsCount = 0, trigCount = 0;
                Dictionary<string, List<(string name, string content)>> grouped = new();

                grouped["Таблицы"] = new List<(string, string)>();
                grouped["Триггеры"] = new List<(string, string)>();
                grouped["Функции"] = new List<(string, string)>();
                grouped["Процедуры"] = new List<(string, string)>();

                // Обработка таблиц
                foreach (var item in listBoxTables.Items)
                {
                    string tableName = item.ToString();

                    // Получаем настройки экспорта для этой таблицы
                    int exportPercent = 100; // значение по умолчанию
                    if (tableExportSettings.TryGetValue(tableName, out var settings))
                    {
                        exportPercent = settings.ExportPercent;
                    }

                    // Получение скрипта таблицы
                    var script = $"-- Таблица: {tableName}\n" + GetPostgresTableScript(connection, tableName);

                    // Если процент меньше 100, добавляем данные
                    if (exportPercent <= 100)
                    {
                        script += "\n\n-- Данные таблицы (" + exportPercent + "%):\n";
                        script += GetPostgresTableData(connection, tableName, exportPercent);
                    }
                    grouped["Таблицы"].Add((tableName, script));
                    tablesCount++;
                }


                foreach (var item in listBoxTrigger.Items)
                {
                    string trigName = item.ToString();
                    var script = $"-- Триггер: {trigName}\n" + GetPostgresTriggerScript(connection, trigName);
                    grouped["Триггеры"].Add((trigName, script));
                    trigCount++;
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

                string folderName = $"PG_Таблицы_{tablesCount}_Триггеры{trigCount}_Функции_{funcsCount}_Процедуры_{procsCount}";
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

        private string GetPostgresTableData(SqlConnection connection, string tableName, int percent)
        {
            try
            {
                StringBuilder dataScript = new StringBuilder();

                // 1. Проверяем наличие автоинкрементных полей и отключаем их
                string identityCheckQuery = $@"
SELECT c.name AS column_name
FROM sys.columns c
JOIN sys.tables t ON c.object_id = t.object_id
WHERE t.name = '{tableName}' AND c.is_identity = 1";

                List<string> identityColumns = new List<string>();
                using (SqlCommand cmd = new SqlCommand(identityCheckQuery, connection))
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        identityColumns.Add(reader["column_name"].ToString());
                    }
                }

                // Добавляем команды для отключения автоинкремента (если есть такие поля)
                foreach (var column in identityColumns)
                {
                    dataScript.AppendLine($"ALTER TABLE {tableName} ALTER COLUMN {column} DROP IDENTITY;");
                }

                // 2. Получить количество строк в таблице
                string countQuery = $"SELECT COUNT(*) FROM {tableName}";
                int totalRows = 0;
                using (SqlCommand cmd = new SqlCommand(countQuery, connection))
                {
                    totalRows = (int)cmd.ExecuteScalar();
                }

                // 3. Вычислить количество строк для выборки
                int rowsToSelect = (int)Math.Round(totalRows * percent / 100.0);
                if (rowsToSelect < 1) rowsToSelect = 1;

                // 4. Получить данные
                string dataQuery = $@"
SELECT * FROM (
    SELECT *, ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) AS RowNum 
    FROM {tableName}
) AS T 
WHERE RowNum <= {rowsToSelect}";

                using (SqlCommand cmd = new SqlCommand(dataQuery, connection))
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        // Получить список столбцов (исключая RowNum)
                        var columns = new List<string>();
                        for (int i = 0; i < reader.FieldCount - 1; i++) // -1 чтобы исключить RowNum
                        {
                            columns.Add(reader.GetName(i));
                        }

                        // Генерация INSERT-запросов для PostgreSQL
                        while (reader.Read())
                        {
                            var values = new List<string>();
                            for (int i = 0; i < reader.FieldCount - 1; i++) // -1 чтобы исключить RowNum
                            {
                                if (reader.IsDBNull(i))
                                {
                                    values.Add("NULL");
                                }
                                else
                                {
                                    var value = reader.GetValue(i);
                                    // Форматирование значений для PostgreSQL
                                    if (value is string || value is Guid)
                                    {
                                        values.Add($"'{value.ToString().Replace("'", "''")}'");
                                    }
                                    else if (value is DateTime)
                                    {
                                        values.Add($"'{(DateTime)value:yyyy-MM-dd HH:mm:ss}'::timestamp");
                                    }
                                    else if (value is DateTimeOffset)
                                    {
                                        values.Add($"'{(DateTimeOffset)value:yyyy-MM-dd HH:mm:sszzz}'::timestamp with time zone");
                                    }
                                    else if (value is bool)
                                    {
                                        values.Add((bool)value ? "TRUE" : "FALSE");
                                    }
                                    else if (value is byte[])
                                    {
                                        values.Add($"E'\\\\x{BitConverter.ToString((byte[])value).Replace("-", "")}'");
                                    }
                                    else
                                    {
                                        values.Add(value.ToString());
                                    }
                                }
                            }

                            dataScript.AppendLine($"INSERT INTO {tableName} ({string.Join(", ", columns)}) " +
                                                 $"VALUES ({string.Join(", ", values)});");
                        }
                    }
                }

                // Восстанавливаем автоинкрементные поля (если они были)
                foreach (var column in identityColumns)
                {
                    dataScript.AppendLine($"ALTER TABLE {tableName} ALTER COLUMN {column} ADD GENERATED BY DEFAULT AS IDENTITY;");
                }

                return dataScript.ToString();
            }
            catch (Exception ex)
            {
                return $"-- Ошибка при выгрузке данных: {ex.Message}\n";
            }
        }

        private string GetPostgresTableScript(SqlConnection connection, string tableName)
        {
            try
            {
                string script = $"-- PostgreSQL скрипт создания таблицы {tableName}\n";

                // Запрос для получения информации о столбцах, включая IDENTITY
                string tableScriptQuery = @"
SELECT 
    c.name AS COLUMN_NAME,
    tp.name AS DATA_TYPE,
    c.max_length AS CHARACTER_MAXIMUM_LENGTH,
    c.is_nullable AS IS_NULLABLE,
    c.precision AS NUMERIC_PRECISION,
    c.scale AS NUMERIC_SCALE,
    c.is_identity AS IS_IDENTITY,
    IDENT_SEED(TABLE_SCHEMA + '.' + TABLE_NAME) AS IDENTITY_SEED,
    IDENT_INCR(TABLE_SCHEMA + '.' + TABLE_NAME) AS IDENTITY_INCREMENT
FROM sys.columns c
JOIN sys.tables t ON c.object_id = t.object_id
JOIN sys.types tp ON c.user_type_id = tp.user_type_id
JOIN INFORMATION_SCHEMA.TABLES it ON t.name = it.TABLE_NAME
WHERE t.name = @tableName
ORDER BY c.column_id";

                using (SqlCommand cmd = new SqlCommand(tableScriptQuery, connection))
                {
                    cmd.Parameters.AddWithValue("@tableName", tableName);
                    SqlDataReader reader = cmd.ExecuteReader();

                    var columns = new List<string>();
                    while (reader.Read())
                    {
                        string columnName = reader["COLUMN_NAME"].ToString();
                        string dataType = reader["DATA_TYPE"].ToString();
                        string isNullable = reader["IS_NULLABLE"].ToString();
                        object maxLength = reader["CHARACTER_MAXIMUM_LENGTH"];
                        object precision = reader["NUMERIC_PRECISION"];
                        object scale = reader["NUMERIC_SCALE"];
                        bool isIdentity = Convert.ToBoolean(reader["IS_IDENTITY"]);
                        object identitySeed = reader["IDENTITY_SEED"];
                        object identityIncrement = reader["IDENTITY_INCREMENT"];

                        string pgType = ConvertToPostgresType(dataType, maxLength, precision, scale);

                        string columnDef = $"{columnName} {pgType}";

                        // Добавляем IDENTITY для PostgreSQL (GENERATED BY DEFAULT AS IDENTITY)
                        if (isIdentity)
                        {
                            columnDef += " GENERATED BY DEFAULT AS IDENTITY";
                            // Добавляем параметры SEQUENCE если они отличаются от стандартных
                            if (identitySeed != DBNull.Value && identityIncrement != DBNull.Value)
                            {
                                int seed = Convert.ToInt32(identitySeed);
                                int increment = Convert.ToInt32(identityIncrement);

                                if (seed != 1 || increment != 1)
                                {
                                    columnDef += $" (START WITH {seed} INCREMENT BY {increment})";
                                }
                            }
                        }
                        else if (isNullable == "NO")
                        {
                            columnDef += " NOT NULL";
                        }

                        columns.Add(columnDef);
                    }
                    reader.Close();

                    // Добавляем первичные ключи
                    string pkQuery = @"
SELECT c.name AS COLUMN_NAME
FROM sys.indexes i
JOIN sys.index_columns ic ON i.object_id = ic.object_id AND i.index_id = ic.index_id
JOIN sys.columns c ON ic.object_id = c.object_id AND ic.column_id = c.column_id
JOIN sys.tables t ON i.object_id = t.object_id
WHERE i.is_primary_key = 1 AND t.name = @tableName
ORDER BY ic.key_ordinal";

                    var pkColumns = new List<string>();
                    using (SqlCommand pkCmd = new SqlCommand(pkQuery, connection))
                    {
                        pkCmd.Parameters.AddWithValue("@tableName", tableName);
                        using (SqlDataReader pkReader = pkCmd.ExecuteReader())
                        {
                            while (pkReader.Read())
                            {
                                pkColumns.Add(pkReader["COLUMN_NAME"].ToString());
                            }
                        }
                    }

                    script += $"CREATE TABLE {tableName} (\n    " + string.Join(",\n    ", columns);

                    if (pkColumns.Count > 0)
                    {
                        script += $",\n    PRIMARY KEY ({string.Join(", ", pkColumns)})";
                    }

                    script += "\n);";
                }

                return script;
            }
            catch (Exception ex)
            {
                return $"-- Ошибка генерации скрипта для таблицы {tableName}: {ex.Message}\n";
            }
        }

        private string ConvertToPostgresType(string sqlServerType, object maxLength, object precision, object scale)
        {
            switch (sqlServerType.ToLower())
            {
                case "varchar":
                case "nvarchar":
                    if (maxLength != DBNull.Value)
                    {
                        int length = Convert.ToInt32(maxLength);
                        if (length == -1) return "TEXT"; // для MAX
                        if (length > 0) return $"VARCHAR({length})"; // явно указываем VARCHAR для PostgreSQL
                        return "TEXT"; // для нулевой длины или других случаев
                    }
                    return "TEXT";
                case "char":
                case "nchar":
                    if (maxLength != DBNull.Value && Convert.ToInt32(maxLength) > 0)
                    {
                        int length = Convert.ToInt32(maxLength);
                        if (length == -1) return "TEXT"; // для MAX
                        return $"{sqlServerType.ToUpper()}({length})";
                    }
                    return "TEXT";

                case "int":
                    return "INTEGER";
                case "bigint":
                    return "BIGINT";
                case "smallint":
                    return "SMALLINT";
                case "tinyint":
                    return "SMALLINT"; // в PostgreSQL нет tinyint, используем smallint
                case "bit":
                    return "BOOLEAN";
                case "float":
                    return "DOUBLE PRECISION";
                case "real":
                    return "REAL";
                case "decimal":
                case "numeric":
                    if (precision != DBNull.Value && scale != DBNull.Value)
                        return $"NUMERIC({precision}, {scale})";
                    return "NUMERIC";
                case "money":
                    return "MONEY";
                case "date":
                    return "DATE";
                case "datetime":
                    return "TIMESTAMP";
                case "datetime2":
                    return "TIMESTAMP";
                case "smalldatetime":
                    return "TIMESTAMP";
                case "time":
                    return "TIME";
                case "timestamp":
                    return "TIMESTAMP";
                case "uniqueidentifier":
                    return "UUID";
                case "binary":
                case "varbinary":
                case "image":
                    return "BYTEA";
                case "xml":
                    return "XML";
                case "json":
                    return "JSON";
                default:
                    return sqlServerType.ToUpper();
            }
        }

        // Добавьте эти методы для функций и процедур
        private string GetPostgresFunctionScript(SqlConnection connection, string functionName)
        {
            try
            {
                string script = $"-- PostgreSQL скрипт функции {functionName}\n";

                using (SqlCommand cmd = new SqlCommand("SELECT OBJECT_DEFINITION(OBJECT_ID(@funcName))", connection))
                {
                    cmd.Parameters.AddWithValue("@funcName", functionName);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            string rawCode = reader.GetString(0);
                            script += ConvertToPostgresSyntax(rawCode)
                                .Replace("CREATE FUNCTION", "CREATE OR REPLACE FUNCTION")
                                .Replace("AS BEGIN", "AS $$ BEGIN")
                                .Replace("END", "END; $$ LANGUAGE plpgsql;");
                        }
                    }
                }

                return script;
            }
            catch (Exception ex)
            {
                return $"-- Ошибка генерации скрипта для функции {functionName}: {ex.Message}";
            }
        }



        private string GetPostgresProcedureScript(SqlConnection connection, string procedureName)
        {
            try
            {
                string script = $"-- PostgreSQL скрипт процедуры {procedureName}\n";

                using (SqlCommand cmd = new SqlCommand("SELECT OBJECT_DEFINITION(OBJECT_ID(@procName))", connection))
                {
                    cmd.Parameters.AddWithValue("@procName", procedureName);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            string rawCode = reader.GetString(0);
                            script += ConvertToPostgresSyntax(rawCode)
                                .Replace("CREATE PROCEDURE", "CREATE OR REPLACE PROCEDURE")
                                .Replace("AS BEGIN", "LANGUAGE plpgsql AS $$ BEGIN")
                                .Replace("END", "END; $$");
                        }
                    }
                }

                return script;
            }
            catch (Exception ex)
            {
                return $"-- Ошибка генерации скрипта для процедуры {procedureName}: {ex.Message}";
            }
        }

        private string GetPostgresTriggerScript(SqlConnection connection, string triggerName)
        {
            try
            {
                using (SqlCommand cmd = new SqlCommand(@"
            SELECT 
                tr.name AS TriggerName,
                tb.name AS TableName,
                m.definition AS TriggerBody,
                tr.is_instead_of_trigger,
                tr.type_desc
            FROM sys.triggers tr
            JOIN sys.tables tb ON tr.parent_id = tb.object_id
            JOIN sys.sql_modules m ON tr.object_id = m.object_id
            WHERE tr.name = @triggerName", connection))
                {
                    cmd.Parameters.AddWithValue("@triggerName", triggerName);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            string name = reader.GetString(0);
                            string table = reader.GetString(1);
                            string body = reader.GetString(2);
                            string typeDesc = reader.GetString(4);

                            // Преобразуем тело триггера
                            string pgBody = ConvertToPostgresTriggerFunctionBody(body);

                            // Определяем тип операции
                            string operation = typeDesc.Contains("DELETE") ? "DELETE"
                                             : typeDesc.Contains("INSERT") ? "INSERT"
                                             : typeDesc.Contains("UPDATE") ? "UPDATE"
                                             : "UNKNOWN";

                            string functionName = $"trg_{table}_{operation.ToLower()}";
                            string triggerNamePg = $"{table}_{operation.ToLower()}";

                            // Генерация функции триггера
                            string functionScript = $@"
-- Триггерная функция
CREATE OR REPLACE FUNCTION {functionName}()
RETURNS TRIGGER AS $$
DECLARE
    v_user TEXT := current_user;
    v_ip TEXT;
BEGIN
    -- Получение IP-адреса клиента, если доступно
    BEGIN
        SELECT inet_client_addr() INTO v_ip;
    EXCEPTION
        WHEN OTHERS THEN
            v_ip := 'unknown';
    END;

    {pgBody}

    RETURN NULL;
END;
$$ LANGUAGE plpgsql;";

                            // Генерация триггера
                            string triggerScript = $@"
-- Создание триггера
DROP TRIGGER IF EXISTS {triggerNamePg} ON {table};

CREATE TRIGGER {triggerNamePg}
AFTER {operation} ON {table}
FOR EACH ROW
EXECUTE FUNCTION {functionName}();";

                            return functionScript + "\n\n" + triggerScript;
                        }
                    }
                }

                return $"-- Триггер {triggerName} не найден.";
            }
            catch (Exception ex)
            {
                return $"-- Ошибка генерации скрипта для триггера {triggerName}: {ex.Message}";
            }
        }

        private string ConvertToPostgresTriggerFunctionBody(string sqlServerBody)
        {
            return sqlServerBody
                .Replace("INSERTED", "NEW")
                .Replace("DELETED", "OLD")
                .Replace("BEGIN", "") // BEGIN уже есть в шаблоне
                .Replace("END", "")   // END тоже
                .Replace(";", ";")    // Можно доп. обработку здесь
                .Trim();
        }




        /// <summary>
        /// Преобразует типы данных из MSSQL в PostgreSQL-совместимые.
        /// </summary>
    //         private string ConvertToPostgresType(string sqlType, object maxLength, object precision, object scale)
    //         {
    //             switch (sqlType.ToLower())
    //             {
    //                 case "int":
    //                 case "integer":
    //                     return "integer";
    //                 case "bigint":
    //                     return "bigint";
    //                 case "smallint":
    //                     return "smallint";
    //                 case "tinyint":
    //                     return "smallint"; // PostgreSQL не поддерживает tinyint
    //                 case "bit":
    //                     return "boolean";
    //                 case "nvarchar":
    //                 case "varchar":
    //                     return (maxLength == DBNull.Value || (int)maxLength < 0) ? "text" : $"varchar({maxLength})";
    //                 case "nchar":
    //                 case "char":
    //                     return (maxLength == DBNull.Value || (int)maxLength < 0) ? "text" : $"char({maxLength})";
    //                 case "text":
    //                 case "ntext":
    //                     return "text";
    //                 case "datetime":
    //                 case "smalldatetime":
    //                 case "datetime2":
    //                 case "datetimeoffset":
    //                     return "timestamp";
    //                 case "date":
    //                     return "date";
    //                 case "time":
    //                     return "time";
    //                 case "decimal":
    //                 case "numeric":
    //                     return $"numeric({precision},{scale})";
    //                 case "float":
    //                     return "double precision";
    //                 case "real":
    //                     return "real";
    //                 case "uniqueidentifier":
    //                     return "uuid";
    //                 case "binary":
    //                 case "varbinary":
    //                 case "image":
    //                     return "bytea";
    //                 default:
    //                     return "text"; // по умолчанию
    //             }
    //         }

        private string ConvertToPostgresSyntax(string sqlCode)
        {
            if (string.IsNullOrWhiteSpace(sqlCode))
                return "-- Пустой объект, нечего конвертировать";

            return sqlCode
                // Общие символы
                .Replace("[", "\"")
                .Replace("]", "\"")
                .Replace("dbo.", "")
                .Replace("GETDATE()", "CURRENT_TIMESTAMP")
                .Replace("NEWID()", "gen_random_uuid()")
                .Replace("ISNULL", "COALESCE")
                .Replace("SYSDATETIME()", "CURRENT_TIMESTAMP")
                .Replace("TRY_CONVERT", "CAST")
                .Replace("LEN(", "LENGTH(")
                .Replace("INSERTED", "NEW")
                .Replace("DELETED", "OLD")

                // Типы данных
                .Replace("INT IDENTITY", "SERIAL")
                .Replace("INT", "INTEGER")
                .Replace("BIGINT", "BIGINT")
                .Replace("SMALLINT", "SMALLINT")
                .Replace("TINYINT", "SMALLINT")
                .Replace("BIT", "BOOLEAN")
                .Replace("DECIMAL", "NUMERIC")
                .Replace("NUMERIC", "NUMERIC")
                .Replace("FLOAT", "DOUBLE PRECISION")
                .Replace("REAL", "REAL")
                .Replace("MONEY", "NUMERIC(19,4)")
                .Replace("DATETIME", "TIMESTAMP")
                .Replace("SMALLDATETIME", "TIMESTAMP")
                .Replace("DATE", "DATE")
                .Replace("TIME", "TIME")
                .Replace("CHAR", "CHAR")
                .Replace("NCHAR", "CHAR")
                .Replace("VARCHAR", "VARCHAR")
                .Replace("NVARCHAR", "VARCHAR")
                .Replace("TEXT", "TEXT")
                .Replace("NTEXT", "TEXT")
                .Replace("IMAGE", "BYTEA")
                .Replace("UNIQUEIDENTIFIER", "UUID");
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
            string folderName = $"Экспорт_{date}";
            string exportFolderPath = Path.Combine(basePath, folderName);
            Directory.CreateDirectory(exportFolderPath);

            foreach (var dbObject in originalScripts)
            {
                if (dbObject == null || string.IsNullOrWhiteSpace(dbObject.Script) || string.IsNullOrWhiteSpace(dbObject.Name))
                    continue;

                string safeName = Regex.Replace(dbObject.Name, $"[{Regex.Escape(new string(Path.GetInvalidFileNameChars()))}]", "_");
                string filePath = Path.Combine(exportFolderPath, $"{safeName}__{date}.sql");

                File.WriteAllText(filePath, dbObject.Script, Encoding.UTF8);
                exportCount++;
            }

            MessageBox.Show($"Экспорт завершён!\nСоздано файлов: {exportCount}\nПуть: {exportFolderPath}", "Успешно");
        }


        private void btnTableShow_Click(object sender, EventArgs e)
        {
            listBoxTables.Visible = true;
            listBoxProcedures.Visible = false;
            listBoxTrigger.Visible = false;
            listBoxFunctions.Visible = false;
        }

        private void btnFuncShow_Click(object sender, EventArgs e)
        {
            listBoxTables.Visible = false;
            listBoxTrigger.Visible = false;
            listBoxProcedures.Visible = false;
            listBoxFunctions.Visible = true;
        }

        private void btnProcShow_Click(object sender, EventArgs e)
        {
            listBoxTables.Visible = false;
            listBoxTrigger.Visible = false;
            listBoxProcedures.Visible = true;
            listBoxFunctions.Visible = false;
        }
        private void btnTrigShow_Click(object sender, EventArgs e)
        {
            listBoxTables.Visible = false;
            listBoxTrigger.Visible = true;
            listBoxProcedures.Visible = false;
            listBoxFunctions.Visible = false;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            PosgreConnectionForm formPosgr = new PosgreConnectionForm();
            formPosgr.Show();
        }
    }
    public class TableExportSettings
    {
        public string TableName { get; set; }
        public int ExportPercent { get; set; } = 100; // По умолчанию 100%
    }
}
