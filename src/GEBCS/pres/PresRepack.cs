using GECV_EX_TR2_Editor_GUI;
using GIL.FUNCTION;
using Ionic.Zlib;
using System.Text;
using System.Text.Json;
namespace GEBCS
{
    class PresRepack
    {
        private Pres pres = new Pres();
        private BW writer;
        private string outFolder;
        private MemoryStream resStream;
        private bool isDlc = false;
        Dictionary<UInt64, int> ptSame = new Dictionary<UInt64, int>();
        public PresRepack(string resName, bool dlc = false)
        {
            isDlc = dlc;
            pres = JsonSerializer.Deserialize<Pres>(File.ReadAllText(Path.ChangeExtension(resName, "json")));


            outFolder = Path.GetDirectoryName(resName) + "\\" + Path.GetFileNameWithoutExtension(resName) + "\\";
            resStream = new MemoryStream();
            writer = new BW(resStream);
            writer.Write(pres.Magic);
            writer.Write(pres.GrupOffset);
            writer.Write(pres.GrupCount);
            writer.Write(pres.CeckSum & 3);
            foreach (int i in pres.Grups)
            {
                writer.Write(i);
            }
        }
        public void RepackF(ref BW package, ref Dictionary<int, int> ptSeekSame, ref Dictionary<string, int> packageDict,CompressionLevel level = CompressionLevel.Default)
        {
            MemoryStream memory = new MemoryStream();
            BW newFiles = new BW(memory);
            
            foreach (Record file in pres.Files)
            {
                Console.WriteLine("Repack: " + outFolder + file.FileName);
                if (file.Location == "Local")
                {
                   


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
                else if (file.Location == "PackageFiles")
                {
                    byte[] buffer = File.ReadAllBytes(outFolder + file.FileName);
                    byte[] compBuffer;
                    if (isDlc)
                    {
                        
                        if (file.Compression == true)
                        {
         
                            compBuffer = Compress(buffer, level);
                            file.Size = compBuffer.Length;
                           
                        }
                        else
                        {
                            compBuffer = buffer;
                            file.Size = compBuffer.Length;
                        }


                        int newOffset;

                        if (ptSeekSame.TryGetValue(packageDict[outFolder + file.FileName], out newOffset))
                        {
                        }
                        else
                        {
                            newOffset = (int)package.BaseStream.Position >> file.ShiftOffset;
                            ptSeekSame.Add(packageDict[outFolder + file.FileName], newOffset);
                            package.Write(compBuffer);
                            package.WritePadding(0X10, 0);
                           



                        }
                        file.Offset = newOffset;


                    }
                    else
                    {

                        if (file.Compression == true)
                        {
                            compBuffer = Compress(buffer, level);
                            file.Size = compBuffer.Length;
                           
                        }
                        else
                        {
                            compBuffer = buffer;
                            file.Size = compBuffer.Length;

                        }

                        int newOffset;


                        if (ptSeekSame.TryGetValue(packageDict[outFolder + file.FileName], out newOffset))
                        {
                            
                        }
                        else
                        {
                            newOffset = (int)package.BaseStream.Position >> file.ShiftOffset;
                            ptSeekSame.Add(packageDict[outFolder + file.FileName], newOffset);
                            package.Write(compBuffer);
                            package.WritePadding(0X8000, 0);
                           
                        }
                        file.Offset = newOffset;


                    }

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
        private byte[] Compress(byte[] raw, CompressionLevel level)
        {
            using (MemoryStream memory = new MemoryStream())
            {
                using (GZipStream gzip = new GZipStream(memory, CompressionMode.Compress, level))
                {
                    gzip.Write(raw, 0, raw.Length);
                }
                return memory.ToArray();
            }
        }
        private UInt64 CalcCecksumU(byte[] destAarray)
        {

            uint t = 0;
            uint cecksum = 0;
            foreach (byte b in destAarray)
            {
                cecksum = b ^ t;
                t = b + cecksum;
            }
            return cecksum;
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
            cecksum = (cecksum << 2) + (pres.CeckSum & 3);
            destAarray[15] = (byte)(cecksum / 16777216 & byte.MaxValue);
            destAarray[14] = (byte)(cecksum / 65536 & byte.MaxValue);
            destAarray[13] = (byte)(cecksum / 256 & byte.MaxValue);
            destAarray[12] = (byte)(cecksum & byte.MaxValue);
            File.WriteAllBytes(destName, destAarray);
        }
    }
}