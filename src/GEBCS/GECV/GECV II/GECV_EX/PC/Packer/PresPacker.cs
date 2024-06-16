using GECV_EX.Shared;
using GECV_EX.Utils;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace GECV_EX.PC.Packer
{
    public class PresPacker
    {

        private DirectoryInfo root_dir;
        private DirectoryInfo data_dir;
        private DirectoryInfo mod_dir;
        private DirectoryInfo merge_dir;


        private PresPC PresInf;

        private BinaryBooker booker;

        private PresPackDataManager datamanager;

        private Dictionary<string, string> FileBookMarkMap = new Dictionary<string, string>();



        public PresPacker(string dir)
        {


            if (Path.Exists(dir))
            {
                root_dir = new DirectoryInfo(dir);
                data_dir = new DirectoryInfo(root_dir.FullName + "\\Data");
                mod_dir = new DirectoryInfo(root_dir.FullName + "\\Mod");
                merge_dir = new DirectoryInfo(root_dir.FullName + "\\Merge");

                Console.WriteLine($"Pres Pack Working For:{root_dir.FullName}");

                //merge_dir.Create();

                if (merge_dir.Exists)
                {
                    merge_dir.Delete(true);
                }

                if (!data_dir.Exists)
                {

                    throw new FileNotFoundException($"Directory Not Found;{data_dir.FullName}");
                }
                else
                {
                    booker = new BinaryBooker();
                    CreateMerge();
                    PresInf = XmlUtils.Load<PresPC>(merge_dir.FullName + "\\GECV_PRES.xml");
                    Console.WriteLine($"Get Pres Magic :{PresInf.magic_0},{PresInf.magic_1},{PresInf.magic_2},{PresInf.magic_3}");

                    datamanager = new PresPackDataManager(data_dir.FullName);




                    int offset = SetPresInformation(datamanager.nodes.Count);
                    offset = SetCountryAndSetInformation(offset);


                    if (offset % 16 != 0)
                    {
                        throw new InvalidDataException($"The Pres Header Length:{offset.ToString("X16")} % 16 != 0. Result:({offset % 16})");
                    }
                    //else
                    //{
                    //    booker.WriteData("pres_offset_data", offset);
                    //}

                    booker.WriteData("pres_offset_data", offset);


                    offset = SetEndFile(offset);

                }

            }
            else
            {
                throw new FileNotFoundException($"Directory Not Found;{dir}");
            }





        }

        public List<string> GetRegisterText()
        {
            return datamanager.registerInformation;
        }


        private void CreateMerge()
        {
            FileUtils.CopyDirectory(data_dir.FullName, merge_dir.FullName, true);
        }


        public byte[] GetPresFileBytes()
        {
            return booker.GetAllData();
        }

        private int SetPresInformation(int country_count)
        {
            Console.WriteLine($"Write Country Type:{country_count}.");

            int offset = 0;

            for (int i = 0; i < 4; i++)
            {
                booker.SetBookMark("pres_magic_" + i, offset);
                offset += 4;
            }


            booker.SetBookMark("pres_offset_data", offset);
            booker.WriteData("pres_offset_data", 0);
            offset += 4;
            booker.SetBookMark("pres_zerozero", offset);
            booker.WriteData("pres_zerozero", 0L);
            offset += 8;
            booker.SetBookMark("pres_country_count", offset);
            booker.WriteData("pres_country_count", country_count);

            booker.WriteData("pres_magic_0", PresInf.magic_0);
            booker.WriteData("pres_magic_1", PresInf.magic_1);
            booker.WriteData("pres_magic_2", PresInf.magic_2);
            booker.WriteData("pres_magic_3", PresInf.magic_3);

            offset += 4;






            return offset;
        }

        private int SetCountryAndSetInformation(int offset)
        {

            bool IsSingle;

            if (datamanager.nodes.Count > 1)
            {
                for (int i = 0; i < datamanager.nodes.Count; i++)
                {
                    booker.SetBookMark("country_offset_" + i, offset);
                    booker.WriteData("country_offset_" + i, i);
                    offset += 4;
                    booker.SetBookMark("country_length_" + i, offset);
                    booker.WriteData("country_length_" + i, i);
                    offset += 4;
                }
                //booker.SetBookMark("country_zero16", offset);
                //booker.WriteData("country_zero16",new byte[16]);

                //offset += 16;
                IsSingle = false;
            }
            else
            {
                IsSingle = true;
            }

            Console.WriteLine($"Country End,Now Is DataSet (Offset:{offset.ToString("X")})");

            int country_offset = offset;

            for (int i = 0; i < datamanager.nodes.Count; i++)
            {
                var country_node = datamanager.nodes[i];
                int origin_offset;
                if (!IsSingle)
                {
                    booker.WriteData($"country_offset_{i}", offset);
                    
                }
                origin_offset = offset;

                int set_offset = 0;

                for (int si = 0; si < country_node.GetNodeCount(); si++)
                {



                    var set_node = country_node.GetNode(si);

                    booker.SetBookMark($"dataset_offset_{i}_{si}", offset);
                    booker.WriteData($"dataset_offset_{i}_{si}", 0);




                    offset += 4;
                    set_offset += 4;
                    booker.SetBookMark($"dataset_count_{i}_{si}", offset);
                    booker.WriteData($"dataset_count_{i}_{si}", set_node.GetNodeCount());

                    offset += 4;


                    set_offset += 4;

                }




                //booker.SetBookMark("dataset_zero16_"+i, offset);
                //booker.WriteData("dataset_zero16_"+i, new byte[16]);
                //offset += 16;

                

                //var country_node = datamanager.nodes[i];
                offset = SetDataSetInformation(country_node, offset);
                //int origin_offset = booker.GetBookMark($"country_offset_{i}");
                
                if (offset % 16 != 0)
                {
                    throw new InvalidDataException($"The Pres Country Length:{offset.ToString("X16")} % 16 != 0. Result:({offset % 16})");
                }

                if (!IsSingle)
                {
                    Console.WriteLine($"Country Length:Start:{origin_offset.ToString("X")},End:{offset.ToString("X")},Length:{(offset - origin_offset).ToString("X")}");
                    booker.WriteData("country_length_" + country_node.id, offset - origin_offset);
                }




            }









            return offset;

        }

        private int SetDataSetInformation(PresPackCountryNode node, int offset)
        {



            List<PresPackDataSetNode> list = new List<PresPackDataSetNode>();

            list.Add(node.GetNode(2));
            list.Add(node.GetNode(3));
            list.Add(node.GetNode(4));
            list.Add(node.GetNode(5)); // NO FILE SET
            list.Add(node.GetNode(6)); // NO FILE SET 
            list.Add(node.GetNode(7));
            list.Add(node.GetNode(0));
            list.Add(node.GetNode(1));

            foreach (var n in list)
            {

                if (n.IsBlankSet())
                {
                    continue;
                }

                booker.WriteData($"dataset_offset_{node.id}_{n.id}", offset);

                for (int i = 0; i < n.GetNodeCount(); i++)
                {

                    var item = n.GetNode(i);

                    booker.SetBookMark($"data_{node.id}_{n.id}_{i}_offset", offset); /// After
                    offset += 4;
                    booker.SetBookMark($"data_{node.id}_{n.id}_{i}_csize_file", offset);
                    offset += 4;
                    booker.SetBookMark($"data_{node.id}_{n.id}_{i}_name_off_file", offset); //After
                    offset += 4;
                    booker.SetBookMark($"data_{node.id}_{n.id}_{i}_name_elements_file", offset);
                    offset += 4;
                    booker.SetBookMark($"data_{node.id}_{n.id}_{i}_unk1", offset);
                    offset += 4;
                    booker.SetBookMark($"data_{node.id}_{n.id}_{i}_unk2", offset);
                    offset += 4;
                    booker.SetBookMark($"data_{node.id}_{n.id}_{i}_unk3", offset);
                    offset += 4;
                    booker.SetBookMark($"data_{node.id}_{n.id}_{i}_usize_file", offset);
                    offset += 4;

                    booker.WriteData($"data_{node.id}_{n.id}_{i}_csize_file", item.presDataInf.csize_file);
                    booker.WriteData($"data_{node.id}_{n.id}_{i}_name_elements_file", item.presDataInf.name_list.Length);
                    booker.WriteData($"data_{node.id}_{n.id}_{i}_unk1", item.presDataInf.file_unk1);
                    booker.WriteData($"data_{node.id}_{n.id}_{i}_unk2", item.presDataInf.file_unk2);
                    booker.WriteData($"data_{node.id}_{n.id}_{i}_unk3", item.presDataInf.file_unk3);
                    booker.WriteData($"data_{node.id}_{n.id}_{i}_usize_file", item.presDataInf.usize_file);


                }




            }

            //booker.SetBookMark($"data_{node.id}_zero16", offset);
            //booker.WriteData($"data_{node.id}_zero16", new byte[16]);

            //offset += 16;


            foreach (var n in list)
            {

                if (n.IsBlankSet())
                {
                    continue;
                }



                for (int i = 0; i < n.GetNodeCount(); i++)
                {

                    var item = n.GetNode(i);

                    bool PassAfterAction = false;


                    if (!item.presDataInf.IsVirtualFile && (n.id == 5 || n.id == 6)) // SET_6_ SET_7
                    {
                        //booker.SetBookMark($"data_{node.id}_{n.id}_{i}_header_file_zero16", offset);

                        //booker.WriteData($"data_{node.id}_{n.id}_{i}_header_file_zero16",new byte[0]);

                        //offset += 16;


                        booker.SetBookMark($"data_{node.id}_{n.id}_{i}_header_file", offset);

                        string header_file_offset_hex = 'F' + offset.ToString("X7");

                        int virtual_address = Convert.ToInt32(header_file_offset_hex, 16);


                        byte[] file_data = item.presDataInf.file_data;

                        Array.Resize<byte>(ref file_data, file_data.Length + 1);


                        int file_data_start = offset + file_data.Length;

                        int file_data_end = file_data_start % 16;


                        if (file_data_end == 0)
                        {
                            file_data_end = file_data_start + 16;
                        }
                        else
                        {
                            file_data_end = file_data_start + (16 - file_data_end);
                        }


                        Array.Resize<byte>(ref file_data, file_data_end - offset);


                        booker.WriteData($"data_{node.id}_{n.id}_{i}_header_file", file_data);

                        if (item.presDataInf.csize_file > 0)
                        {
                            booker.WriteData($"data_{node.id}_{n.id}_{i}_offset", virtual_address);
                        }
                        else
                        {
                            booker.WriteData($"data_{node.id}_{n.id}_{i}_offset", 0);
                        }



                        booker.WriteData($"data_{node.id}_{n.id}_{i}_usize_file", 0);

                        offset += file_data.Length;

                        PassAfterAction = true;

                    }
                    else
                    {
                        Console.WriteLine($"No Virtual 6&7 Action.");
                    }


                    booker.WriteData($"data_{node.id}_{n.id}_{i}_name_off_file", offset);

                    int conf_offset = 0;

                    //int name_4_offset = -1;
                    string virutal_name = item.presDataInf.VirtualFileName;

                    int conf_cursor_length = 0;

                    for (int si = 0; si < item.presDataInf.name_list.Length; si++)
                    {

                        booker.SetBookMark($"data_{node.id}_{n.id}_{i}_name_elements_offset_{si}", offset);

                        offset += 4;
                        conf_cursor_length += 4;

                    }


                    for (int si = 0; si < item.presDataInf.name_list.Length; si++)
                    {

                        string str = item.presDataInf.name_list[si];

                        byte[] data = Encoding.UTF8.GetBytes(str);


                        Array.Resize(ref data, data.Length + 1);

                        if (item.presDataInf.name_list.Length - 1 == si)
                        {

                            int resize_length_end = conf_cursor_length + data.Length;

                            int resize_length = resize_length_end % 16;

                            if (resize_length == 0)
                            {
                                resize_length += 16;
                            }
                            else
                            {
                                resize_length_end = resize_length_end + (16 - resize_length);
                            }

                            Console.WriteLine($"Align 16:Start:{conf_cursor_length},End:{resize_length_end},Length:{resize_length_end - conf_cursor_length}");

                            Array.Resize(ref data, resize_length_end - conf_cursor_length);

                            //if(data.Length % 16 != 0)
                            //{
                            //    throw new ArgumentOutOfRangeException($"{data.Length} Length Error, {data.Length} % 16 != 0");
                            //}
                            //else
                            //{
                            //    Console.WriteLine($"Result Array Size: {data.Length}");
                            //}

                        }
                        else
                        {
                            conf_cursor_length += data.Length;
                        }

                        var result_offset = offset + conf_offset;

                        booker.SetBookMark($"data_{node.id}_{n.id}_{i}_name_elements_{si}", result_offset);
                        booker.WriteData($"data_{node.id}_{n.id}_{i}_name_elements_{si}", data);
                        booker.WriteData($"data_{node.id}_{n.id}_{i}_name_elements_offset_{si}", result_offset);
                        //if (si == 3)
                        //{
                        //    name_4_offset = result_offset;
                        //}
                        Console.WriteLine($"Conf List({si + 1}/{item.presDataInf.name_list.Length}):{str}");
                        conf_offset += data.Length;





                    }

                    offset += conf_offset;

                    Console.WriteLine($"Name 4 Status:{virutal_name}");


                    if (item.presDataInf.IsVirtualFile && !string.IsNullOrEmpty(virutal_name))
                    {

                        byte[] data = Encoding.UTF8.GetBytes(virutal_name);

                        Array.Resize<byte>(ref data, data.Length + 1);

                        int resize_length_end = offset + data.Length;

                        int resize_length = resize_length_end % 16;

                        if (resize_length == 0)
                        {
                            resize_length += 16;
                        }
                        else
                        {
                            resize_length_end = resize_length_end + (16 - resize_length);
                        }

                        Array.Resize(ref data, resize_length_end - offset);

                        string name_4_offset_hex = 'B' + offset.ToString("X7");

                        int virtual_address = Convert.ToInt32(name_4_offset_hex, 16);

                        booker.WriteData($"data_{node.id}_{n.id}_{i}_offset", virtual_address);

                        booker.SetBookMark($"data_{node.id}_{n.id}_{i}_virtual_name", offset);
                        booker.WriteData($"data_{node.id}_{n.id}_{i}_virtual_name", data);

                        //booker.SetBookMark($"data_{node.id}_{n.id}_{i}_usize_file", offset);

                        booker.WriteData($"data_{node.id}_{n.id}_{i}_usize_file", 0);

                        offset += data.Length;


                        continue;

                    }
                    else
                    {
                        Console.WriteLine($"No Virtual Action.");
                    }




                    if (!item.presDataInf.IsVirtualFile && n.id != 5 && n.id != 6)
                    {

                        var title = $"data_{node.id}_{n.id}_{i}_offset";

                        var md5 = item.md5;

                        FileBookMarkMap.Add(title, md5);



                        continue;
                    }
                    else
                    {
                        Console.WriteLine($"No Real File Action.");
                    }


                    if (offset % 16 != 0)
                    {
                        throw new InvalidDataException($"The Pres Data Length:{offset.ToString("X16")} % 16 != 0. Result:({offset % 16})");
                    }

                    if (!PassAfterAction)
                    {
                        throw new ArgumentException($"data_{node.id}_{n.id}_{i}_offset Not Have Action.");
                    }





                }


                //booker.SetBookMark($"data_{node.id}_{n.id}_name_conf_zero16", offset);

                //booker.WriteData($"data_{node.id}_{n.id}_name_conf_zero16",new byte[16]);

                //offset += 16;


            }





            return offset;

        }


        public int SetEndFile(int offset)
        {

            Dictionary<string, int> FileWriteBookMap = new Dictionary<string, int>();




            foreach (var kv in FileBookMarkMap)
            {

                byte[] data = PresPackDataManager.GetCacheByMD5(kv.Value);


                Console.WriteLine($"Write End File {kv.Key} Data:{kv.Value}, Length:{data.Length}");

                string file_id = "end_file_data_" + kv.Value;

                if (FileWriteBookMap.ContainsKey(file_id))
                {
                    int cache_offset = FileWriteBookMap[file_id];
                    string hex_offet = 'F' + cache_offset.ToString("X7");
                    int virtual_offset = Convert.ToInt32(hex_offet, 16);
                    booker.WriteData(kv.Key, virtual_offset);
                    Console.WriteLine($"Get Old File:{hex_offet}");
                }
                else
                {

                    //if (FileWriteBookMap.ContainsKey(file_id))
                    //{
                    //    throw new ArgumentException($"FileWriteMap Has This File:{file_id}.");
                    //}


                    booker.SetBookMark(file_id, offset);

                    booker.WriteData(file_id, data);

                    string hex_offet = 'F' + offset.ToString("X7");



                    int virtual_offset = Convert.ToInt32(hex_offet, 16);

                    booker.WriteData(kv.Key, virtual_offset);
                    Console.WriteLine($"Write New File:{hex_offet}");

                    FileWriteBookMap.Add(file_id, offset);

                    offset += data.Length;

                    int start_align_offset = offset;
                    int end_align_offset = offset % 16;

                    if (end_align_offset == 0)
                    {
                        end_align_offset = start_align_offset + 16;
                    }
                    else
                    {
                        end_align_offset = start_align_offset + (16 - end_align_offset);
                    }



                    booker.SetBookMark(file_id + "_align", offset);

                    booker.WriteData(file_id + "_align", new byte[end_align_offset - start_align_offset]);

                    Console.WriteLine($"Align:Start:{start_align_offset},End:{end_align_offset},Length:{end_align_offset - start_align_offset}");


                    offset += (end_align_offset - start_align_offset);
                }




            }

            return offset;

        }


        public List<string> GetBookInformation()
        {
            return booker.GetBookInformation();
        }



    }
}
