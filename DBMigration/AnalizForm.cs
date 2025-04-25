using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

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

        public AnalizForm(List<string> items)
        {
            InitializeComponent();
            selectedItems = items;

            // Пример: отобразим в ListBox
            foreach (var item in selectedItems)
            {
                listBox1.Items.Add(item);
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

        // Пример метода, возвращающего текст с информацией
        private string GetItemChangeInfo(string itemName)
        {
            // Здесь можно реализовать анализ изменений для таблицы/процедуры/функции
            // Сейчас просто пример текста
            return $"Изменения для элемента: {itemName}\n\n— Структура изменилась\n— Были добавлены триггеры";
        }


    }
}
