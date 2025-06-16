using System;
using System.Windows.Forms;

namespace DBMigration
{
    public partial class DataExportPercentageForm : Form
    {
        // Публичное свойство с get и set
        public int SelectedPercentage { get; set; } = 100;

        public DataExportPercentageForm()
        {
            InitializeComponent();
            InitializeComboBox();
        }

        private void InitializeComboBox()
        {
            // Добавляем возможные значения процентов
            comboBoxPercentage.Items.AddRange(new object[] { 10, 25, 50, 75, 100 });

            // Устанавливаем выбранное значение равным текущему SelectedPercentage
            comboBoxPercentage.SelectedItem = SelectedPercentage;
        }

        private void buttonOk_Click(object sender, EventArgs e)
        {
            // Устанавливаем выбранное значение
            SelectedPercentage = (int)comboBoxPercentage.SelectedItem;
            DialogResult = DialogResult.OK;
            Close();
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}