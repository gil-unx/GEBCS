namespace RETAEDOG_GUI
{
    partial class Main
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Main));
            this.TableLayoutPanel_Main = new System.Windows.Forms.TableLayoutPanel();
            this.Picture_Main = new System.Windows.Forms.PictureBox();
            this.FlowLayoutPanel_Button = new System.Windows.Forms.FlowLayoutPanel();
            this.Button_Install = new System.Windows.Forms.Button();
            this.Button_Build = new System.Windows.Forms.Button();
            this.Button_Restore = new System.Windows.Forms.Button();
            this.Button_Create = new System.Windows.Forms.Button();
            this.Progress_Main = new System.Windows.Forms.ProgressBar();
            this.TableLayoutPanel_Main.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.Picture_Main)).BeginInit();
            this.FlowLayoutPanel_Button.SuspendLayout();
            this.SuspendLayout();
            // 
            // TableLayoutPanel_Main
            // 
            this.TableLayoutPanel_Main.ColumnCount = 1;
            this.TableLayoutPanel_Main.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.TableLayoutPanel_Main.Controls.Add(this.Picture_Main, 0, 0);
            this.TableLayoutPanel_Main.Controls.Add(this.FlowLayoutPanel_Button, 0, 1);
            this.TableLayoutPanel_Main.Controls.Add(this.Progress_Main, 0, 2);
            this.TableLayoutPanel_Main.Dock = System.Windows.Forms.DockStyle.Fill;
            this.TableLayoutPanel_Main.Location = new System.Drawing.Point(0, 0);
            this.TableLayoutPanel_Main.Margin = new System.Windows.Forms.Padding(4);
            this.TableLayoutPanel_Main.Name = "TableLayoutPanel_Main";
            this.TableLayoutPanel_Main.RowCount = 3;
            this.TableLayoutPanel_Main.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 40F));
            this.TableLayoutPanel_Main.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.TableLayoutPanel_Main.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 10F));
            this.TableLayoutPanel_Main.Size = new System.Drawing.Size(624, 281);
            this.TableLayoutPanel_Main.TabIndex = 0;
            // 
            // Picture_Main
            // 
            this.Picture_Main.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Picture_Main.Location = new System.Drawing.Point(0, 0);
            this.Picture_Main.Margin = new System.Windows.Forms.Padding(0);
            this.Picture_Main.Name = "Picture_Main";
            this.Picture_Main.Size = new System.Drawing.Size(624, 112);
            this.Picture_Main.TabIndex = 0;
            this.Picture_Main.TabStop = false;
            // 
            // FlowLayoutPanel_Button
            // 
            this.FlowLayoutPanel_Button.Controls.Add(this.Button_Install);
            this.FlowLayoutPanel_Button.Controls.Add(this.Button_Build);
            this.FlowLayoutPanel_Button.Controls.Add(this.Button_Restore);
            this.FlowLayoutPanel_Button.Controls.Add(this.Button_Create);
            this.FlowLayoutPanel_Button.Dock = System.Windows.Forms.DockStyle.Fill;
            this.FlowLayoutPanel_Button.Location = new System.Drawing.Point(4, 116);
            this.FlowLayoutPanel_Button.Margin = new System.Windows.Forms.Padding(4);
            this.FlowLayoutPanel_Button.Name = "FlowLayoutPanel_Button";
            this.FlowLayoutPanel_Button.Size = new System.Drawing.Size(616, 132);
            this.FlowLayoutPanel_Button.TabIndex = 1;
            // 
            // Button_Install
            // 
            this.Button_Install.Location = new System.Drawing.Point(4, 4);
            this.Button_Install.Margin = new System.Windows.Forms.Padding(4);
            this.Button_Install.Name = "Button_Install";
            this.Button_Install.Size = new System.Drawing.Size(125, 25);
            this.Button_Install.TabIndex = 0;
            this.Button_Install.Text = "Install Mod Pack";
            this.Button_Install.UseVisualStyleBackColor = true;
            this.Button_Install.Click += new System.EventHandler(this.Button_Install_Click);
            // 
            // Button_Build
            // 
            this.Button_Build.Enabled = false;
            this.Button_Build.Location = new System.Drawing.Point(137, 4);
            this.Button_Build.Margin = new System.Windows.Forms.Padding(4);
            this.Button_Build.Name = "Button_Build";
            this.Button_Build.Size = new System.Drawing.Size(125, 25);
            this.Button_Build.TabIndex = 1;
            this.Button_Build.Text = "Build Mod Pack";
            this.Button_Build.UseVisualStyleBackColor = true;
            this.Button_Build.Click += new System.EventHandler(this.Button_Build_Click);
            // 
            // Button_Restore
            // 
            this.Button_Restore.Enabled = false;
            this.Button_Restore.Location = new System.Drawing.Point(270, 4);
            this.Button_Restore.Margin = new System.Windows.Forms.Padding(4);
            this.Button_Restore.Name = "Button_Restore";
            this.Button_Restore.Size = new System.Drawing.Size(125, 25);
            this.Button_Restore.TabIndex = 2;
            this.Button_Restore.Text = "Restore Game Data";
            this.Button_Restore.UseVisualStyleBackColor = true;
            this.Button_Restore.Click += new System.EventHandler(this.Button_Restore_Click);
            // 
            // Button_Create
            // 
            this.Button_Create.Location = new System.Drawing.Point(402, 3);
            this.Button_Create.Name = "Button_Create";
            this.Button_Create.Size = new System.Drawing.Size(125, 25);
            this.Button_Create.TabIndex = 3;
            this.Button_Create.Text = "Create Empty Mod";
            this.Button_Create.UseVisualStyleBackColor = true;
            this.Button_Create.Click += new System.EventHandler(this.Button_Create_Click);
            // 
            // Progress_Main
            // 
            this.Progress_Main.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Progress_Main.Location = new System.Drawing.Point(3, 255);
            this.Progress_Main.Maximum = 10000;
            this.Progress_Main.Name = "Progress_Main";
            this.Progress_Main.Size = new System.Drawing.Size(618, 23);
            this.Progress_Main.TabIndex = 2;
            // 
            // Main
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(624, 281);
            this.Controls.Add(this.TableLayoutPanel_Main);
            this.Font = new System.Drawing.Font("Times New Roman", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "Main";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "RETAE DOG";
            this.TopMost = true;
            this.Load += new System.EventHandler(this.Main_Load);
            this.TableLayoutPanel_Main.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.Picture_Main)).EndInit();
            this.FlowLayoutPanel_Button.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel TableLayoutPanel_Main;
        private System.Windows.Forms.PictureBox Picture_Main;
        private System.Windows.Forms.FlowLayoutPanel FlowLayoutPanel_Button;
        private System.Windows.Forms.Button Button_Install;
        private System.Windows.Forms.Button Button_Build;
        private System.Windows.Forms.Button Button_Restore;
        private System.Windows.Forms.ProgressBar Progress_Main;
        private System.Windows.Forms.Button Button_Create;
    }
}

