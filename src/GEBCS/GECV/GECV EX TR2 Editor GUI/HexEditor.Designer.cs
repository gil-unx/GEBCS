namespace GECV_EX_TR2_Editor_GUI
{
    partial class Form_HexEditor
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            TableLayoutPanel_Main = new TableLayoutPanel();
            Button_Save = new Button();
            TableLayoutPanel_Extra = new TableLayoutPanel();
            Button_Import = new Button();
            Button_Export = new Button();
            RichTextBox_Text = new RichTextBox();
            RichTextBox_Hex = new RichTextBox();
            Label_ToolTips = new Label();
            TableLayoutPanel_Main.SuspendLayout();
            TableLayoutPanel_Extra.SuspendLayout();
            SuspendLayout();
            // 
            // TableLayoutPanel_Main
            // 
            TableLayoutPanel_Main.ColumnCount = 2;
            TableLayoutPanel_Main.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            TableLayoutPanel_Main.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            TableLayoutPanel_Main.Controls.Add(Button_Save, 0, 2);
            TableLayoutPanel_Main.Controls.Add(TableLayoutPanel_Extra, 1, 2);
            TableLayoutPanel_Main.Controls.Add(RichTextBox_Text, 0, 1);
            TableLayoutPanel_Main.Controls.Add(RichTextBox_Hex, 1, 1);
            TableLayoutPanel_Main.Controls.Add(Label_ToolTips, 1, 0);
            TableLayoutPanel_Main.Dock = DockStyle.Fill;
            TableLayoutPanel_Main.Location = new Point(0, 0);
            TableLayoutPanel_Main.Name = "TableLayoutPanel_Main";
            TableLayoutPanel_Main.RowCount = 3;
            TableLayoutPanel_Main.RowStyles.Add(new RowStyle(SizeType.Percent, 10F));
            TableLayoutPanel_Main.RowStyles.Add(new RowStyle(SizeType.Percent, 80F));
            TableLayoutPanel_Main.RowStyles.Add(new RowStyle(SizeType.Percent, 10F));
            TableLayoutPanel_Main.Size = new Size(800, 450);
            TableLayoutPanel_Main.TabIndex = 0;
            // 
            // Button_Save
            // 
            Button_Save.Dock = DockStyle.Fill;
            Button_Save.Location = new Point(3, 408);
            Button_Save.Name = "Button_Save";
            Button_Save.Size = new Size(394, 39);
            Button_Save.TabIndex = 0;
            Button_Save.Text = "Save Data";
            Button_Save.UseVisualStyleBackColor = true;
            Button_Save.Click += Button_Save_Click;
            // 
            // TableLayoutPanel_Extra
            // 
            TableLayoutPanel_Extra.ColumnCount = 2;
            TableLayoutPanel_Extra.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            TableLayoutPanel_Extra.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            TableLayoutPanel_Extra.Controls.Add(Button_Import, 0, 0);
            TableLayoutPanel_Extra.Controls.Add(Button_Export, 1, 0);
            TableLayoutPanel_Extra.Dock = DockStyle.Fill;
            TableLayoutPanel_Extra.Location = new Point(403, 408);
            TableLayoutPanel_Extra.Name = "TableLayoutPanel_Extra";
            TableLayoutPanel_Extra.RowCount = 1;
            TableLayoutPanel_Extra.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            TableLayoutPanel_Extra.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            TableLayoutPanel_Extra.Size = new Size(394, 39);
            TableLayoutPanel_Extra.TabIndex = 1;
            // 
            // Button_Import
            // 
            Button_Import.Dock = DockStyle.Fill;
            Button_Import.Location = new Point(3, 3);
            Button_Import.Name = "Button_Import";
            Button_Import.Size = new Size(191, 33);
            Button_Import.TabIndex = 0;
            Button_Import.Text = "Import Data";
            Button_Import.UseVisualStyleBackColor = true;
            Button_Import.Click += Button_Import_Click;
            // 
            // Button_Export
            // 
            Button_Export.Dock = DockStyle.Fill;
            Button_Export.Location = new Point(200, 3);
            Button_Export.Name = "Button_Export";
            Button_Export.Size = new Size(191, 33);
            Button_Export.TabIndex = 1;
            Button_Export.Text = "Export Data";
            Button_Export.UseVisualStyleBackColor = true;
            Button_Export.Click += Button_Export_Click;
            // 
            // RichTextBox_Text
            // 
            RichTextBox_Text.DetectUrls = false;
            RichTextBox_Text.Dock = DockStyle.Fill;
            RichTextBox_Text.Location = new Point(3, 48);
            RichTextBox_Text.Name = "RichTextBox_Text";
            RichTextBox_Text.Size = new Size(394, 354);
            RichTextBox_Text.TabIndex = 2;
            RichTextBox_Text.Text = "";
            RichTextBox_Text.TextChanged += RichTextBox_Text_TextChanged;
            // 
            // RichTextBox_Hex
            // 
            RichTextBox_Hex.DetectUrls = false;
            RichTextBox_Hex.Dock = DockStyle.Fill;
            RichTextBox_Hex.ImeMode = ImeMode.NoControl;
            RichTextBox_Hex.Location = new Point(403, 48);
            RichTextBox_Hex.Name = "RichTextBox_Hex";
            RichTextBox_Hex.Size = new Size(394, 354);
            RichTextBox_Hex.TabIndex = 3;
            RichTextBox_Hex.Text = "";
            RichTextBox_Hex.TextChanged += RichTextBox_Hex_TextChanged;
            // 
            // Label_ToolTips
            // 
            Label_ToolTips.AutoSize = true;
            Label_ToolTips.Dock = DockStyle.Fill;
            Label_ToolTips.Location = new Point(403, 0);
            Label_ToolTips.Name = "Label_ToolTips";
            Label_ToolTips.Size = new Size(394, 45);
            Label_ToolTips.TabIndex = 4;
            Label_ToolTips.Text = "If you export txt file and open it by other editor, you must set file encoding to ";
            // 
            // Form_HexEditor
            // 
            AutoScaleDimensions = new SizeF(7F, 17F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(TableLayoutPanel_Main);
            MinimizeBox = false;
            Name = "Form_HexEditor";
            Text = "Hex Editor";
            TopMost = true;
            TableLayoutPanel_Main.ResumeLayout(false);
            TableLayoutPanel_Main.PerformLayout();
            TableLayoutPanel_Extra.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private TableLayoutPanel TableLayoutPanel_Main;
        private Button Button_Save;
        private TableLayoutPanel TableLayoutPanel_Extra;
        private Button Button_Import;
        private Button Button_Export;
        private RichTextBox RichTextBox_Text;
        private RichTextBox RichTextBox_Hex;
        private Label Label_ToolTips;
    }
}