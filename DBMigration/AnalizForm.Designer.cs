using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Drawing;

namespace DBMigration
{
    partial class AnalizForm
    {
        private System.ComponentModel.IContainer components = null;
        private Label lblPanelMenuConnectionFrom;

        private Panel panelResizeMenu;
        private bool isResizing = false;
        private Point lastMousePosition;

        private System.Windows.Forms.Panel panelMenu;
        private System.Windows.Forms.Panel panelContent;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this.panelMenu = new System.Windows.Forms.Panel();
            this.label6 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.lblPanelMenuConnectionFrom = new System.Windows.Forms.Label();
            this.panelContent = new System.Windows.Forms.Panel();
            this.label5 = new System.Windows.Forms.Label();
            this.button2 = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.btnTrigShow = new System.Windows.Forms.Button();
            this.listBoxTrigger = new System.Windows.Forms.ListBox();
            this.listBoxProcedures = new System.Windows.Forms.ListBox();
            this.listBoxFunctions = new System.Windows.Forms.ListBox();
            this.btnProcShow = new System.Windows.Forms.Button();
            this.btnFuncShow = new System.Windows.Forms.Button();
            this.btnTableShow = new System.Windows.Forms.Button();
            this.convertToPostgresButton = new System.Windows.Forms.Button();
            this.exportButton = new System.Windows.Forms.Button();
            this.listBoxTables = new System.Windows.Forms.ListBox();
            this.panelResizeMenu = new System.Windows.Forms.Panel();
            this.panelMenu.SuspendLayout();
            this.panelContent.SuspendLayout();
            this.SuspendLayout();
            // 
            // panelMenu
            // 
            this.panelMenu.BackColor = System.Drawing.Color.LightGray;
            this.panelMenu.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelMenu.Controls.Add(this.label6);
            this.panelMenu.Controls.Add(this.label4);
            this.panelMenu.Controls.Add(this.label3);
            this.panelMenu.Controls.Add(this.label2);
            this.panelMenu.Controls.Add(this.label1);
            this.panelMenu.Controls.Add(this.lblPanelMenuConnectionFrom);
            this.panelMenu.Dock = System.Windows.Forms.DockStyle.Left;
            this.panelMenu.Location = new System.Drawing.Point(0, 0);
            this.panelMenu.Name = "panelMenu";
            this.panelMenu.Size = new System.Drawing.Size(277, 544);
            this.panelMenu.TabIndex = 2;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Underline))));
            this.label6.Location = new System.Drawing.Point(11, 48);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(241, 36);
            this.label6.TabIndex = 17;
            this.label6.Text = "3. Экспорт и преобразование\nобъектов исходной БД";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            this.label4.Location = new System.Drawing.Point(3, 112);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(175, 40);
            this.label4.TabIndex = 16;
            this.label4.Text = "5. Загрузка объектов \n в целевую БД";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            this.label3.Location = new System.Drawing.Point(3, 92);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(240, 20);
            this.label3.TabIndex = 9;
            this.label3.Text = "4. Подключение к целевой БД";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Underline))));
            this.label2.Location = new System.Drawing.Point(11, 48);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(0, 18);
            this.label2.TabIndex = 3;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            this.label1.Location = new System.Drawing.Point(3, 28);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(254, 20);
            this.label1.TabIndex = 2;
            this.label1.Text = "2. Выбор объектов исходной БД";
            // 
            // lblPanelMenuConnectionFrom
            // 
            this.lblPanelMenuConnectionFrom.AutoSize = true;
            this.lblPanelMenuConnectionFrom.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.lblPanelMenuConnectionFrom.Location = new System.Drawing.Point(3, 8);
            this.lblPanelMenuConnectionFrom.Name = "lblPanelMenuConnectionFrom";
            this.lblPanelMenuConnectionFrom.Size = new System.Drawing.Size(247, 20);
            this.lblPanelMenuConnectionFrom.TabIndex = 0;
            this.lblPanelMenuConnectionFrom.Text = "1. Подключение к исходной БД";
            // 
            // panelContent
            // 
            this.panelContent.BackColor = System.Drawing.Color.White;
            this.panelContent.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelContent.Controls.Add(this.label5);
            this.panelContent.Controls.Add(this.button2);
            this.panelContent.Controls.Add(this.button1);
            this.panelContent.Controls.Add(this.btnTrigShow);
            this.panelContent.Controls.Add(this.listBoxTrigger);
            this.panelContent.Controls.Add(this.listBoxProcedures);
            this.panelContent.Controls.Add(this.listBoxFunctions);
            this.panelContent.Controls.Add(this.btnProcShow);
            this.panelContent.Controls.Add(this.btnFuncShow);
            this.panelContent.Controls.Add(this.btnTableShow);
            this.panelContent.Controls.Add(this.convertToPostgresButton);
            this.panelContent.Controls.Add(this.exportButton);
            this.panelContent.Controls.Add(this.listBoxTables);
            this.panelContent.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelContent.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.panelContent.Location = new System.Drawing.Point(277, 0);
            this.panelContent.Name = "panelContent";
            this.panelContent.Size = new System.Drawing.Size(476, 544);
            this.panelContent.TabIndex = 1;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Underline))));
            this.label5.Location = new System.Drawing.Point(108, 9);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(222, 36);
            this.label5.TabIndex = 14;
            this.label5.Text = "Экспорт и преобразование\nобъектов исходной БД";
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(487, 451);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(75, 23);
            this.button2.TabIndex = 13;
            this.button2.Text = "button2";
            this.button2.UseVisualStyleBackColor = true;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(296, 469);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(175, 62);
            this.button1.TabIndex = 12;
            this.button1.Text = "Подключиться к целевой БД";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // btnTrigShow
            // 
            this.btnTrigShow.Location = new System.Drawing.Point(10, 282);
            this.btnTrigShow.Name = "btnTrigShow";
            this.btnTrigShow.Size = new System.Drawing.Size(170, 74);
            this.btnTrigShow.TabIndex = 11;
            this.btnTrigShow.Text = "Показать список выбранных триггеров";
            this.btnTrigShow.UseVisualStyleBackColor = true;
            this.btnTrigShow.Click += new System.EventHandler(this.btnTrigShow_Click);
            // 
            // listBoxTrigger
            // 
            this.listBoxTrigger.FormattingEnabled = true;
            this.listBoxTrigger.ItemHeight = 20;
            this.listBoxTrigger.Location = new System.Drawing.Point(186, 75);
            this.listBoxTrigger.Name = "listBoxTrigger";
            this.listBoxTrigger.Size = new System.Drawing.Size(285, 264);
            this.listBoxTrigger.TabIndex = 10;
            this.listBoxTrigger.SelectedIndexChanged += new System.EventHandler(this.listBoxTrigger_SelectedIndexChanged);
            // 
            // listBoxProcedures
            // 
            this.listBoxProcedures.FormattingEnabled = true;
            this.listBoxProcedures.ItemHeight = 20;
            this.listBoxProcedures.Location = new System.Drawing.Point(186, 75);
            this.listBoxProcedures.Name = "listBoxProcedures";
            this.listBoxProcedures.Size = new System.Drawing.Size(285, 264);
            this.listBoxProcedures.TabIndex = 9;
            this.listBoxProcedures.SelectedIndexChanged += new System.EventHandler(this.listBoxProcedures_SelectedIndexChanged);
            // 
            // listBoxFunctions
            // 
            this.listBoxFunctions.FormattingEnabled = true;
            this.listBoxFunctions.ItemHeight = 20;
            this.listBoxFunctions.Location = new System.Drawing.Point(186, 75);
            this.listBoxFunctions.Name = "listBoxFunctions";
            this.listBoxFunctions.Size = new System.Drawing.Size(285, 264);
            this.listBoxFunctions.TabIndex = 8;
            this.listBoxFunctions.SelectedIndexChanged += new System.EventHandler(this.listBoxFunctions_SelectedIndexChanged);
            // 
            // btnProcShow
            // 
            this.btnProcShow.Location = new System.Drawing.Point(10, 196);
            this.btnProcShow.Name = "btnProcShow";
            this.btnProcShow.Size = new System.Drawing.Size(170, 80);
            this.btnProcShow.TabIndex = 7;
            this.btnProcShow.Text = "Показать список выбранных процедур";
            this.btnProcShow.UseVisualStyleBackColor = true;
            this.btnProcShow.Click += new System.EventHandler(this.btnProcShow_Click);
            // 
            // btnFuncShow
            // 
            this.btnFuncShow.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnFuncShow.Location = new System.Drawing.Point(10, 136);
            this.btnFuncShow.Name = "btnFuncShow";
            this.btnFuncShow.Size = new System.Drawing.Size(170, 54);
            this.btnFuncShow.TabIndex = 6;
            this.btnFuncShow.Text = "Показать список выбранных функций";
            this.btnFuncShow.UseVisualStyleBackColor = true;
            this.btnFuncShow.Click += new System.EventHandler(this.btnFuncShow_Click);
            // 
            // btnTableShow
            // 
            this.btnTableShow.Location = new System.Drawing.Point(10, 75);
            this.btnTableShow.Name = "btnTableShow";
            this.btnTableShow.Size = new System.Drawing.Size(170, 55);
            this.btnTableShow.TabIndex = 5;
            this.btnTableShow.Text = "Показать список выбранных таблиц";
            this.btnTableShow.UseVisualStyleBackColor = true;
            this.btnTableShow.Click += new System.EventHandler(this.btnTableShow_Click);
            // 
            // convertToPostgresButton
            // 
            this.convertToPostgresButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.convertToPostgresButton.Location = new System.Drawing.Point(10, 451);
            this.convertToPostgresButton.Name = "convertToPostgresButton";
            this.convertToPostgresButton.Size = new System.Drawing.Size(255, 80);
            this.convertToPostgresButton.TabIndex = 4;
            this.convertToPostgresButton.Text = "Конвертировать и сохранить преобразованные SQL скрипты";
            this.convertToPostgresButton.UseVisualStyleBackColor = true;
            this.convertToPostgresButton.Click += new System.EventHandler(this.convertToPostgresButton_Click);
            // 
            // exportButton
            // 
            this.exportButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.exportButton.Location = new System.Drawing.Point(36, 391);
            this.exportButton.Name = "exportButton";
            this.exportButton.Size = new System.Drawing.Size(229, 54);
            this.exportButton.TabIndex = 3;
            this.exportButton.Text = "Сохранить оригинальные SQL скрипты";
            this.exportButton.UseVisualStyleBackColor = true;
            this.exportButton.Click += new System.EventHandler(this.exportButton_Click);
            // 
            // listBoxTables
            // 
            this.listBoxTables.FormattingEnabled = true;
            this.listBoxTables.ItemHeight = 20;
            this.listBoxTables.Location = new System.Drawing.Point(186, 75);
            this.listBoxTables.Name = "listBoxTables";
            this.listBoxTables.Size = new System.Drawing.Size(285, 264);
            this.listBoxTables.TabIndex = 0;
            this.listBoxTables.SelectedIndexChanged += new System.EventHandler(this.listBoxTables_SelectedIndexChanged);
            // 
            // panelResizeMenu
            // 
            this.panelResizeMenu.BackColor = System.Drawing.Color.DarkGray;
            this.panelResizeMenu.Cursor = System.Windows.Forms.Cursors.VSplit;
            this.panelResizeMenu.Dock = System.Windows.Forms.DockStyle.Left;
            this.panelResizeMenu.Location = new System.Drawing.Point(277, 0);
            this.panelResizeMenu.Name = "panelResizeMenu";
            this.panelResizeMenu.Size = new System.Drawing.Size(5, 544);
            this.panelResizeMenu.TabIndex = 0;
            this.panelResizeMenu.MouseDown += new System.Windows.Forms.MouseEventHandler(this.panelResizeMenu_MouseDown);
            this.panelResizeMenu.MouseMove += new System.Windows.Forms.MouseEventHandler(this.panelResizeMenu_MouseMove);
            this.panelResizeMenu.MouseUp += new System.Windows.Forms.MouseEventHandler(this.panelResizeMenu_MouseUp);
            // 
            // AnalizForm
            // 
            this.ClientSize = new System.Drawing.Size(753, 544);
            this.Controls.Add(this.panelResizeMenu);
            this.Controls.Add(this.panelContent);
            this.Controls.Add(this.panelMenu);
            this.Name = "AnalizForm";
            this.Text = "Main Layout";
            this.panelMenu.ResumeLayout(false);
            this.panelMenu.PerformLayout();
            this.panelContent.ResumeLayout(false);
            this.panelContent.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private ListBox listBoxTables;
        private Label label2;
        private Label label1;
        private Button convertToPostgresButton;
        private Button exportButton;
        private Button btnProcShow;
        private Button btnFuncShow;
        private Button btnTableShow;
        private ListBox listBoxFunctions;
        private ListBox listBoxProcedures;
        private ListBox listBoxTrigger;
        private Button btnTrigShow;
        private Label label3;
        private Button button2;
        private Button button1;
        private Label label4;
        private Label label5;
        private Label label6;
    }
}