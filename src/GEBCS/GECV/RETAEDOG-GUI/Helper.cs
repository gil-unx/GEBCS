using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TextBox;

namespace RETAEDOG_GUI
{
    public static class Helper
    {

        public static int Progress_Current = 0;
        public static int Progress_Total = 0;


        public static void BackupGame(string gamefolder)
        {

            var files = Directory.GetFiles(gamefolder, "*.*", SearchOption.AllDirectories);

            CopyGameFiles(gamefolder, files, Path.Combine(gamefolder, "GameBackup"));
            


            



        }

        //public static void CopyDirectory(string origin, string target, SearchOption option, bool overwrite)
        //{

        //    string relative_path_root = Path.GetDirectoryName(origin);



        //    var dirs = Directory.GetFiles(target, "*", option);


        //    Parallel.ForEach(dirs, dir =>
        //    {

        //        var files = Directory.GetFiles(dir, "*", SearchOption.TopDirectoryOnly);


        //        Parallel.ForEach(files, file =>
        //        {

        //            string path = file.Substring(relative_path_root.Length);

        //            string target_path = Path.Combine(target, path);

        //            string need_dir_path = Path.GetDirectoryName(target_path);

                    
        //            Directory.CreateDirectory(need_dir_path);

        //            File.Copy(file, target_path,overwrite);




        //        });



        //    });


        //}


        public static void CopyGameFiles(string rootfolder, string[] files, string targetfolder)
        {

            Progress_Total = files.Length;

            if (!Directory.Exists(targetfolder))
            {
                Directory.CreateDirectory(targetfolder);
            }


            Parallel.ForEach(files, file =>
            {
                string path = file.Substring(rootfolder.Length);

                string target_path = Path.Combine(targetfolder, path);

                string need_dir_path = Path.GetDirectoryName(target_path);


                Directory.CreateDirectory(need_dir_path);

                File.Copy(file, target_path, true);

                Progress_Current++;

            });

        }



    }
}
