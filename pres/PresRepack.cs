﻿using System;
using System.IO;
using System.Xml.Serialization;
using System.IO.Compression;
using System.Text;
using GIL.FUNCTION;
namespace GEBCS
{
    class PresRepack
    {
        private Pres pres = new Pres();
        private BW writer;
        private string outFolder;
        private MemoryStream resStream;     
        public PresRepack(string resName)
        {
            XmlSerializer presSerial = new XmlSerializer(typeof(Pres));
            Stream xmlReader = new FileStream(Path.ChangeExtension(resName,"xml"), FileMode.Open, FileAccess.Read);
            pres = (Pres)presSerial.Deserialize(xmlReader);
            outFolder = Path.GetDirectoryName(resName) + "\\" + Path.GetFileNameWithoutExtension(resName) + "\\";
            resStream = new MemoryStream();
            writer = new BW(resStream);
            writer.Write(pres.Magic);
            writer.Write(pres.GrupOffset);
            writer.Write(pres.GrupCount);
            writer.Write(pres.CeckSum & 3);
            foreach(int i in pres.Grups)
            {
                writer.Write(i);
            }
        }
        public void Repack(ref BW package)
        {
            MemoryStream memory = new MemoryStream();
            BW newFiles = new BW(memory);
            foreach (Record file in pres.Files)
            {
                Console.WriteLine("Repack: "+outFolder + file.FileName);
                if (file.Location == "Local")
                {
                    if (Path.GetExtension(file.FileName) == ".tr2") new Tr2Encoder(outFolder + file.FileName);
            
                    byte[] buffer = File.ReadAllBytes(outFolder + file.FileName);
                    file.Offset = (int)newFiles.BaseStream.Position + pres.TocSize;
                    if (file.ChunkName == 1)
                    {

                    }
                    else if ((file.UpdatePointer[0] == true) && (file.UpdatePointer[1] == false) && (file.UpdatePointer[2] == true) && (file.UpdatePointer[3] == true))
                    {
                        file.Size = 0;
                    }
                    else
                    {
                        file.Size = buffer.Length;
                    }
                    newFiles.Write(buffer);
                    newFiles.WritePadding(16, 0);
                    file.OffsetName = (int)newFiles.BaseStream.Position + pres.TocSize;
                    MemoryStream arrName = new MemoryStream();
                    int baseName = (int)newFiles.BaseStream.Position + pres.TocSize + (file.ChunkName * 4);
                    foreach (string cname in file.ElementName)
                    {
                        newFiles.Write((int)arrName.Position + baseName);
                        arrName.Write(Encoding.UTF8.GetBytes(cname), 0, cname.Length);
                        arrName.Write(new byte[1], 0, 1);
                    }
                    newFiles.Write(arrName.ToArray());
                    newFiles.WritePadding(16, 0);
                }
                else if (file.Location == "Package")
                {
                    byte[] buffer = File.ReadAllBytes(outFolder + file.FileName);
                    byte[] compBuffer;
                    if (file.Compression == true)
                    {
                        compBuffer = Compress(buffer);
                    }
                    else
                    {
                        compBuffer = buffer;
                    }
                    file.Size = compBuffer.Length;
                    if(file.Size > file.MaxSize)
                    {
                        Console.WriteLine("Max size reached!!\nFile :\n{0}\nmaxsize :{1, 0:X8}\nnew size :{2, 0:X8}", outFolder + file.FileName,file.MaxSize,file.Size);
                        Environment.Exit(0);
                    }
                    package.BaseStream.Seek(file.Offset << file.ShiftOffset, SeekOrigin.Begin);
                    package.Write(compBuffer);
                    package.WritePadding(0X8000, 0);
                    file.OffsetName = (int)newFiles.BaseStream.Position + pres.TocSize;
                    MemoryStream arrName = new MemoryStream();
                    int baseName = (int)newFiles.BaseStream.Position + pres.TocSize + (file.ChunkName * 4);
                    foreach (string cname in file.ElementName)
                    {
                        newFiles.Write((int)arrName.Position + baseName);
                        arrName.Write(Encoding.UTF8.GetBytes(cname), 0, cname.Length);
                        arrName.Write(new byte[1], 0, 1);
                    }
                    newFiles.Write(arrName.ToArray());
                    newFiles.WritePadding(16, 0);
                }
                writer.Write(file.Offset);
                writer.Write(file.Size);
                writer.Write(file.OffsetName);
                writer.Write(file.ChunkName);
                writer.BaseStream.Seek(16, SeekOrigin.Current);
            }
            writer.Write(memory.ToArray());
            writer.Flush();
            CalcCecksum(pres.Filename);
        }
        private byte[] Compress(byte[] raw)
        {
            using (MemoryStream memory = new MemoryStream())
            {
                using (GZipStream gzip = new GZipStream(memory, CompressionLevel.Optimal,true))
                {      
                    gzip.Write(raw, 0, raw.Length);
                }
                return memory.ToArray();
            }
        }
        private void CalcCecksum(string destName)
        {
            byte[] destAarray = resStream.ToArray();
            uint t = 0;
            uint cecksum = 0;
            foreach (byte b in destAarray)
            {          
                cecksum = b ^ t;
                t = b + cecksum;
            }
            cecksum = (cecksum << 2)+ (pres.CeckSum & 3);
            destAarray[15] = (byte)(cecksum / 16777216 & byte.MaxValue);
            destAarray[14] = (byte)(cecksum / 65536 & byte.MaxValue);
            destAarray[13] = (byte)(cecksum / 256 & byte.MaxValue);
            destAarray[12] = (byte)(cecksum & byte.MaxValue);
            File.WriteAllBytes(destName, destAarray);
        }
    }
}