using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace RETAEDOG_GUI
{
    public partial class Main : Form
    {


        public static Thread ProgressRefresher;



        public Main()
        {
            InitializeComponent();
        }

        private void Button_Install_Click(object sender, EventArgs e)
        {

            OpenFileDialog dialog = new OpenFileDialog();

            

            dialog.Filter = "God Eater Mod Pack Info|*.gemodinfo";
            dialog.Title = "Select God Eater Mod Pack Info:";
            dialog.Multiselect = false;
            BinaryModPack modpack;
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                modpack = BinaryModPack.GetModInfo(dialog.FileName);




            }
            else
            {
                return;
            }



            dialog.Filter = $"Target Game For Install Mod|{Path.GetFileName(dialog.FileName)}";



        }

        private void Button_Build_Click(object sender, EventArgs e)
        {

        }

        private void Button_Restore_Click(object sender, EventArgs e)
        {

        }

        private void Button_Create_Click(object sender, EventArgs e)
        {

            //SaveFileDialog saveFileDialog = new SaveFileDialog();
            //saveFileDialog.Filter = "God Eater Mod Pack Info|*.gemodinfo";
            //saveFileDialog.Title = "Select A Blank Directory To Create A Project:";


            FolderBrowserDialog dialog = new FolderBrowserDialog();

            if(dialog.ShowDialog() == DialogResult.OK)
            {

                if(!Directory.Exists(dialog.SelectedPath))
                {
                    Directory.CreateDirectory(dialog.SelectedPath);
                }

                

                Directory.CreateDirectory($"{dialog.SelectedPath}\\bin.qpck");
                Directory.CreateDirectory($"{dialog.SelectedPath}\\bin_patch.qpck");
                Directory.CreateDirectory($"{dialog.SelectedPath}\\conf.qpck");
                Directory.CreateDirectory($"{dialog.SelectedPath}\\data.qpck");
                Directory.CreateDirectory($"{dialog.SelectedPath}\\gamedata");
                Directory.CreateDirectory($"{dialog.SelectedPath}\\gamedata\\data");

                BinaryModPack.CreateEmptyModInfo($"{dialog.SelectedPath}\\GodEater_Mod_Info.xml");



            }
            

        }

        private void Main_Load(object sender, EventArgs e)
        {

            ProgressRefresher = new Thread(() => {

                while(true) { 

                if(Helper.Progress_Current != 0 && Helper.Progress_Total != 0)
                {
                    this.Progress_Main.Value = (int)(Helper.Progress_Current / Helper.Progress_Total) * 100;
                }


                Thread.Sleep(100);


                }

            });


        }
    }
}
