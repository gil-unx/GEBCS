using GECV_EX.Shared.Enum;
using GECV_EX.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace GECV_EX.PC
{



    [Serializable]
    [XmlRoot]
    public class PresPC
    {



        [XmlIgnore]
        public static readonly int PRES_MAGIC = 0x73657250;

        [XmlAttribute]
        public int magic_0; // vaild this = PRES_MAGIC
        [XmlAttribute]
        public int magic_1; //group_offset
        [XmlAttribute]
        public int magic_2; //group_count
        [XmlAttribute]
        public int magic_3; //checksum

        [XmlIgnore]
        private int offset_data; //HeaderBinaryLength

        [XmlIgnore]
        private long zerozero;


        [XmlIgnore]
        private PresCountry[] countries;

        [XmlIgnore]
        public Dictionary<string, string> symbol_map;


        private PresPC() {
        }



        
        public PresPC(byte[] pres_data)
        {

            using (MemoryStream ms = new MemoryStream(pres_data))
            {
                using (BinaryReader br = new BinaryReader(ms))
                {

                    magic_0 = br.ReadInt32();

                    if (magic_0 != PRES_MAGIC)
                    {
                        throw new FileLoadException($"Pres Header Error:{magic_0.ToString("X8")}!={PRES_MAGIC.ToString("X8")}.");
                    }

                    magic_1 = br.ReadInt32();
                    magic_2 = br.ReadInt32();
                    magic_3 = br.ReadInt32();

                    offset_data = br.ReadInt32();
                    zerozero = br.ReadInt64();

                    var count_set = br.ReadInt32();

                    if (count_set <= 0)
                    {
                        throw new FileLoadException($"Pres Count Set Error:{count_set}<=0.");
                    }
                    else
                    {
                        countries = new PresCountry[count_set];
                    }
                    
                    if(count_set > 1)
                    {

                        for (int i = 0; i < countries.Length; i++)
                        {
                            countries[i] = new PresCountry();
                            countries[i].offset = br.ReadInt32();
                            countries[i].length = br.ReadInt32();

                        }
                    }
                    else
                    {
                        countries[0] = new PresCountry();
                        countries[0].offset = (int)br.BaseStream.Position;
                        countries[0].length = 0;
                    }

                    
                    for(int i = 0;i < countries.Length;i++)
                    {
                        PresSet[] pres_set = new PresSet[8];    
                        BuildPresSet(pres_data, br.BaseStream.Position, ref pres_set);


                        countries[i].resources = pres_set;


                    }








                }




            }





        }


        public void Unpack(string dir)
        {
            DirectoryInfo dir_root = new DirectoryInfo(dir);
            symbol_map = new Dictionary<string, string>();
            dir_root.Create();
            
            for (int i = 0; i < countries.Length; i++)
            {
                DirectoryInfo country_dir;

                if (IsNoCountryPres())
                {
                    country_dir = new DirectoryInfo(dir_root.FullName+'\\'+FileUtils.GetOrderName(i,"BLANK"));
                }
                else
                {
                    
                    string enum_name = Enum.GetName(typeof(CountryEnum),i);


                    country_dir = new DirectoryInfo(dir_root.FullName + '\\' + FileUtils.GetOrderName(i, enum_name));
                }

                country_dir.Create();
                
                PresCountry country = countries[i];

                if (country.resources == null || country.resources.Length<=0)
                {
                    Console.WriteLine($"{i + 1}/6 Is Blank Country, No Need Output.");
                    continue;
                }
               
                for(int si = 0;si<country.resources.Length;si++)
                {
                    string enum_name = Enum.GetName(typeof(ResSetEnum), si);
                    DirectoryInfo set_dir = new DirectoryInfo(country_dir.FullName+'\\'+ FileUtils.GetOrderName(si, enum_name));

                    set_dir.Create();

                    PresSet pset = country.resources[si];

                    if (pset.IsNoUsed)
                    {
                        Console.WriteLine($"{si+1}/8 Is Blank Set, No Need Output.");
                        continue;
                    }

                    for(int ssi = 0;ssi<pset.pres_file_set.Length;ssi++)
                    {
                        PresFileData pfd = pset.pres_file_set[ssi];

                        byte[] file_data;

                        if (!pfd.IsVirtualFile)
                        {
                            if (pfd.IsCompressed && BLZ4Utils.IsBLZ4(pfd.file_data))
                            {
                                
                                file_data = BLZ4Utils.UnpackBLZ4Data(pfd.file_data);

                            }
                            else
                            {
                                file_data = pfd.file_data;
                            }
                        }
                        else
                        {
                            file_data = pfd.file_data;
                        }
                        

                        


                        string file_name = FileUtils.GetOrderName(ssi, pfd.GetFileMD5Name());


                        string out_file_path = set_dir.FullName + '\\' + file_name;

                        Directory.CreateDirectory(Path.GetDirectoryName(out_file_path));

                        string bin_name = out_file_path + ".bin";
                        string win32_name = pfd.GetWindowsFileName();

                        symbol_map.Add(bin_name,win32_name);

                        File.WriteAllBytes(bin_name, file_data);

                        File.WriteAllText(set_dir.FullName+'\\'+file_name+".xml",XmlUtils.Save(pfd));

                    }


                }




            }



            File.WriteAllText(dir_root.FullName + "\\GECV_PRES.xml", XmlUtils.Save(this));

        }



        private void BuildPresSet(byte[] pres_data,long reader_cursor,ref PresSet[] pres_data_set)
        {

            using(MemoryStream ms = new MemoryStream(pres_data))
            {
                using(BinaryReader br =new BinaryReader(ms))
                {

                    br.BaseStream.Seek(reader_cursor, SeekOrigin.Begin);

                    for (int i = 0; i < pres_data_set.Length; i++)
                    {


                        
                        int offset = br.ReadInt32();
                        int count = br.ReadInt32();
                        int mul = 1;

                        switch (i)
                        {
                            case 5:
                                mul = 4;
                                break;
                            default:
                                break;
                        }
                        Console.WriteLine($"Build Pres Set {i + 1}/8,offset:{offset.ToString("X8")},Count:{count},mul:{mul}");


                        PresSet ps;

                        if (offset != 0 || count>0)
                        {
                            ps = new PresSet(offset, count, mul);
                        }
                        else
                        {
                            ps = new PresSet(0, 1, 1);
                            ps.IsNoUsed = true;
                            Console.WriteLine("Pass Blank Set.");
                            pres_data_set[i] = ps;
                            continue;
                        }



                        pres_data_set[i] = ps;
                        
                        pres_data_set[i].GetFileDataFromPres(pres_data);

                    }
                    


                }
            }


        }


        public bool IsNoCountryPres()
        {


            return countries.Length <= 1 ? true : false;


        }










    }
}

