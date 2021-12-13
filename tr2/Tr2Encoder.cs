using System;
using System.IO;
using System.Collections.Generic;
using System.Xml.Serialization;
using GIL.FUNCTION;
using System.Text;

namespace GEBCS
{
    class Tr2Encoder
    {
        private Tr2 tr2 = new Tr2();
        private BW writer;
        public Tr2Encoder(string fileName)
        {
            Console.WriteLine("Encoding: "+fileName);
            XmlSerializer tr2Serial = new XmlSerializer(typeof(Tr2));
            Stream xmlReader = new FileStream(Path.ChangeExtension(fileName, "xml"), FileMode.Open, FileAccess.Read);
            try
            {
                tr2 = (Tr2)tr2Serial.Deserialize(xmlReader);
            }
            catch (InvalidOperationException)
            {
                Console.WriteLine("");
                return;
            }
           
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
                            buffer = Utf8Yobi(tr2Info);
                        }
                        else
                        {
                            buffer = Utf8(tr2Info);
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
        private byte[] Utf8(Tr2Info tr2Info)
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
                newOff += (int)Encoding.UTF8.GetBytes(newStrings[i]).Length;
                newOff++;
            }
            mw.Write(newOff);
            mw.BaseStream.Seek((long)(tr2Info.Count * 4) + 0x88,SeekOrigin.Begin);
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
