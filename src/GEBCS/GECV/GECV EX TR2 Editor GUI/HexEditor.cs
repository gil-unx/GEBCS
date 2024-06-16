using GECV_EX.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GECV_EX_TR2_Editor_GUI
{
    public partial class Form_HexEditor : Form
    {

        public string data_type;

        public bool Locker = true;

        public int row_number;

        public int column_number;

        public bool can_save;



        public Form_HexEditor(int row_number, int column_number, string data_type, bool OnlyHex)
        {
            InitializeComponent();

            //this.RichTextBox_Hex.LanguageOption = System.Windows.Forms.RichTextBoxLanguageOptions.UIFonts;
            //this.RichTextBox_Text.LanguageOption = System.Windows.Forms.RichTextBoxLanguageOptions.UIFonts;

            this.data_type = data_type;

            this.RichTextBox_Hex.Text = Main.System_DataTable_Hex.Rows[row_number][column_number].ToString();

            this.RichTextBox_Text.Text = ParseHex(this.RichTextBox_Hex.Text);

            this.RichTextBox_Hex.ForeColor = Color.Blue;

            this.RichTextBox_Text.ReadOnly = OnlyHex;

            this.row_number = row_number;
            this.column_number = column_number;

            this.Text = $"{this.Text} : {data_type} Row {row_number} Column {column_number}";
            can_save = true;
            Locker = false;

            this.Label_ToolTips.Text += this.data_type;

        }

        private void RichTextBox_Text_TextChanged(object sender, EventArgs e)
        {

            if (Locker)
            {
                return;
            }
            else
            {
                Locker = true;
            }


            byte[] bytes = ParseString(this.RichTextBox_Text.Text);

            this.RichTextBox_Hex.Text = FileUtils.GetByteArrayString(bytes);


            Locker = false;

        }

        private string ParseHex(string hex)
        {
            return ParseHex(hex, data_type);
        }


        public static string ParseHex(string hex,string data_type)
        {


            var bytes = FileUtils.GetBytesByHexString(hex);

            string xdata;

            switch (data_type)
            {
                case "ASCII":
                    xdata = Encoding.ASCII.GetString(bytes);
                    break;
                case "UTF-16LE":
                    xdata = Encoding.Unicode.GetString(bytes);
                    break;
                case "UTF-8":
                    xdata = Encoding.UTF8.GetString(bytes);
                    break;
                case "UTF-16":
                    xdata = Encoding.Unicode.GetString(bytes); //BIG?
                    break;
                default:
                    throw new ArgumentException($"What is {data_type}?");
            }

            //MessageBox.Show($"{hex}:{xdata}:{bytes.Length}");
            return xdata;
        }

        private byte[] ParseString(string str)
        {

            byte[] xdata;

            switch (data_type)
            {
                case "ASCII":
                    xdata = Encoding.ASCII.GetBytes(str);
                    break;
                case "UTF-16LE":
                    xdata = Encoding.Unicode.GetBytes(str);
                    break;
                case "UTF-8":
                    xdata = Encoding.UTF8.GetBytes(str);
                    break;
                case "UTF-16":
                    xdata = Encoding.Unicode.GetBytes(str); //BIG?
                    break;
                default:
                    throw new ArgumentException($"What is {data_type}?");
            }


            return xdata;
        }

        private void RichTextBox_Hex_TextChanged(object sender, EventArgs e)
        {

            if (Locker)
            {
                return;
            }
            else
            {
                Locker = true;
            }


            try
            {

                this.RichTextBox_Text.Text = ParseHex(this.RichTextBox_Hex.Text);
                can_save = true;


            }
            catch (Exception ex)
            {


                this.RichTextBox_Hex.ForeColor = Color.Red;
                Locker = false;
                can_save = false;
                return;

            }




            this.RichTextBox_Hex.ForeColor = Color.Blue;

            Locker = false;

        }

        private void Button_Save_Click(object sender, EventArgs e)
        {

            if (!CheckCanSave())
            {
                return;
            }

            Main.System_DataTable_Hex.Rows[row_number][column_number] = this.RichTextBox_Hex.Text;

            Main.System_DataTable.Rows[row_number][column_number] = this.RichTextBox_Text.Text;

            this.Close();



        }


        private bool CheckCanSave()
        {
            if (!can_save)
            {
                MessageBox.Show($"Cannot Save Beacause Hex Is Wrong!\nCheck Your Data Or Exit!", "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            return true;
        }

        private void Button_Export_Click(object sender, EventArgs e)
        {
            if (!CheckCanSave())
            {

                return;
            }


            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.Filter = "txt file(*.txt)|*.txt";
                sfd.RestoreDirectory = true;
                sfd.Title = "Export File:";
                sfd.FileName = $"GECV_EX_HEX_Editor.txt";

                if (sfd.ShowDialog() == DialogResult.OK)
                {

                    byte[] data = FileUtils.GetBytesByHexString(this.RichTextBox_Hex.Text);


                    File.WriteAllBytes(sfd.FileName, data);



                }

            }


        }

        private void Button_Import_Click(object sender, EventArgs e)
        {
            this.Locker = true;

            using (OpenFileDialog ofd = new OpenFileDialog())
            {

                ofd.RestoreDirectory = true;
                ofd.Multiselect = false;
                ofd.Title = "Import TXT File (No File Check):";
                ofd.Filter = "txt file(*.txt)|*.txt";
                ofd.RestoreDirectory = false;


                if (ofd.ShowDialog() == DialogResult.OK)
                {

                    byte[] data = File.ReadAllBytes(ofd.FileName);


                    string hex_str =  FileUtils.GetByteArrayString(data);

                    this.RichTextBox_Hex.Text = hex_str;

                    this.RichTextBox_Text.Text = ParseHex(this.RichTextBox_Hex.Text);



                    this.RichTextBox_Hex.ForeColor = Color.Green;
                }
            }

            this.Locker = false;
        }
    }
}
