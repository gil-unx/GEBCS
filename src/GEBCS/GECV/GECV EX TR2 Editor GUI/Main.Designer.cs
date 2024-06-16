namespace GECV_EX_TR2_Editor_GUI
{
    partial class Main
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            MenuSet_Main = new MenuStrip();
            MenuItem_AutoOpen = new ToolStripMenuItem();
            MenuItem_Open = new ToolStripMenuItem();
            MenuItem_SaveTr2 = new ToolStripMenuItem();
            MenuItem_Save = new ToolStripMenuItem();
            MenuItem_Import = new ToolStripMenuItem();
            MenuItem_Export = new ToolStripMenuItem();
            importExcelToolStripMenuItem = new ToolStripMenuItem();
            MenuItem_Excel_Export = new ToolStripMenuItem();
            MenuItem_Old = new ToolStripMenuItem();
            MenuItem_OldOpen = new ToolStripMenuItem();
            MenuItem_OldSave = new ToolStripMenuItem();
            MenuItem_Help = new ToolStripMenuItem();
            MenuItem_Tr2VersionHelp = new ToolStripMenuItem();
            MenuStatus_Main = new StatusStrip();
            DataGridView_Main = new DataGridView();
            MenuSet_Main.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)DataGridView_Main).BeginInit();
            SuspendLayout();
            // 
            // MenuSet_Main
            // 
            MenuSet_Main.Items.AddRange(new ToolStripItem[] { MenuItem_AutoOpen, MenuItem_Open, MenuItem_SaveTr2, MenuItem_Save, MenuItem_Import, MenuItem_Export, importExcelToolStripMenuItem, MenuItem_Excel_Export, MenuItem_Old, MenuItem_Help });
            MenuSet_Main.Location = new Point(0, 0);
            MenuSet_Main.Name = "MenuSet_Main";
            MenuSet_Main.Size = new Size(1008, 25);
            MenuSet_Main.TabIndex = 0;
            MenuSet_Main.Text = "MenuSet_Main";
            // 
            // MenuItem_AutoOpen
            // 
            MenuItem_AutoOpen.Name = "MenuItem_AutoOpen";
            MenuItem_AutoOpen.Size = new Size(106, 21);
            MenuItem_AutoOpen.Text = "Auto Open Tr2";
            MenuItem_AutoOpen.Click += MenuItem_AutoOpen_Click;
            // 
            // MenuItem_Open
            // 
            MenuItem_Open.Enabled = false;
            MenuItem_Open.Name = "MenuItem_Open";
            MenuItem_Open.Size = new Size(109, 21);
            MenuItem_Open.Text = "Open Ver.2 Tr2";
            MenuItem_Open.Click += MenuItem_Open_Click;
            // 
            // MenuItem_SaveTr2
            // 
            MenuItem_SaveTr2.Name = "MenuItem_SaveTr2";
            MenuItem_SaveTr2.Size = new Size(104, 21);
            MenuItem_SaveTr2.Text = "Save Ver.2 Tr2";
            MenuItem_SaveTr2.Click += MenuItem_SaveTr2_Click;
            // 
            // MenuItem_Save
            // 
            MenuItem_Save.Name = "MenuItem_Save";
            MenuItem_Save.Size = new Size(73, 21);
            MenuItem_Save.Text = "Save Xml";
            MenuItem_Save.Click += MenuItem_Save_Click;
            // 
            // MenuItem_Import
            // 
            MenuItem_Import.Name = "MenuItem_Import";
            MenuItem_Import.Size = new Size(88, 21);
            MenuItem_Import.Text = "Import Text";
            MenuItem_Import.Click += MenuItem_Import_Click;
            // 
            // MenuItem_Export
            // 
            MenuItem_Export.Name = "MenuItem_Export";
            MenuItem_Export.Size = new Size(86, 21);
            MenuItem_Export.Text = "Export Text";
            MenuItem_Export.Click += MenuItem_Export_Click;
            // 
            // importExcelToolStripMenuItem
            // 
            importExcelToolStripMenuItem.Enabled = false;
            importExcelToolStripMenuItem.Name = "importExcelToolStripMenuItem";
            importExcelToolStripMenuItem.Size = new Size(93, 21);
            importExcelToolStripMenuItem.Text = "Import Excel";
            importExcelToolStripMenuItem.Click += importExcelToolStripMenuItem_Click;
            // 
            // MenuItem_Excel_Export
            // 
            MenuItem_Excel_Export.Name = "MenuItem_Excel_Export";
            MenuItem_Excel_Export.Size = new Size(91, 21);
            MenuItem_Excel_Export.Text = "Export Excel";
            MenuItem_Excel_Export.Click += exportExcelToolStripMenuItem_Click;
            // 
            // MenuItem_Old
            // 
            MenuItem_Old.DropDownItems.AddRange(new ToolStripItem[] { MenuItem_OldOpen, MenuItem_OldSave });
            MenuItem_Old.Name = "MenuItem_Old";
            MenuItem_Old.Size = new Size(110, 21);
            MenuItem_Old.Text = "Ver.1 Tr2 Menu";
            // 
            // MenuItem_OldOpen
            // 
            MenuItem_OldOpen.Enabled = false;
            MenuItem_OldOpen.Name = "MenuItem_OldOpen";
            MenuItem_OldOpen.Size = new Size(180, 22);
            MenuItem_OldOpen.Text = "Open Ver.1 Tr2";
            MenuItem_OldOpen.Click += MenuItem_OldOpen_Click;
            // 
            // MenuItem_OldSave
            // 
            MenuItem_OldSave.Name = "MenuItem_OldSave";
            MenuItem_OldSave.Size = new Size(180, 22);
            MenuItem_OldSave.Text = "Save Ver.1 Tr2";
            MenuItem_OldSave.Click += MenuItem_OldSave_Click;
            // 
            // MenuItem_Help
            // 
            MenuItem_Help.DropDownItems.AddRange(new ToolStripItem[] { MenuItem_Tr2VersionHelp });
            MenuItem_Help.Name = "MenuItem_Help";
            MenuItem_Help.Size = new Size(55, 21);
            MenuItem_Help.Text = "About";
            MenuItem_Help.Click += MenuItem_Help_Click;
            // 
            // MenuItem_Tr2VersionHelp
            // 
            MenuItem_Tr2VersionHelp.Name = "MenuItem_Tr2VersionHelp";
            MenuItem_Tr2VersionHelp.Size = new Size(196, 22);
            MenuItem_Tr2VersionHelp.Text = "What is Tr2 Version?";
            MenuItem_Tr2VersionHelp.Click += MenuItem_Tr2VersionHelp_Click;
            // 
            // MenuStatus_Main
            // 
            MenuStatus_Main.Location = new Point(0, 707);
            MenuStatus_Main.Name = "MenuStatus_Main";
            MenuStatus_Main.Size = new Size(1008, 22);
            MenuStatus_Main.TabIndex = 2;
            MenuStatus_Main.Text = "MenuStatus_Main";
            // 
            // DataGridView_Main
            // 
            DataGridView_Main.AllowUserToAddRows = false;
            DataGridView_Main.AllowUserToDeleteRows = false;
            DataGridView_Main.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            DataGridView_Main.Dock = DockStyle.Fill;
            DataGridView_Main.Location = new Point(0, 25);
            DataGridView_Main.Name = "DataGridView_Main";
            DataGridView_Main.Size = new Size(1008, 682);
            DataGridView_Main.TabIndex = 1;
            DataGridView_Main.VirtualMode = true;
            DataGridView_Main.CellDoubleClick += DataGridView_Main_CellDoubleClick;
            DataGridView_Main.ColumnAdded += DataGridView_Main_ColumnAdded;
            // 
            // Main
            // 
            AllowDrop = true;
            AutoScaleDimensions = new SizeF(7F, 17F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1008, 729);
            Controls.Add(DataGridView_Main);
            Controls.Add(MenuStatus_Main);
            Controls.Add(MenuSet_Main);
            MainMenuStrip = MenuSet_Main;
            Name = "Main";
            Text = "GECV EX TR2 Editor GUI";
            FormClosing += Main_FormClosing;
            Load += Main_Load;
            DragDrop += Main_DragDrop;
            DragEnter += Main_DragEnter;
            MenuSet_Main.ResumeLayout(false);
            MenuSet_Main.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)DataGridView_Main).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private MenuStrip MenuSet_Main;
        private ToolStripMenuItem MenuItem_Open;
        private ToolStripMenuItem MenuItem_Save;
        private StatusStrip MenuStatus_Main;
        private DataGridView DataGridView_Main;
        private ToolStripMenuItem MenuItem_Import;
        private ToolStripMenuItem MenuItem_Export;
        private ToolStripMenuItem MenuItem_Help;
        private ToolStripMenuItem MenuItem_Excel_Export;
        private ToolStripMenuItem MenuItem_SaveTr2;
        private ToolStripMenuItem importExcelToolStripMenuItem;
        private ToolStripMenuItem MenuItem_Old;
        private ToolStripMenuItem MenuItem_OldOpen;
        private ToolStripMenuItem MenuItem_OldSave;
        private ToolStripMenuItem MenuItem_AutoOpen;
        private ToolStripMenuItem MenuItem_Tr2VersionHelp;
    }
}
