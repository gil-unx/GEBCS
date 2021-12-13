using System;
using System.IO;
using System.Collections.Generic;
using System.Xml.Serialization;
using GIL.FUNCTION;
using System.Text;

namespace GEBCS
{
    public class Uint
    {
        public List<uint> Value { get; set; }
    }
    public class Tr2Info
    {

        public int Offset { get; set; }
        public int PointerSize { get; set; }
        public int CompSize { get; set; }
        public int DecSize { get; set; }
        public int Index { get; set; }
        public string Name { get; set; }
       
        public int Dir { get; set; }
        public string Type { get; set; }
        public int Unk1 { get; set; }
        public string EncodingType { get; set; }

        public int PointerType { get; set; }
        public int Unk2 { get; set; }
        public int Unk3 { get; set; }
        public int Count { get; set; }

        public List<int> IndexString { get; set; }
   
        public List<string> ListString { get; set; }
        public List<string> NewListString { get; set; }

        public string ArrFile { get; set; }
    }
    public class Tr2
    {
        public string FileName { get; set; }
        public string Ext { get; set; }
        public int Unk { get; set; }
        public string InternalName { get; set; }
        public int PointerOffset { get; set; }
        public int Count { get; set; }
        public int PtStart { get; set; }
        public List<Tr2Info> Record { get; set; }
        public string UnkArr { get; set; }
       

    }
    class Tr2Decoder
    {
        private Tr2 tr2 = new Tr2();
        private BR reader;
        public Tr2Decoder(string fileName)
        {
            Console.WriteLine("Decoding: "+fileName);
            FileStream fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read);
            tr2.FileName = fileName;
            reader = new BR(fileStream);
            tr2.Ext = Encoding.UTF8.GetString(reader.ReadBytes(4));
            tr2.Unk = reader.ReadInt32();
            tr2.InternalName = reader.GetUtf8();
            reader.ReadPadding(0x30,8);
            tr2.PointerOffset = reader.ReadInt32();
            tr2.Count = reader.ReadInt32();
            tr2.Record = new List<Tr2Info>();
            for(int i =0;i< tr2.Count; i++)
            {
                Tr2Info tr2Info = new Tr2Info();
                tr2Info.Index = reader.ReadInt32();
                tr2Info.Offset = reader.ReadInt32();
                tr2Info.PointerSize = reader.ReadInt32();
                tr2Info.CompSize = reader.ReadInt32();
                tr2Info.DecSize = reader.ReadInt32();             
                MemoryStream memoryStream = new MemoryStream(reader.GetBytes((long)tr2Info.Offset,tr2Info.CompSize));
                BR memoryReader = new BR(memoryStream);
                tr2Info.Name = memoryReader.GetUtf8();
                memoryReader.ReadPadding(0x30, 0);
                tr2Info.Dir = memoryReader.ReadInt32();
                memoryReader.ReadBytes(4);
                tr2Info.PointerType = memoryReader.ReadInt32();
                tr2Info.Unk1 = memoryReader.ReadInt32();
                tr2Info.EncodingType = memoryReader.GetUtf8();
                memoryReader.BaseStream.Seek(0x74, SeekOrigin.Begin);
                tr2Info.Unk2 = memoryReader.ReadInt32();
                tr2Info.Unk3 = memoryReader.ReadInt32();
                tr2Info.Count = memoryReader.ReadInt32();              
                switch (tr2Info.EncodingType)
                {
                    case "UTF-8":
                        if(tr2Info.PointerType == 0x69626F79)
                        {
                            Utf8Yobi(ref tr2Info,ref memoryReader);
                        }
                        else
                        {
                            Utf8(ref tr2Info, ref memoryReader);
                        }
                        break;
                    case "ASCII":
                        
                        if (tr2Info.PointerType == 0x69626F79)
                        {
                            Utf8Yobi(ref tr2Info, ref memoryReader);
                        }
                        else
                        {
                            Utf8(ref tr2Info, ref memoryReader);
                        }
                        break;
              


                    default:
                        tr2Info.ArrFile = Convert.ToBase64String(memoryStream.ToArray());
                        break;
                }

                tr2.Record.Add(tr2Info);
                if (tr2Info.PointerType != 0x69626F79) memoryReader.ReadPadding(16, 0);
               


            }
            if(tr2.Record[0].Offset!= (int)reader.BaseStream.Position) tr2.UnkArr = Convert.ToBase64String(reader.ReadBytes(tr2.Record[0].Offset - (int)reader.BaseStream.Position));
            tr2.PtStart = (int)reader.BaseStream.Position;
            CreateXml();
            fileStream.Close();
            




        }
        private void Utf8(ref Tr2Info tr2Info, ref BR rdr)
        {
            tr2Info.ListString = new List<string>();
            for (int i = 0; i < tr2Info.Count; i++)
            {
                int off = rdr.ReadInt32();
                string str = rdr.GetUtf8((long)off);
               
                if (tr2.InternalName == "anagura_progress[enus]")
                {
                    tr2Info.ListString.Add(str.Replace("<br>", "\n"));
                }
                else
                {
                    tr2Info.ListString.Add(str);
                }

            }
            CreateTxt(tr2.FileName.Replace(".tr2", "\\") + tr2Info.Name + ".txt", tr2Info.ListString);
        }
        private void Utf8Yobi(ref Tr2Info tr2Info, ref BR rdr)
        {
           
            tr2Info.IndexString = new List<int>();
            tr2Info.ListString = new List<string>();
            for (int i = 0; i< tr2Info.Count; i++)
            {
                int index = rdr.ReadInt32();
                int off = rdr.ReadInt32();
                int len = rdr.ReadInt32();
                tr2Info.IndexString.Add(index);
                string str = rdr.GetUtf8((long)off);
                tr2Info.ListString.Add(str);
                

            }
            CreateTxt(tr2.FileName.Replace(".tr2","\\")+ tr2Info.Name + ".txt", tr2Info.ListString);

        }


      
        private void CreateXml()
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(Tr2));
            StreamWriter writer = new StreamWriter(Path.ChangeExtension(tr2.FileName, "xml"));
            xmlSerializer.Serialize(writer, tr2);
            writer.Close();


        }
        private void CreateTxt(string fileName, List<string> strings)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(fileName));
            StreamWriter writer = new StreamWriter(fileName);
            for (int i = 0; i< strings.Count;i++)
            {
                writer.WriteLine("[{0, 0:d4}]",i);
                writer.WriteLine(strings[i]);
                writer.WriteLine("--------------------------------------------");

            }
            writer.Close();


        }

    }
}
