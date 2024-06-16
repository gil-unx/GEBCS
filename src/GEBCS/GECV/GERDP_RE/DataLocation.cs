using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GERDP_RE
{

    internal enum DataLocationStatus
    {
        NoSet_3, Package_4, Data_5, Patch_6, Current_C, UNK
    }

    internal class DataLocation
    {

        private long origin;
        private bool isPS4;

        public FileInfo res_file { get; private set; }

        public DataLocationStatus status { get; private set; }
        public long real_offset { get;private set; }

        public string hex_origin { get; private set; }
        public string hex_origin_fix { get; private set; }

        private DataLocation() { }


        public long CalcRealOffset()
        {
            
            if(this.status == DataLocationStatus.Package_4 || this.status == DataLocationStatus.Data_5 || this.status == DataLocationStatus.Patch_6)
            {
                return  real_offset * 0x800;
            }
            return real_offset;
        }


        public DataLocation(long origin, FileInfo res_file,bool isPS4)
        {
            this.origin = origin;
            this.isPS4 = isPS4;
            this.res_file = res_file;

            string visual_name;

            if(isPS4 )
            {
                visual_name = origin.ToString("X16");
                hex_origin = visual_name;
                real_offset = Convert.ToInt64(visual_name.Substring(1),16);
            }
            else
            {


                visual_name = origin.ToString("X8");
                hex_origin = visual_name;
                visual_name = visual_name.Length > 8 ? visual_name.Substring(8) : visual_name;

                real_offset = Convert.ToInt32(visual_name.Substring(1), 16);
            }

            switch (visual_name[0])
            {
                case '3':
                    this.status = DataLocationStatus.NoSet_3;
                    break;
                case '4':
                    this.status = DataLocationStatus.Package_4;
                    break;
                case '5':
                    this.status = DataLocationStatus.Data_5;
                    break;
                case '6':
                    this.status = DataLocationStatus.Patch_6;
                    break;
                case 'C':
                    this.status = DataLocationStatus.Current_C;
                    break;
                default:
                    this.status = DataLocationStatus.UNK;
                    break;
                
            }

            
            this.res_file =  GetDataByLocation(this);
            hex_origin_fix = visual_name;

        }


        private FileInfo GetDataByLocation(DataLocation dl) //还是传过来吧，放在这有点抽象了。
        {

            switch (dl.status)
            {
                case DataLocationStatus.NoSet_3:
                    return null;
                case DataLocationStatus.Package_4:
                    return Program.PackageRDP;
                case DataLocationStatus.Data_5:
                    return Program.DataRDP;
                case DataLocationStatus.Patch_6:
                    return Program.PatchRDP;
                case DataLocationStatus.Current_C:
                    return this.res_file;
                default:
                    return null;
            }


        }

        public string GetDataLocationName()
        {
            switch (this.status)
            {
                case DataLocationStatus.NoSet_3:
                    return "外部文件（3）";
                case DataLocationStatus.Package_4:
                    return "package.rdp（4）";
                case DataLocationStatus.Data_5:
                    return "data.rdp（5）";
                case DataLocationStatus.Patch_6:
                    return "patch.rdp（6）";
                case DataLocationStatus.Current_C:
                    return "包内文件（C）";
                default:
                    return $"未知（{hex_origin[0]}）";
            }
        }








    }
}
