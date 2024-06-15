using System;
using System.IO;
using System.Collections.Generic;
using System.Xml.Serialization;
using GIL.FUNCTION;
using System.IO.Compression;

namespace GEBCS
{
    public class PresUnpack 
    {
        private Pres pres = new Pres();
        private BR reader;
        private Dictionary<long,int> grup = new Dictionary<long, int>();
        private string outFolder;
        private Dictionary<string, int> duplicateCek = new Dictionary<string, int>();
        private string duplicate = "";
        private FileStream fileStream;
        private bool isDlc = false;
        public PresUnpack(string fileName,bool dlc = false)
        {
            isDlc = dlc;
            fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read);   
            outFolder = Path.GetDirectoryName(fileName)+"\\" + Path.GetFileNameWithoutExtension(fileName) + "\\";
            reader = new BR(fileStream);
            pres.Filename = fileName;
            pres.Magic = reader.ReadUInt32();
            pres.GrupOffset = reader.ReadInt32();
            pres.GrupCount = reader.ReadInt32();
            pres.CeckSum = reader.ReadUInt32();
            pres.TotalFile = 0;
            pres.Grups = new List<int>();
            for(int i = 0; i<8; i++)
            {
                int off = reader.ReadInt32();
                int count = reader.ReadInt32();
                pres.Grups.Add(off);
                pres.Grups.Add(count);
                if(off>0)
                {
                    grup.Add((long)off,count);
                    pres.TotalFile+=count;
                }
                               
            }                  
            GenerateFileInfo();
        }
        private void GenerateFileInfo()
        {
            pres.Files = new List<Record>();
            MemoryStream memory = new MemoryStream(reader.ReadBytes(pres.TotalFile*32));
            pres.TocSize = (int)reader.BaseStream.Position;
            BinaryReader toc = new BinaryReader(memory);
            for(int i = 0; i < grup.Count; i++)
            {
                int count = grup[toc.BaseStream.Position+0x50];
                for(int j = 0; j < count;j++)
                {
                    Record file = new Record();
                    file.Offset = toc.ReadInt32();
                    file.Size = toc.ReadInt32();
                    file.OffsetName = toc.ReadInt32();
                    file.ChunkName = toc.ReadInt32();
                    toc.ReadBytes(16);
                    file.UpdatePointer = new List<bool>(){
                        Convert.ToBoolean(file.Offset),
                        Convert.ToBoolean(file.Size),
                        Convert.ToBoolean(file.OffsetName),
                        Convert.ToBoolean(file.ChunkName),                       
                    };
                    if(file.ChunkName>0)
                    {
                        reader.BaseStream.Seek((long)file.OffsetName,SeekOrigin.Begin);
                        file.ElementName = new List<string>();
                        for(int k = 0; k <file.ChunkName; k++)
                        {
                            int offChunk = reader.ReadInt32();
                            
                            file.ElementName.Add(reader.GetUtf8((long)offChunk));
                        }
                        string dupName = String.Join("",file.ElementName).ToUpper();
                        try
                        {
                            duplicate = String.Format("_{0,0:d4}",duplicateCek[dupName]);
                            duplicateCek[dupName]++;                            
                        }
                        catch(Exception)
                        {
                            duplicateCek.Add(dupName,1);
                        }
                    }
                    file.RealSize = file.Size;
                    if(file.ChunkName == 0)
                    {
                        file.FileName = "dummy";
                    }
                    if(file.ChunkName == 1)
                    {
                        file.FileName = file.ElementName[0];
                        file.RealSize = file.OffsetName-file.Offset;
                    }
                    if(file.ChunkName > 1)
                    {
                    	for(int k = 2; k < file.ChunkName;k++)
                    	{
                    		file.FileName += file.ElementName[k]+"\\";
                    	}
                    	file.FileName+=(file.ElementName[0]+duplicate+"."+file.ElementName[1]);
                    }                   
                    duplicate = "";
                    int cek =  file.Offset+ file.RealSize+(((16 - (file.RealSize % 16) | 16))-16);
                    if(cek == file.OffsetName)
                    {
                        file.Location = "Local";
                    }
                    else
                    {
                        if(file.ChunkName == 1)
                        {
                            file.Location = "Local";
                        }
                        else
                        {
                            if ((file.UpdatePointer[0] == true) && (file.UpdatePointer[1] == false) && (file.UpdatePointer[2] == true) && (file.UpdatePointer[3] == true))
                            {
                                file.RealSize = file.OffsetName - file.Offset;
                                file.Location = "Local";
                                file.ShiftOffset = 0;
                            }
                            else
                            {
                                file.Location = "Package";
                                file.ShiftOffset = 15;
                                if (isDlc)
                                {
                                    file.ShiftOffset = 4;
                                }
                            }                          
                        }
                    }
                    if(file.FileName == "dummy")
                    {
                        file.Location = "dummy";
                    }
                    pres.Files.Add(file);
                }               
            }                                                 
        }
        public void Unpack(ref BR package,ref ResNames resNames,ref Tr2Names tr2Names)
        {
            for(int i = 0; i < pres.TotalFile; i++)
            {
                Record file = pres.Files[i];
                if ((file.FileName != "dummy")&& (file.Location != "dummy"))
                {                  
                    Directory.CreateDirectory(Path.GetDirectoryName(outFolder + file.FileName));
                    Console.WriteLine("Extract: " + outFolder + file.FileName);
                    int offset = file.Offset << file.ShiftOffset;
                    int size = file.RealSize;
                    byte[] comBuffer;
                    switch (file.Location)
                    {
                        case "Local":
                            comBuffer = reader.GetBytes((long)offset, size);                           
                            break;
                        case "Package":
                            comBuffer = package.GetBytes((long)offset, file.Size);
                            int padding = 0x8000 - (file.Size % 0x8000);
                            if (padding == 0x8000) padding = 0;
                            pres.Files[i].MaxSize = file.Size + padding;
                            break;
                        default:
                            comBuffer = new byte[0];
                            
                            break;
                    }
                    if(comBuffer.Length > 0)
                    {
                        if ((comBuffer[0] == 0x1f) && (comBuffer[1] == 0x8b) && (comBuffer[2] == 0x08) && (comBuffer[4] == 0x00))
                        {
                            file.Compression = true;
                            byte[] decBuffer = Decompress(comBuffer);
                            File.WriteAllBytes(outFolder + file.FileName, decBuffer);
                        }
                        else
                        {
                            File.WriteAllBytes(outFolder + file.FileName, comBuffer);

                        }                                               
                    }
                    else
                    {
                        File.WriteAllBytes(outFolder + file.FileName, comBuffer);
                    }                   
                }
                if(Path.GetExtension(file.FileName) == ".res")
                {
                    resNames.Names.Add(outFolder + file.FileName);                                                        
                    PresUnpack pres = new PresUnpack((outFolder + file.FileName));
                    pres.Unpack(ref package, ref resNames , ref tr2Names);                  
                }
                if (Path.GetExtension(file.FileName) == ".tr2")
                {
                    tr2Names.Names.Add(outFolder + file.FileName);
                    new Tr2Decoder(outFolder + file.FileName);
                }
            }
            CreateXml();
        }      
        private void CreateXml()
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(Pres));
            TextWriter writer = new StreamWriter(Path.ChangeExtension(pres.Filename,"xml"));
            xmlSerializer.Serialize(writer, pres);
            writer.Close();
        }
        private byte[] Decompress(byte[] gzip)
        {
            using (GZipStream stream = new GZipStream(new MemoryStream(gzip),CompressionMode.Decompress))
            {
                const int size = 4096;
                byte[] buffer = new byte[size];
                using (MemoryStream memory = new MemoryStream())
                {
                    int count = 0;
                    do
                    {
                        count = stream.Read(buffer, 0, size);
                        if (count > 0)
                        {
                            memory.Write(buffer, 0, count);
                        }
                    }
                    while (count > 0);
                    return memory.ToArray();
                }
            }
        }
    }
}