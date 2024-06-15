
using GIL.FUNCTION;
using System.Text.Json;

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
            pres.Unpack(ref package, ref resName, ref tr2Name);
            package.Close();

       
            File.WriteAllText("Resnames.json", JsonSerializer.Serialize(resName, new JsonSerializerOptions() { WriteIndented = true, Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping }));

     
            File.WriteAllText("Tr2names.json", JsonSerializer.Serialize(tr2Name, new JsonSerializerOptions() { WriteIndented = true, Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping }));

            Console.WriteLine("Unpack finished ,pres any key");
            Console.ReadKey();

        }
        static void UnpackDlc(string smallEdat, string bigEdat)
        {
            ResNames resName = new ResNames();
            Tr2Names tr2Name = new Tr2Names();
            resName.Names = new List<string>();
            resName.Names.Add(".\\" + smallEdat);
            tr2Name.Names = new List<string>();
            FileStream fileStream = new FileStream(".\\" + bigEdat, FileMode.Open, FileAccess.Read);
            BR package = new BR(fileStream);
            PresUnpack pres = new PresUnpack(".\\" + smallEdat, true);
            pres.Unpack(ref package, ref resName, ref tr2Name);
            package.Close();


            File.WriteAllText("ResnamesDlc.json", JsonSerializer.Serialize(resName, new JsonSerializerOptions() { WriteIndented = true, Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping }));

            File.WriteAllText("Tr2namesDlc.json", JsonSerializer.Serialize(tr2Name, new JsonSerializerOptions() { WriteIndented = true, Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping }));


            Console.WriteLine("Unpack finished ,pres any key");
            Console.ReadKey();

        }
        static void Repack()
        {
            long pointOffset = 0x50500000;
            ResNames resNames = JsonSerializer.Deserialize<ResNames>(File.ReadAllText("Resnames.json"));


            FileStream fileStream = new FileStream("package.rdp", FileMode.Open, FileAccess.ReadWrite);
            BW package = new BW(fileStream);

            for (int i = resNames.Names.Count - 1; i >= 0; i--)
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
            ResNames resNames = JsonSerializer.Deserialize<ResNames>(File.ReadAllText("ResnamesDlc.json"));
            FileStream fileStream = new FileStream(".\\" + bigEdat, FileMode.Open, FileAccess.ReadWrite);
            BW package = new BW(fileStream);

            for (int i = resNames.Names.Count - 1; i >= 0; i--)
            {
                PresRepack pres = new PresRepack(resNames.Names[i], true);
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
           // Tr2Encoder t = new Tr2Encoder("C:\\Users\\ThinkPad\\Documents\\GitHub\\GEBCS\\system\\god_menu\\god_m001_Title\\muji\\article.tr2");
           
            string mode = "";
            Console.WriteLine("God Eater Burst package unpacker");
            Console.WriteLine("================================");
            if (args.Length < 1) Usage();
            mode = args[0].ToUpper();
            if (mode == "-X")
            {
                Unpack();
            }
            else if (mode == "-XDLC")
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
