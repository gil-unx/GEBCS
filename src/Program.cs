using System;
using System.IO;
using System.Collections.Generic;
using GIL.FUNCTION;
using System.Xml.Serialization;

namespace GEBCS
{
    class Program
    {
        static void Unpack()
        {
            ResNames resName = new ResNames();
            Tr2Names tr2Name = new Tr2Names();
            resName.Names = new List<string>();
            resName.Names.Add(".\\system.res");
            tr2Name.Names = new List<string>();
            FileStream fileStream = new FileStream("package.rdp", FileMode.Open, FileAccess.Read);
            BR package = new BR(fileStream);
            PresUnpack pres = new PresUnpack(".\\system.res");
            pres.Unpack(ref package, ref resName,ref tr2Name);
            package.Close();

            XmlSerializer resNameSerial = new XmlSerializer(typeof(ResNames));
            TextWriter resNameWriter = new StreamWriter("Resnames.xml");
            resNameSerial.Serialize(resNameWriter, resName);
            resNameWriter.Close();

            XmlSerializer tr2NameSerial = new XmlSerializer(typeof(Tr2Names));
            TextWriter tr2NameWriter = new StreamWriter("Tr2names.xml");
            tr2NameSerial.Serialize(tr2NameWriter, tr2Name);
            tr2NameWriter.Close();

            Console.WriteLine("Unpack finished ,pres any key");
            Console.ReadKey();

        }
        static void UnpackDlc(string smallEdat ,string bigEdat)
        {
            ResNames resName = new ResNames();
            Tr2Names tr2Name = new Tr2Names();
            resName.Names = new List<string>();
            resName.Names.Add(".\\"+smallEdat);
            tr2Name.Names = new List<string>();
            FileStream fileStream = new FileStream(".\\"+bigEdat, FileMode.Open, FileAccess.Read);
            BR package = new BR(fileStream);
            PresUnpack pres = new PresUnpack(".\\"+smallEdat,true);
            pres.Unpack(ref package, ref resName, ref tr2Name);
            package.Close();

            XmlSerializer resNameSerial = new XmlSerializer(typeof(ResNames));
            TextWriter resNameWriter = new StreamWriter("Resnames.xml");
            resNameSerial.Serialize(resNameWriter, resName);
            resNameWriter.Close();

            XmlSerializer tr2NameSerial = new XmlSerializer(typeof(Tr2Names));
            TextWriter tr2NameWriter = new StreamWriter("Tr2names.xml");
            tr2NameSerial.Serialize(tr2NameWriter, tr2Name);
            tr2NameWriter.Close();

            Console.WriteLine("Unpack finished ,pres any key");
            Console.ReadKey();

        }
        static void Repack()
        {
            long pointOffset = 0x50500000;
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(ResNames));
            Stream reader = new FileStream("Resnames.xml", FileMode.Open, FileAccess.Read);
            ResNames resNames = (ResNames)xmlSerializer.Deserialize(reader);
            FileStream fileStream = new FileStream("package.rdp", FileMode.Open, FileAccess.ReadWrite);
            BW package = new BW(fileStream);
           
            for(int i = resNames.Names.Count-1; i >= 0; i--)
            {
                PresRepack pres = new PresRepack(resNames.Names[i]);
                pres.Repack(ref package, ref pointOffset);

            }
            package.Close();

            Console.WriteLine("Repack finished ,pres any key");
            Console.ReadKey();

        }
        static void RepackDlc(string smallEdat, string bigEdat)
        {
            long pointOffset = 0x0;
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(ResNames));
            Stream reader = new FileStream("Resnames.xml", FileMode.Open, FileAccess.Read);
            ResNames resNames = (ResNames)xmlSerializer.Deserialize(reader);
            FileStream fileStream = new FileStream(".\\" + bigEdat, FileMode.Open, FileAccess.ReadWrite);
            BW package = new BW(fileStream);

            for (int i = resNames.Names.Count - 1; i >= 0; i--)
            {
                PresRepack pres = new PresRepack(resNames.Names[i],true);
                pres.Repack(ref package, ref pointOffset);

            }
            package.Close();

            Console.WriteLine("Repack finished ,pres any key");
            Console.ReadKey();

        }
        static void Usage()
        {
            Console.WriteLine("Unpack: GEBCS.exe -x ");
            Console.WriteLine("Repack: GEBCS.exe -c ");
            Console.WriteLine("DLC Unpack: GEBCS.exe -xdlc [small].edat [big].edat");
            Console.WriteLine("DLC Repack: GEBCS.exe -cdlc  [small].edat [big].edat ");
            Console.WriteLine("Note: Put GEBCS.exe, system.res and package.rdp in same folder ");
            Environment.Exit(0);
            
        }
        static void Main(string[] args)
        {
            string mode = "";
            Console.WriteLine("God Eater Burst package unpacker");
            Console.WriteLine("================================");
            if (args.Length < 1) Usage();
            mode = args[0].ToUpper();
            if (mode == "-X")
            {
                Unpack();
            }
            else if(mode == "-XDLC")
            {
                UnpackDlc(args[1], args[2]);
            }
            else if (mode == "-C")
            {
                Repack();
            }
            else if (mode == "-CDLC")
            {
                RepackDlc(args[1], args[2]);
            }
            else
            {
                Usage();
            }
        }
    }
}
