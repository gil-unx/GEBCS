using GEBCS.tr2;
using GIL.FUNCTION;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
namespace GEBCS
{
    class Tr2Encoder
    {
        private Tr2 tr2 = new Tr2();
        private BW writer;
        public Tr2Encoder(string fileName)
        {
            Console.WriteLine("Encoding: "+fileName);
            tr2 = JsonSerializer.Deserialize<Tr2>(File.ReadAllText(Path.ChangeExtension(fileName, "json")));

           
            FileStream fileStream = new FileStream(fileName, FileMode.Create, FileAccess.Write);
            
            writer = new BW(fileStream);
            writer.Write(Encoding.UTF8.GetBytes(tr2.Ext));
            writer.Write(tr2.Unk);
            writer.Write(Encoding.UTF8.GetBytes(tr2.InternalName));
            writer.WritePadding(0x30, 8);
            writer.Write(tr2.PointerOffset);
            writer.Write(tr2.Count);
            MemoryStream ms = new MemoryStream();
            BW mw = new BW(ms);
            int newOffset = tr2.PtStart;
          
          
            foreach (Tr2Info tr2Info in tr2.Record)
            {
                byte[] buffer;
                switch (tr2Info.EncodingType)
                {
                    case "UTF-8":
                        if (tr2Info.PointerType == 0x69626F79)
                        {
                            buffer = Utf8Yobi(tr2Info);


                        }
                        else
                        {
                            buffer = Utf8(tr2Info);
                        }
                        break;
                    case "ASCII":

                        if (tr2Info.PointerType == 0x69626F79)
                        {
                            buffer = AsciiYobi(tr2Info);
                        }
                        else
                        {
                            buffer = Ascii(tr2Info);
                        }
                        break;


                    case "INT8":
                       
                        if (tr2Info.PointerType == 0x69626F79)
                        {
                            buffer = Int8Yobi(tr2Info);
                        }
                        else
                        {
                            buffer = Int8(tr2Info);
                        }
                        break;
                    case "INT16":
                        if (tr2Info.PointerType == 0x69626F79)
                        {
                            buffer = Int16Yobi(tr2Info);
                        }
                        else
                        {
                            buffer = Int16(tr2Info);
                        }
                        break;
                    case "INT32":
                       if (tr2Info.PointerType == 0x69626F79)
                        {
                            buffer = Int32Yobi(tr2Info);
                        }
                        else
                        {
                            buffer = Int32(tr2Info);
                        }
                        break;
                    case "UINT8":
                        if (tr2Info.PointerType == 0x69626F79)
                        {
                            buffer = UInt8Yobi(tr2Info);
                        }
                        else
                        {
                            buffer = UInt8(tr2Info);
                        }
                        break;
                    case "UINT16":
                        if (tr2Info.PointerType == 0x69626F79)
                        {
                            buffer = UInt16Yobi(tr2Info);
                        }
                        else
                        {
                            buffer = UInt16(tr2Info);
                        }
                        break;
                    case "UINT32":
                        buffer = UInt32(tr2Info);
                        if (tr2Info.PointerType == 0x69626F79)
                        {
                            buffer = UInt32Yobi(tr2Info);
                        }
                        else
                        {
                            buffer = UInt32(tr2Info);
                        }
                        break;
                    case "FLOAT32":
                        buffer = Float32(tr2Info);
                        if (tr2Info.PointerType == 0x69626F79)
                        {
                            buffer = Float32Yobi(tr2Info);
                        }
                        else
                        {
                            buffer = Float32(tr2Info);
                        }
                        break;

                    default:
                       
                        buffer = Convert.FromBase64String(tr2Info.ArrFile);
                        break;
                       

                }
                int newSize = buffer.Length;
                int padding = 16 - (newSize % 16);
                if (padding == 16) padding = 0;
                newSize += padding;
                writer.Write(tr2Info.Index);
                writer.Write(newOffset);
                writer.Write(tr2Info.PointerSize);
                writer.Write(newSize);
                writer.Write(newSize);
                mw.Write(buffer);
                mw.WritePadding(16, 0);
                newOffset = (int)mw.BaseStream.Position+tr2.PtStart;
               




            }
            if(tr2.UnkArr!= null) writer.Write(Convert.FromBase64String(tr2.UnkArr));
            writer.Write(ms.ToArray());
            fileStream.Flush();
            fileStream.Close();
             




        }
        private byte[] Ascii(Tr2Info tr2Info)
        {
            List<string> newStrings =  LoadTxt(tr2.FileName.Replace(".tr2", "\\") + tr2Info.Name + ".txt",tr2Info.Count);
            MemoryStream ms = new MemoryStream();
            BW mw = new BW(ms);
            mw.Write(Encoding.UTF8.GetBytes(tr2Info.Name));
            mw.BaseStream.Seek(0x30, SeekOrigin.Begin);
            mw.Write(tr2Info.Dir);
            mw.Write(0);
            mw.Write(tr2Info.PointerType);
            mw.Write(tr2Info.Unk1);
            mw.Write(Encoding.UTF8.GetBytes(tr2Info.EncodingType));
            mw.BaseStream.Seek(0x74, SeekOrigin.Begin);
            mw.Write(tr2Info.Unk2);
            mw.Write(tr2Info.Unk3);
            mw.Write(tr2Info.Count);
            int newOff = (tr2Info.Count * 4) + 0x88;
            
            for (int i = 0; i < tr2Info.Count; i++)
            {
                mw.Write(newOff);

                if (tr2.InternalName == "anagura_progress[enus]") newStrings[i]=newStrings[i].Replace("\n", "<br>");
                newOff += (int)Convert.FromBase64String(newStrings[i]).Length;
                
            }
            mw.Write(newOff);
            mw.BaseStream.Seek((long)(tr2Info.Count * 4) + 0x88,SeekOrigin.Begin);
            for (int i = 0; i < tr2Info.Count; i++)
            {
                mw.Write(Convert.FromBase64String(newStrings[i]));
               
            }
            mw.WritePadding(16, 0);
            return ms.ToArray();
        }
        private byte[] AsciiYobi(Tr2Info tr2Info)
        {
            List<string> newStrings = LoadTxt(tr2.FileName.Replace(".tr2", "\\") + tr2Info.Name + ".txt", tr2Info.Count);
            MemoryStream ms = new MemoryStream();
            BW mw = new BW(ms);
            mw.Write(Encoding.UTF8.GetBytes(tr2Info.Name));
            mw.BaseStream.Seek(0x30, SeekOrigin.Begin);
            mw.Write(tr2Info.Dir);
            mw.Write(0);
            mw.Write(tr2Info.PointerType);
            mw.Write(tr2Info.Unk1);
            mw.Write(Encoding.UTF8.GetBytes(tr2Info.EncodingType));
            mw.BaseStream.Seek(0x74, SeekOrigin.Begin);
            mw.Write(tr2Info.Unk2);
            mw.Write(tr2Info.Unk3);
            mw.Write(tr2Info.Count);
            int newOff = (tr2Info.Count * 12) + 0x80;
            for (int i = 0; i < tr2Info.Count; i++)
            {
                mw.Write(tr2Info.IndexString[i]);
                mw.Write(newOff);
                int newLen = (int)Convert.FromBase64String(newStrings[i]).Length;
                mw.Write(newLen);
                newOff += newLen;
                
            }
            mw.BaseStream.Seek((long)(tr2Info.Count * 12) + 0x80, SeekOrigin.Begin);
            for (int i = 0; i < tr2Info.Count; i++)
            {
                mw.Write(Convert.FromBase64String(newStrings[i]));
                
            }
            mw.WritePadding(16, 0);
            return ms.ToArray();
        }
        private byte[] Utf8(Tr2Info tr2Info)
        {
            List<string> newStrings = LoadTxt(tr2.FileName.Replace(".tr2", "\\") + tr2Info.Name + ".txt", tr2Info.Count);
            MemoryStream ms = new MemoryStream();
            BW mw = new BW(ms);
            mw.Write(Encoding.UTF8.GetBytes(tr2Info.Name));
            mw.BaseStream.Seek(0x30, SeekOrigin.Begin);
            mw.Write(tr2Info.Dir);
            mw.Write(0);
            mw.Write(tr2Info.PointerType);
            mw.Write(tr2Info.Unk1);
            mw.Write(Encoding.UTF8.GetBytes(tr2Info.EncodingType));
            mw.BaseStream.Seek(0x74, SeekOrigin.Begin);
            mw.Write(tr2Info.Unk2);
            mw.Write(tr2Info.Unk3);
            mw.Write(tr2Info.Count);
            int newOff = (tr2Info.Count * 4) + 0x88;

            for (int i = 0; i < tr2Info.Count; i++)
            {
                mw.Write(newOff);

                if (tr2.InternalName == "anagura_progress[enus]") newStrings[i] = newStrings[i].Replace("\n", "<br>");
                newOff += (int)Encoding.UTF8.GetBytes(newStrings[i]).Length;
                newOff++;
            }
            mw.Write(newOff);
            mw.BaseStream.Seek((long)(tr2Info.Count * 4) + 0x88, SeekOrigin.Begin);
            for (int i = 0; i < tr2Info.Count; i++)
            {
                mw.Write(Encoding.UTF8.GetBytes(newStrings[i]));
                mw.Write((byte)0);
            }
            mw.WritePadding(16, 0);
            return ms.ToArray();
        }
        private byte[] Utf8Yobi(Tr2Info tr2Info)
        {
            List<string> newStrings = LoadTxt(tr2.FileName.Replace(".tr2", "\\") + tr2Info.Name + ".txt", tr2Info.Count);
            MemoryStream ms = new MemoryStream();
            BW mw = new BW(ms);
            mw.Write(Encoding.UTF8.GetBytes(tr2Info.Name));
            mw.BaseStream.Seek(0x30, SeekOrigin.Begin);
            mw.Write(tr2Info.Dir);
            mw.Write(0);
            mw.Write(tr2Info.PointerType);
            mw.Write(tr2Info.Unk1);
            mw.Write(Encoding.UTF8.GetBytes(tr2Info.EncodingType));
            mw.BaseStream.Seek(0x74, SeekOrigin.Begin);
            mw.Write(tr2Info.Unk2);
            mw.Write(tr2Info.Unk3);
            mw.Write(tr2Info.Count);
            int newOff = (tr2Info.Count * 12) + 0x80;
            for (int i = 0; i < tr2Info.Count; i++)
            {
                mw.Write(tr2Info.IndexString[i]);
                mw.Write(newOff);
                int newLen = (int)Encoding.UTF8.GetBytes(newStrings[i]).Length;
                mw.Write(newLen);
                newOff += newLen;
                newOff++;
            }
            mw.BaseStream.Seek((long)(tr2Info.Count * 12) + 0x80, SeekOrigin.Begin);
            for (int i = 0; i < tr2Info.Count; i++)
            {
                mw.Write(Encoding.UTF8.GetBytes(newStrings[i]));
                mw.Write((byte)0);
            }
            mw.WritePadding(16, 0);
            return ms.ToArray();
        }

        private byte[] Int8(Tr2Info tr2Info)
        {
            INT8 int8 = JsonSerializer.Deserialize<INT8>(File.ReadAllText(tr2.FileName.Replace(".tr2", "\\") + tr2Info.Name + ".json"));


               
            MemoryStream ms = new MemoryStream();
            BW mw = new BW(ms);
            mw.Write(Encoding.UTF8.GetBytes(tr2Info.Name));
            mw.BaseStream.Seek(0x30, SeekOrigin.Begin);
            mw.Write(tr2Info.Dir);
            mw.Write(0);
            mw.Write(tr2Info.PointerType);
            mw.Write(tr2Info.Unk1);
            mw.Write(Encoding.UTF8.GetBytes(tr2Info.EncodingType));
            mw.BaseStream.Seek(0x74, SeekOrigin.Begin);
            mw.Write(tr2Info.Unk2);
            mw.Write(tr2Info.Unk3);
            mw.Write(tr2Info.Count);
            int newOff = (tr2Info.Count * 4) + 0x88;
            for (int i = 0; i < tr2Info.Count; i++)
            {
                mw.Write(newOff);
                int newLen = int8.Data[i].Count;
                newOff += newLen;
                
            }
            mw.Write(newOff);
            mw.BaseStream.Seek((long)(tr2Info.Count * 4) + 0x88, SeekOrigin.Begin);
            for (int i = 0; i < tr2Info.Count; i++)
            {
                foreach (var b in int8.Data[i])
                {
                    mw.Write(b);
                }
               
               
            }
            mw.WritePadding(16, 0);
            return ms.ToArray();
        }
        private byte[] Int16(Tr2Info tr2Info)
        {
            INT16 int16 = JsonSerializer.Deserialize<INT16>(File.ReadAllText(tr2.FileName.Replace(".tr2", "\\") + tr2Info.Name + ".json"));



            MemoryStream ms = new MemoryStream();
            BW mw = new BW(ms);
            mw.Write(Encoding.UTF8.GetBytes(tr2Info.Name));
            mw.BaseStream.Seek(0x30, SeekOrigin.Begin);
            mw.Write(tr2Info.Dir);
            mw.Write(0);
            mw.Write(tr2Info.PointerType);
            mw.Write(tr2Info.Unk1);
            mw.Write(Encoding.UTF8.GetBytes(tr2Info.EncodingType));
            mw.BaseStream.Seek(0x74, SeekOrigin.Begin);
            mw.Write(tr2Info.Unk2);
            mw.Write(tr2Info.Unk3);
            mw.Write(tr2Info.Count);
            int newOff = (tr2Info.Count * 4) + 0x88;
            for (int i = 0; i < tr2Info.Count; i++)
            {
                mw.Write(newOff);
                int newLen = int16.Data[i].Count*2;
                newOff += newLen;

            }
            mw.Write(newOff);
            mw.BaseStream.Seek((long)(tr2Info.Count * 4) + 0x88, SeekOrigin.Begin);
            for (int i = 0; i < tr2Info.Count; i++)
            {
                foreach (var b in int16.Data[i])
                {
                    mw.Write(b);
                }


            }
            mw.WritePadding(16, 0);
            return ms.ToArray();
        }
        private byte[] Int32(Tr2Info tr2Info)
        {
            INT32 int32 = JsonSerializer.Deserialize<INT32>(File.ReadAllText(tr2.FileName.Replace(".tr2", "\\") + tr2Info.Name + ".json"));



            MemoryStream ms = new MemoryStream();
            BW mw = new BW(ms);
            mw.Write(Encoding.UTF8.GetBytes(tr2Info.Name));
            mw.BaseStream.Seek(0x30, SeekOrigin.Begin);
            mw.Write(tr2Info.Dir);
            mw.Write(0);
            mw.Write(tr2Info.PointerType);
            mw.Write(tr2Info.Unk1);
            mw.Write(Encoding.UTF8.GetBytes(tr2Info.EncodingType));
            mw.BaseStream.Seek(0x74, SeekOrigin.Begin);
            mw.Write(tr2Info.Unk2);
            mw.Write(tr2Info.Unk3);
            mw.Write(tr2Info.Count);
            int newOff = (tr2Info.Count * 4) + 0x88;
            for (int i = 0; i < tr2Info.Count; i++)
            {
                mw.Write(newOff);
                int newLen = int32.Data[i].Count*4;
                newOff += newLen;

            }
            mw.Write(newOff);
            mw.BaseStream.Seek((long)(tr2Info.Count * 4) + 0x88, SeekOrigin.Begin);
            for (int i = 0; i < tr2Info.Count; i++)
            {
                foreach (var b in int32.Data[i])
                {
                    mw.Write(b);
                }


            }
            mw.WritePadding(16, 0);
            return ms.ToArray();
        }
        private byte[] UInt8(Tr2Info tr2Info)
        {
            UINT8 uint8 = JsonSerializer.Deserialize<UINT8>(File.ReadAllText(tr2.FileName.Replace(".tr2", "\\") + tr2Info.Name + ".json"));



            MemoryStream ms = new MemoryStream();
            BW mw = new BW(ms);
            mw.Write(Encoding.UTF8.GetBytes(tr2Info.Name));
            mw.BaseStream.Seek(0x30, SeekOrigin.Begin);
            mw.Write(tr2Info.Dir);
            mw.Write(0);
            mw.Write(tr2Info.PointerType);
            mw.Write(tr2Info.Unk1);
            mw.Write(Encoding.UTF8.GetBytes(tr2Info.EncodingType));
            mw.BaseStream.Seek(0x74, SeekOrigin.Begin);
            mw.Write(tr2Info.Unk2);
            mw.Write(tr2Info.Unk3);
            mw.Write(tr2Info.Count);
            int newOff = (tr2Info.Count * 4) + 0x88;
            for (int i = 0; i < tr2Info.Count; i++)
            {
                mw.Write(newOff);
                int newLen = uint8.Data[i].Count;
                newOff += newLen;

            }
            mw.Write(newOff);
            mw.BaseStream.Seek((long)(tr2Info.Count * 4) + 0x88, SeekOrigin.Begin);
            for (int i = 0; i < tr2Info.Count; i++)
            {
                foreach (var b in uint8.Data[i])
                {
                    mw.Write(b);
                }


            }
            mw.WritePadding(16, 0);
            return ms.ToArray();
        }
        private byte[] UInt16(Tr2Info tr2Info)
        {
            UINT16 uint16 = JsonSerializer.Deserialize<UINT16>(File.ReadAllText(tr2.FileName.Replace(".tr2", "\\") + tr2Info.Name + ".json"));



            MemoryStream ms = new MemoryStream();
            BW mw = new BW(ms);
            mw.Write(Encoding.UTF8.GetBytes(tr2Info.Name));
            mw.BaseStream.Seek(0x30, SeekOrigin.Begin);
            mw.Write(tr2Info.Dir);
            mw.Write(0);
            mw.Write(tr2Info.PointerType);
            mw.Write(tr2Info.Unk1);
            mw.Write(Encoding.UTF8.GetBytes(tr2Info.EncodingType));
            mw.BaseStream.Seek(0x74, SeekOrigin.Begin);
            mw.Write(tr2Info.Unk2);
            mw.Write(tr2Info.Unk3);
            mw.Write(tr2Info.Count);
            int newOff = (tr2Info.Count * 4) + 0x88;
            for (int i = 0; i < tr2Info.Count; i++)
            {
                mw.Write(newOff);
                int newLen = uint16.Data[i].Count * 2;
                newOff += newLen;

            }
            mw.Write(newOff);
            mw.BaseStream.Seek((long)(tr2Info.Count * 4) + 0x88, SeekOrigin.Begin);
            for (int i = 0; i < tr2Info.Count; i++)
            {
                foreach (var b in uint16.Data[i])
                {
                    mw.Write(b);
                }


            }
            mw.WritePadding(16, 0);
            return ms.ToArray();
        }
        private byte[] UInt32(Tr2Info tr2Info)
        {
            UINT32 uint32 = JsonSerializer.Deserialize<UINT32>(File.ReadAllText(tr2.FileName.Replace(".tr2", "\\") + tr2Info.Name + ".json"));



            MemoryStream ms = new MemoryStream();
            BW mw = new BW(ms);
            mw.Write(Encoding.UTF8.GetBytes(tr2Info.Name));
            mw.BaseStream.Seek(0x30, SeekOrigin.Begin);
            mw.Write(tr2Info.Dir);
            mw.Write(0);
            mw.Write(tr2Info.PointerType);
            mw.Write(tr2Info.Unk1);
            mw.Write(Encoding.UTF8.GetBytes(tr2Info.EncodingType));
            mw.BaseStream.Seek(0x74, SeekOrigin.Begin);
            mw.Write(tr2Info.Unk2);
            mw.Write(tr2Info.Unk3);
            mw.Write(tr2Info.Count);
            int newOff = (tr2Info.Count * 4) + 0x88;
            for (int i = 0; i < tr2Info.Count; i++)
            {
                mw.Write(newOff);
                int newLen = uint32.Data[i].Count * 4;
                newOff += newLen;

            }
            mw.Write(newOff);
            mw.BaseStream.Seek((long)(tr2Info.Count * 4) + 0x88, SeekOrigin.Begin);
            for (int i = 0; i < tr2Info.Count; i++)
            {
                foreach (var b in uint32.Data[i])
                {
                    mw.Write(b);
                }


            }
            mw.WritePadding(16, 0);
            return ms.ToArray();
        }
        private byte[] Float32(Tr2Info tr2Info)
        {
            FLOAT32 float32 = JsonSerializer.Deserialize<FLOAT32>(File.ReadAllText(tr2.FileName.Replace(".tr2", "\\") + tr2Info.Name + ".json"));



            MemoryStream ms = new MemoryStream();
            BW mw = new BW(ms);
            mw.Write(Encoding.UTF8.GetBytes(tr2Info.Name));
            mw.BaseStream.Seek(0x30, SeekOrigin.Begin);
            mw.Write(tr2Info.Dir);
            mw.Write(0);
            mw.Write(tr2Info.PointerType);
            mw.Write(tr2Info.Unk1);
            mw.Write(Encoding.UTF8.GetBytes(tr2Info.EncodingType));
            mw.BaseStream.Seek(0x74, SeekOrigin.Begin);
            mw.Write(tr2Info.Unk2);
            mw.Write(tr2Info.Unk3);
            mw.Write(tr2Info.Count);
            int newOff = (tr2Info.Count * 4) + 0x88;
            for (int i = 0; i < tr2Info.Count; i++)
            {
                mw.Write(newOff);
                int newLen = float32.Data[i].Count * 4;
                newOff += newLen;

            }
            mw.Write(newOff);
            mw.BaseStream.Seek((long)(tr2Info.Count * 4) + 0x88, SeekOrigin.Begin);
            for (int i = 0; i < tr2Info.Count; i++)
            {
                foreach (var b in float32.Data[i])
                {
                    mw.Write(b);
                }


            }
            mw.WritePadding(16, 0);
            return ms.ToArray();
        }

        private byte[] Int8Yobi(Tr2Info tr2Info)
        {
            INT8 int8 = JsonSerializer.Deserialize<INT8>(File.ReadAllText(tr2.FileName.Replace(".tr2", "\\") + tr2Info.Name + ".json"));



            MemoryStream ms = new MemoryStream();
            BW mw = new BW(ms);
            mw.Write(Encoding.UTF8.GetBytes(tr2Info.Name));
            mw.BaseStream.Seek(0x30, SeekOrigin.Begin);
            mw.Write(tr2Info.Dir);
            mw.Write(0);
            mw.Write(tr2Info.PointerType);
            mw.Write(tr2Info.Unk1);
            mw.Write(Encoding.UTF8.GetBytes(tr2Info.EncodingType));
            mw.BaseStream.Seek(0x74, SeekOrigin.Begin);
            mw.Write(tr2Info.Unk2);
            mw.Write(tr2Info.Unk3);
            mw.Write(tr2Info.Count);
            int newOff = (tr2Info.Count * 12) + 0x80;
            for (int i = 0; i < tr2Info.Count; i++)
            {
                mw.Write(tr2Info.IndexString[i]);
                mw.Write(newOff);
                int newLen = int8.Data[i].Count;
                mw.Write(newLen);
                newOff += newLen;
               


               

            }
            mw.Write(newOff);
            mw.BaseStream.Seek((long)(tr2Info.Count * 12) + 0x80, SeekOrigin.Begin);
            for (int i = 0; i < tr2Info.Count; i++)
            {
                foreach (var b in int8.Data[i])
                {
                    mw.Write(b);
                }


            }
            mw.WritePadding(16, 0);
            return ms.ToArray();
        }
        private byte[] Int16Yobi(Tr2Info tr2Info)
        {
            INT16 int16 = JsonSerializer.Deserialize<INT16>(File.ReadAllText(tr2.FileName.Replace(".tr2", "\\") + tr2Info.Name + ".json"));



            MemoryStream ms = new MemoryStream();
            BW mw = new BW(ms);
            mw.Write(Encoding.UTF8.GetBytes(tr2Info.Name));
            mw.BaseStream.Seek(0x30, SeekOrigin.Begin);
            mw.Write(tr2Info.Dir);
            mw.Write(0);
            mw.Write(tr2Info.PointerType);
            mw.Write(tr2Info.Unk1);
            mw.Write(Encoding.UTF8.GetBytes(tr2Info.EncodingType));
            mw.BaseStream.Seek(0x74, SeekOrigin.Begin);
            mw.Write(tr2Info.Unk2);
            mw.Write(tr2Info.Unk3);
            mw.Write(tr2Info.Count);
            int newOff = (tr2Info.Count * 12) + 0x80;
            for (int i = 0; i < tr2Info.Count; i++)
            {
                mw.Write(tr2Info.IndexString[i]);
                mw.Write(newOff);
                int newLen = int16.Data[i].Count*2;
                mw.Write(newLen);
                newOff += newLen;
               

            }
            mw.Write(newOff);
            mw.BaseStream.Seek((long)(tr2Info.Count * 12) + 0x80, SeekOrigin.Begin);
            for (int i = 0; i < tr2Info.Count; i++)
            {
                foreach (var b in int16.Data[i])
                {
                    mw.Write(b);
                }


            }
            mw.WritePadding(16, 0);
            return ms.ToArray();
        }
        private byte[] Int32Yobi(Tr2Info tr2Info)
        {
            INT32 int32 = JsonSerializer.Deserialize<INT32>(File.ReadAllText(tr2.FileName.Replace(".tr2", "\\") + tr2Info.Name + ".json"));



            MemoryStream ms = new MemoryStream();
            BW mw = new BW(ms);
            mw.Write(Encoding.UTF8.GetBytes(tr2Info.Name));
            mw.BaseStream.Seek(0x30, SeekOrigin.Begin);
            mw.Write(tr2Info.Dir);
            mw.Write(0);
            mw.Write(tr2Info.PointerType);
            mw.Write(tr2Info.Unk1);
            mw.Write(Encoding.UTF8.GetBytes(tr2Info.EncodingType));
            mw.BaseStream.Seek(0x74, SeekOrigin.Begin);
            mw.Write(tr2Info.Unk2);
            mw.Write(tr2Info.Unk3);
            mw.Write(tr2Info.Count);
            int newOff = (tr2Info.Count * 12) + 0x80;
            for (int i = 0; i < tr2Info.Count; i++)
            {
                mw.Write(tr2Info.IndexString[i]);
                mw.Write(newOff);
                int newLen = int32.Data[i].Count * 4;
                mw.Write(newLen);
                newOff += newLen;
               

            }
            mw.Write(newOff);
            mw.BaseStream.Seek((long)(tr2Info.Count * 12) + 0x80, SeekOrigin.Begin);
            for (int i = 0; i < tr2Info.Count; i++)
            {
                foreach (var b in int32.Data[i])
                {
                    mw.Write(b);
                }


            }
            mw.WritePadding(16, 0);
            return ms.ToArray();
        }
        private byte[] UInt8Yobi(Tr2Info tr2Info)
        {
            UINT8 uint8 = JsonSerializer.Deserialize<UINT8>(File.ReadAllText(tr2.FileName.Replace(".tr2", "\\") + tr2Info.Name + ".json"));



            MemoryStream ms = new MemoryStream();
            BW mw = new BW(ms);
            mw.Write(Encoding.UTF8.GetBytes(tr2Info.Name));
            mw.BaseStream.Seek(0x30, SeekOrigin.Begin);
            mw.Write(tr2Info.Dir);
            mw.Write(0);
            mw.Write(tr2Info.PointerType);
            mw.Write(tr2Info.Unk1);
            mw.Write(Encoding.UTF8.GetBytes(tr2Info.EncodingType));
            mw.BaseStream.Seek(0x74, SeekOrigin.Begin);
            mw.Write(tr2Info.Unk2);
            mw.Write(tr2Info.Unk3);
            mw.Write(tr2Info.Count);
            int newOff = (tr2Info.Count * 12) + 0x80;
            for (int i = 0; i < tr2Info.Count; i++)
            {
                mw.Write(tr2Info.IndexString[i]);
                mw.Write(newOff);
                int newLen = uint8.Data[i].Count;
                mw.Write(newLen);
                newOff += newLen;

            }
            mw.Write(newOff);
            mw.BaseStream.Seek((long)(tr2Info.Count * 12) + 0x80, SeekOrigin.Begin);
            for (int i = 0; i < tr2Info.Count; i++)
            {
                foreach (var b in uint8.Data[i])
                {
                    mw.Write(b);
                }


            }
            mw.WritePadding(16, 0);
            return ms.ToArray();
        }
        private byte[] UInt16Yobi(Tr2Info tr2Info)
        {
            UINT16 uint16 = JsonSerializer.Deserialize<UINT16>(File.ReadAllText(tr2.FileName.Replace(".tr2", "\\") + tr2Info.Name + ".json"));



            MemoryStream ms = new MemoryStream();
            BW mw = new BW(ms);
            mw.Write(Encoding.UTF8.GetBytes(tr2Info.Name));
            mw.BaseStream.Seek(0x30, SeekOrigin.Begin);
            mw.Write(tr2Info.Dir);
            mw.Write(0);
            mw.Write(tr2Info.PointerType);
            mw.Write(tr2Info.Unk1);
            mw.Write(Encoding.UTF8.GetBytes(tr2Info.EncodingType));
            mw.BaseStream.Seek(0x74, SeekOrigin.Begin);
            mw.Write(tr2Info.Unk2);
            mw.Write(tr2Info.Unk3);
            mw.Write(tr2Info.Count);
            int newOff = (tr2Info.Count * 12) + 0x80;
            for (int i = 0; i < tr2Info.Count; i++)
            {
                mw.Write(tr2Info.IndexString[i]);
                mw.Write(newOff);
                int newLen = uint16.Data[i].Count*2;
                mw.Write(newLen);
                newOff += newLen;
               

            }
            mw.Write(newOff);
            mw.BaseStream.Seek((long)(tr2Info.Count * 12) + 0x80, SeekOrigin.Begin);
            for (int i = 0; i < tr2Info.Count; i++)
            {
                foreach (var b in uint16.Data[i])
                {
                    mw.Write(b);
                }


            }
            mw.WritePadding(16, 0);
            return ms.ToArray();
        }
        private byte[] UInt32Yobi(Tr2Info tr2Info)
        {
            UINT32 uint32 = JsonSerializer.Deserialize<UINT32>(File.ReadAllText(tr2.FileName.Replace(".tr2", "\\") + tr2Info.Name + ".json"));



            MemoryStream ms = new MemoryStream();
            BW mw = new BW(ms);
            mw.Write(Encoding.UTF8.GetBytes(tr2Info.Name));
            mw.BaseStream.Seek(0x30, SeekOrigin.Begin);
            mw.Write(tr2Info.Dir);
            mw.Write(0);
            mw.Write(tr2Info.PointerType);
            mw.Write(tr2Info.Unk1);
            mw.Write(Encoding.UTF8.GetBytes(tr2Info.EncodingType));
            mw.BaseStream.Seek(0x74, SeekOrigin.Begin);
            mw.Write(tr2Info.Unk2);
            mw.Write(tr2Info.Unk3);
            mw.Write(tr2Info.Count);
            int newOff = (tr2Info.Count * 12) + 0x80;
            for (int i = 0; i < tr2Info.Count; i++)
            {
                mw.Write(tr2Info.IndexString[i]);
                mw.Write(newOff);
                int newLen = uint32.Data[i].Count * 4;
                mw.Write(newLen);
                newOff += newLen;
               
            }
            mw.Write(newOff);
            mw.BaseStream.Seek((long)(tr2Info.Count * 12) + 0x80, SeekOrigin.Begin);
            for (int i = 0; i < tr2Info.Count; i++)
            {
                foreach (var b in uint32.Data[i])
                {
                    mw.Write(b);
                }


            }
            mw.WritePadding(16, 0);
            return ms.ToArray();
        }
        private byte[] Float32Yobi(Tr2Info tr2Info)
        {
            FLOAT32 float32 = JsonSerializer.Deserialize<FLOAT32>(File.ReadAllText(tr2.FileName.Replace(".tr2", "\\") + tr2Info.Name + ".json"));



            MemoryStream ms = new MemoryStream();
            BW mw = new BW(ms);
            mw.Write(Encoding.UTF8.GetBytes(tr2Info.Name));
            mw.BaseStream.Seek(0x30, SeekOrigin.Begin);
            mw.Write(tr2Info.Dir);
            mw.Write(0);
            mw.Write(tr2Info.PointerType);
            mw.Write(tr2Info.Unk1);
            mw.Write(Encoding.UTF8.GetBytes(tr2Info.EncodingType));
            mw.BaseStream.Seek(0x74, SeekOrigin.Begin);
            mw.Write(tr2Info.Unk2);
            mw.Write(tr2Info.Unk3);
            mw.Write(tr2Info.Count);
            int newOff = (tr2Info.Count * 12) + 0x80;
            for (int i = 0; i < tr2Info.Count; i++)
            {
                mw.Write(tr2Info.IndexString[i]);
                mw.Write(newOff);
                int newLen = float32.Data[i].Count * 4;
                mw.Write(newLen);
                newOff += newLen;
               

            }
            mw.Write(newOff);
            mw.BaseStream.Seek((long)(tr2Info.Count * 12) + 0x80, SeekOrigin.Begin);
            for (int i = 0; i < tr2Info.Count; i++)
            {
                foreach (var b in float32.Data[i])
                {
                    mw.Write(b);
                }


            }
            mw.WritePadding(16, 0);
            return ms.ToArray();
        }

        private List<string> LoadTxt(string txtName, int count)
        {
            List<string> listString = new List<string>();
            FileStream fileStream = new FileStream(txtName, FileMode.Open, FileAccess.Read);
            StreamReader txtReader = new StreamReader(fileStream);
            int txtIndex;
            for (int i = 0; i < count; i++)
            {
                try
                {
                    txtIndex = int.Parse(txtReader.ReadLine().Substring(1, 4));
                    if (txtIndex != i)
                    {
                        Console.WriteLine("Mismatch txt index");
                        Environment.Exit(0);
                    }
                }
                catch (FormatException)
                {
                    Console.WriteLine("Broken txt index");
                    Environment.Exit(0);
                }
                string text = "";
                while (true)
                {
                    string line = txtReader.ReadLine();

                    if (line.Length > 10)
                    {
                        if (line.Substring(0, 10) == "----------")
                        {
                            text = text.Substring(0, text.Length - 1);

                            break;
                        }
                    }
                    text += line + "\n";
                }

                listString.Add(text);
            }
            fileStream.Close();
            return listString;
        }   
    }


}
