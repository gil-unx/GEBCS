
using GECV_EX.TR2;
using GECV_EX_TR2_Editor_GUI;
using GIL.FUNCTION;
using Ionic.Zlib;
using System.Text.Json;

namespace GEBCS
{
    class Program
    {
        static void Unpack()
        {
            ResNames resName = new ResNames();
            Tr2Names tr2Name = new Tr2Names();
            PackageFiles packageFiles = new PackageFiles() { Files = new List<PackageContent>() { } };
            resName.Names = new List<string>();
            resName.Names.Add(".\\system.res");
            tr2Name.Names = new List<string>();
            FileStream fileStream = new FileStream("package.rdp", FileMode.Open, FileAccess.Read);
            BR package = new BR(fileStream);
            PresUnpack pres = new PresUnpack(".\\system.res");
            pres.Unpack(ref package, ref resName, ref tr2Name,ref packageFiles);
            package.Close();


            File.WriteAllText("Resnames.json", JsonSerializer.Serialize(resName, new JsonSerializerOptions() { WriteIndented = true, Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping }));
            File.WriteAllText("Tr2names.json", JsonSerializer.Serialize(tr2Name, new JsonSerializerOptions() { WriteIndented = true, Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping }));
            File.WriteAllText("PackageFiles.json", JsonSerializer.Serialize(packageFiles, new JsonSerializerOptions() { WriteIndented = true, Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping }));

            Console.WriteLine("Unpack finished ,pres any key");
            Console.ReadKey();

        }
        static void UnpackDlc(string smallEdat, string bigEdat)
        {
            ResNames resName = new ResNames();
            Tr2Names tr2Name = new Tr2Names();
            PackageFiles packageFiles = new PackageFiles() { Files = new List<PackageContent>() { } };
            resName.Names = new List<string>();
            resName.Names.Add(".\\" + smallEdat);
            tr2Name.Names = new List<string>();
            FileStream fileStream = new FileStream(".\\" + bigEdat, FileMode.Open, FileAccess.Read);
            BR package = new BR(fileStream);
            PresUnpack pres = new PresUnpack(".\\" + smallEdat, true);
            pres.Unpack(ref package, ref resName, ref tr2Name,ref packageFiles);
            package.Close();


            File.WriteAllText("ResnamesDlc.json", JsonSerializer.Serialize(resName, new JsonSerializerOptions() { WriteIndented = true, Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping }));
            File.WriteAllText("Tr2namesDlc.json", JsonSerializer.Serialize(tr2Name, new JsonSerializerOptions() { WriteIndented = true, Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping }));
            File.WriteAllText(Path.ChangeExtension(bigEdat,"json"), JsonSerializer.Serialize(packageFiles, new JsonSerializerOptions() { WriteIndented = true, Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping }));

            Console.WriteLine("Unpack finished ,pres any key");
            Console.ReadKey();

        }
        static void Repack(CompressionLevel level = CompressionLevel.Default)
        {
            long pointOffset = 0x50500000;
            ResNames resNames = JsonSerializer.Deserialize<ResNames>(File.ReadAllText("Resnames.json"));
            PackageFiles packageFiles = JsonSerializer.Deserialize<PackageFiles>(File.ReadAllText("PackageFiles.json"));

            Dictionary<int, int> ptSeekSame = new Dictionary<int, int>();
            Dictionary<string, int> dictPackageFiles = new Dictionary<string, int>();
            foreach (var item in packageFiles.Files)
            {
                dictPackageFiles.Add(item.Name, item.Offset);
            }
            FileStream fileStream = new FileStream("package.rdp", FileMode.Create, FileAccess.Write);
            BW package = new BW(fileStream);

            for (int i = resNames.Names.Count - 1; i >= 0; i--)
            {
                PresRepack pres = new PresRepack(resNames.Names[i]);
                pres.RepackF(ref package, ref ptSeekSame, ref dictPackageFiles, level);

            }
            package.Close();

            Console.WriteLine("Repack finished ,pres any key");
            Console.ReadKey();

        }
        static void RepackDlc(string smallEdat, string bigEdat)
        {
            long pointOffset = 0x0;
            Dictionary<int, int> ptSeekSame = new Dictionary<int, int>();

            ResNames resNames = JsonSerializer.Deserialize<ResNames>(File.ReadAllText("ResnamesDlc.json"));
            PackageFiles packageFiles = JsonSerializer.Deserialize<PackageFiles>(File.ReadAllText(Path.ChangeExtension(bigEdat,"json")));
            Dictionary<string, int> dictPackageFiles = new Dictionary<string, int>();
            foreach (var item in packageFiles.Files)
            {
                dictPackageFiles.Add(item.Name, item.Offset);
            }
            FileStream fileStream = new FileStream(".\\" + bigEdat, FileMode.Open, FileAccess.ReadWrite);
            BW package = new BW(fileStream);

            for (int i = resNames.Names.Count - 1; i >= 0; i--)
            {
                PresRepack pres = new PresRepack(resNames.Names[i], true);
                pres.RepackF(ref package,ref ptSeekSame,ref dictPackageFiles);

            }
            package.Close();

            Console.WriteLine("Repack finished ,pres any key");
            Console.ReadKey();

        }
        static void Usage()
        {
            Console.WriteLine("Unpack: GEBCS.exe -x ");
            Console.WriteLine("Repack: GEBCS.exe -c ");
            Console.WriteLine("Repack: GEBCS.exe -c CompressionLevel");
            Console.WriteLine("CompressionLevel: 0-9");
            Console.WriteLine("default: 5");
            Console.WriteLine("---------------------------");
            Console.WriteLine("DLC Unpack: GEBCS.exe -xdlc [small].edat [big].edat");
            Console.WriteLine("DLC Repack: GEBCS.exe -cdlc  [small].edat [big].edat");
            Console.WriteLine("---------------------------");
            Console.WriteLine("TR2 Decode to Excel: GEBCS.exe -xtr2 *.tr2");
            Console.WriteLine("TR2 Encode from Excel: GEBCS.exe -ctr2  *.tr2");
            Console.WriteLine("---------------------------");
            Console.WriteLine("TR2 Decode to Excel from json: GEBCS.exe -xtr2 *.tr2 Tr2names.json");
            Console.WriteLine("TR2 Encode from Excel from json: GEBCS.exe -ctr2  *.tr2 Trnames.json");
            Console.WriteLine("---------------------------");
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
            else if (mode == "-XDLC")
            {
                UnpackDlc(args[1], args[2]);
            }
            else if (mode == "-C")
            {
                CompressionLevel level;
                if (args.Length ==2)
                {
                    switch (args[1])
                    {
                        case "0":
                            level = CompressionLevel.Level0;
                            break;
                        case "1":
                            level = CompressionLevel.Level1;
                            break;
                        case "2":
                            level = CompressionLevel.Level2;
                            break;
                        case "3":
                            level = CompressionLevel.Level3;
                            break;
                        case "4":
                            level = CompressionLevel.Level4;
                            break;
                        case "5":
                            level = CompressionLevel.Level5;
                            break;
                        case "6":
                            level = CompressionLevel.Level6;
                            break;
                        case "7":
                            level = CompressionLevel.Level7;
                            break;
                        case "8":
                            level = CompressionLevel.Level8;
                            break;
                        case "9":
                            level = CompressionLevel.Level9;
                            break;
                        default:
                            level = CompressionLevel.Default;
                            break;
                    }
                    Repack(level);
                }
                else
                {
                    Repack();
                }
               
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
