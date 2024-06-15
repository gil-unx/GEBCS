using System;
using System.IO;
using System.Collections.Generic;
using System.Xml.Serialization;
using GIL.FUNCTION;
using System.Text;
using System.Text.Json;
using GEBCS.tr2;

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


                    case "INT8":

                        
                        Int8(ref tr2Info, ref memoryReader);
                        
                        
                        break;
                    case "INT16":


                        Int16(ref tr2Info, ref memoryReader);


                        break;

                    case "INT32":


                        Int32(ref tr2Info, ref memoryReader);


                        break;
                    case "UINT8":


                        UInt8(ref tr2Info, ref memoryReader);


                        break;
                    case "UINT16":


                        UInt16(ref tr2Info, ref memoryReader);


                        break;

                    case "UINT32":


                        UInt32(ref tr2Info, ref memoryReader);


                        break;
                    case "FLOAT32":


                        Float32(ref tr2Info, ref memoryReader);


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


        private void Int8(ref Tr2Info tr2Info, ref BR rdr)
        {

            INT8 int8 = new INT8();
            int8.EncodingType = "INT8";
            int8.Data = new List<List<sbyte>>();
            for (int i = 0; i < tr2Info.Count; i++)
            {
                
                int off = rdr.ReadInt32();
                int end = rdr.ReadInt32();
                int len = end - off;
                rdr.BaseStream.Seek(-4, SeekOrigin.Current);
                long tmp = rdr.BaseStream.Position;
                rdr.BaseStream.Seek(off, SeekOrigin.Begin);
                List<sbyte> s = new List<sbyte>();
                for (int j = 0; j < len; j++)
                {
                    s.Add(rdr.ReadSByte());
                }
                int8.Data.Add(s);
                rdr.BaseStream.Seek(tmp, SeekOrigin.Begin);


            }
            CreateJson(tr2.FileName.Replace(".tr2", "\\") + tr2Info.Name + ".json", int8);

        }
        private void Int16(ref Tr2Info tr2Info, ref BR rdr)
        {

            INT16 int16 = new INT16();
            int16.EncodingType = "INT16";
            int16.Data = new List<List<short>>();
            for (int i = 0; i < tr2Info.Count; i++)
            {

                int off = rdr.ReadInt32();
                int end = rdr.ReadInt32();
                int len = (end - off)/2;
                rdr.BaseStream.Seek(-4, SeekOrigin.Current);
                long tmp = rdr.BaseStream.Position;
                rdr.BaseStream.Seek(off, SeekOrigin.Begin);
                List<short> s = new List<short>();
                for (int j = 0; j < len; j++)
                {
                    s.Add(rdr.ReadInt16());
                }
                int16.Data.Add(s);
                rdr.BaseStream.Seek(tmp, SeekOrigin.Begin);


            }
            CreateJson(tr2.FileName.Replace(".tr2", "\\") + tr2Info.Name + ".json", int16);

        }
        private void Int32(ref Tr2Info tr2Info, ref BR rdr)
        {

            INT32 int32 = new INT32();
            int32.EncodingType = "INT32";
            int32.Data = new List<List<int>>();
            for (int i = 0; i < tr2Info.Count; i++)
            {

                int off = rdr.ReadInt32();
                int end = rdr.ReadInt32();
                int len = (end - off) / 4;
                rdr.BaseStream.Seek(-4, SeekOrigin.Current);
                long tmp = rdr.BaseStream.Position;
                rdr.BaseStream.Seek(off, SeekOrigin.Begin);
                List<int> s = new List<int>();
                for (int j = 0; j < len; j++)
                {
                    s.Add(rdr.ReadInt32());
                }
                int32.Data.Add(s);
                rdr.BaseStream.Seek(tmp, SeekOrigin.Begin);


            }
            CreateJson(tr2.FileName.Replace(".tr2", "\\") + tr2Info.Name + ".json", int32);

        }
        private void UInt8(ref Tr2Info tr2Info, ref BR rdr)
        {

            UINT8 uint8 = new UINT8();
            uint8.EncodingType = "UINT8";
            uint8.Data = new List<List<byte>>();
            for (int i = 0; i < tr2Info.Count; i++)
            {

                int off = rdr.ReadInt32();
                int end = rdr.ReadInt32();
                int len = end - off;
                rdr.BaseStream.Seek(-4, SeekOrigin.Current);
                long tmp = rdr.BaseStream.Position;
                rdr.BaseStream.Seek(off, SeekOrigin.Begin);
                List<byte> s = new List<byte>();
                for (int j = 0; j < len; j++)
                {
                    s.Add(rdr.ReadByte());
                }
                uint8.Data.Add(s);
                rdr.BaseStream.Seek(tmp, SeekOrigin.Begin);


            }
            CreateJson(tr2.FileName.Replace(".tr2", "\\") + tr2Info.Name + ".json", uint8);

        }
        private void UInt16(ref Tr2Info tr2Info, ref BR rdr)
        {

            UINT16 uint16 = new UINT16();
            uint16.EncodingType = "UINT16";
            uint16.Data = new List<List<ushort>>();
            for (int i = 0; i < tr2Info.Count; i++)
            {

                int off = rdr.ReadInt32();
                int end = rdr.ReadInt32();
                int len = (end - off) / 2;
                rdr.BaseStream.Seek(-4, SeekOrigin.Current);
                long tmp = rdr.BaseStream.Position;
                rdr.BaseStream.Seek(off, SeekOrigin.Begin);
                List<ushort> s = new List<ushort>();
                for (int j = 0; j < len; j++)
                {
                    s.Add(rdr.ReadUInt16());
                }
                uint16.Data.Add(s);
                rdr.BaseStream.Seek(tmp, SeekOrigin.Begin);


            }
            CreateJson(tr2.FileName.Replace(".tr2", "\\") + tr2Info.Name + ".json", uint16);

        }
        private void UInt32(ref Tr2Info tr2Info, ref BR rdr)
        {

            UINT32 uint32 = new UINT32();
            uint32.EncodingType = "UINT32";
            uint32.Data = new List<List<uint>>();
            for (int i = 0; i < tr2Info.Count; i++)
            {

                int off = rdr.ReadInt32();
                int end = rdr.ReadInt32();
                int len = (end - off) / 4;
                rdr.BaseStream.Seek(-4, SeekOrigin.Current);
                long tmp = rdr.BaseStream.Position;
                rdr.BaseStream.Seek(off, SeekOrigin.Begin);
                List<uint> s = new List<uint>();
                for (int j = 0; j < len; j++)
                {
                    s.Add(rdr.ReadUInt32());
                }
                uint32.Data.Add(s);
                rdr.BaseStream.Seek(tmp, SeekOrigin.Begin);


            }
            CreateJson(tr2.FileName.Replace(".tr2", "\\") + tr2Info.Name + ".json", uint32);

        }
        private void Float32(ref Tr2Info tr2Info, ref BR rdr)
        {

            FLOAT32 float32 = new FLOAT32();
            float32.EncodingType = "FLOAT32";
            float32.Data = new List<List<Single>>();
            for (int i = 0; i < tr2Info.Count; i++)
            {

                int off = rdr.ReadInt32();
                int end = rdr.ReadInt32();
                int len = (end - off) / 4;
                rdr.BaseStream.Seek(-4, SeekOrigin.Current);
                long tmp = rdr.BaseStream.Position;
                rdr.BaseStream.Seek(off, SeekOrigin.Begin);
                List<Single> s = new List<Single>();
                for (int j = 0; j < len; j++)
                {
                    s.Add(rdr.ReadSingle());
                }
                float32.Data.Add(s);
                rdr.BaseStream.Seek(tmp, SeekOrigin.Begin);


            }
            CreateJson(tr2.FileName.Replace(".tr2", "\\") + tr2Info.Name + ".json", float32);

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
        private void CreateJson(string fileName, INT8 encodingType)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(fileName));
            string jsonString = JsonSerializer.Serialize(encodingType, new JsonSerializerOptions() { WriteIndented = true, Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping });
            File.WriteAllText(fileName, jsonString);



        }
        private void CreateJson(string fileName, UINT8 encodingType)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(fileName));
            string jsonString = JsonSerializer.Serialize(encodingType, new JsonSerializerOptions() { WriteIndented = true, Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping });
            File.WriteAllText(fileName, jsonString);



        }
        private void CreateJson(string fileName, INT16 encodingType)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(fileName));
            string jsonString = JsonSerializer.Serialize(encodingType, new JsonSerializerOptions() { WriteIndented = true, Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping });
            File.WriteAllText(fileName, jsonString);



        }
        private void CreateJson(string fileName, UINT16 encodingType)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(fileName));
            string jsonString = JsonSerializer.Serialize(encodingType, new JsonSerializerOptions() { WriteIndented = true, Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping });
            File.WriteAllText(fileName, jsonString);



        }
        private void CreateJson(string fileName, INT32 encodingType)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(fileName));
            string jsonString = JsonSerializer.Serialize(encodingType, new JsonSerializerOptions() { WriteIndented = true, Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping });
            File.WriteAllText(fileName, jsonString);



        }
        private void CreateJson(string fileName, UINT32 encodingType)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(fileName));
            string jsonString = JsonSerializer.Serialize(encodingType, new JsonSerializerOptions() { WriteIndented = true, Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping });
            File.WriteAllText(fileName, jsonString);



        }
        private void CreateJson(string fileName, FLOAT32 encodingType)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(fileName));
            string jsonString = JsonSerializer.Serialize(encodingType, new JsonSerializerOptions() { WriteIndented = true, Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping });
            File.WriteAllText(fileName, jsonString);



        }
    }
}
