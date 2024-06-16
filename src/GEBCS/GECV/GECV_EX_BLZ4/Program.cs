using GECV_EX.Utils;

namespace GECV_EX_BLZ4
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("GECV EX BLZ BY RANDERION(HAOJUN0823)");
            Console.WriteLine("https://blog.haojun0823.xyz/");
            Console.WriteLine("https://github.com/HaoJun0823/GECV");


            if(args.Length <3)
            {

                Console.WriteLine($"Your Need 3 Args:");
                Console.WriteLine("(Unpack):1.{unpack} 2.{blz4 file} 3.{The name of the unzipped file.}");
                Console.WriteLine("(Pack):1.{pack} 2.{original file} 3.{The name of the new blz4 file.}");
                Console.WriteLine("Extra:If You Need BLZ2 Unpack: 4.blz2");
                Console.WriteLine("If You Need BLZ2 Pack:See blz2_compress.exe");

            }
            else
            {

                if(args.Length >=4 && args[3].ToLower().Equals("blz2")) {

                    Console.ForegroundColor = ConsoleColor.Green;

                    if (args[0].ToLower().Equals("unpack"))
                    {
                        Unpack2(args[1], args[2]);
                        return;
                    }
                    if (args[0].ToLower().Equals("pack"))
                    {
                        Console.WriteLine("Warning:This Function Has Been Deprecated Beacause Game Cannot Read This Version.");
                        Console.WriteLine("Try Python Version (blz2_compress.exe) Better Than Microsoft Fool ZLIB Library.");
                        //pack2(args[1], args[2]);
                        return;
                    }
                    Console.WriteLine($"What is {args[0]}? input (unpack/pack) please.");

                    

                    return;
                }

                Console.ForegroundColor = ConsoleColor.Blue;

                if (args[0].ToLower().Equals("unpack"))
                {
                    Unpack(args[1], args[2]);
                    return;
                }
                if (args[0].ToLower().Equals("pack"))
                {
                    pack(args[1], args[2]);

                    return;
                }

                Console.WriteLine($"What is {args[0]}? input (unpack/pack) please.");

                return;

            }




        }



        public static void Unpack(string blz4,string file)
        {

            byte[] blz4_data = File.ReadAllBytes(blz4);
            byte[] unpack_data;


            unpack_data = BLZ4Utils.UnpackBLZ4Data(blz4_data);


            Console.WriteLine($"Unpack:{blz4}=>{file}.");
            

            File.WriteAllBytes(file, unpack_data);


        }

        public static void pack(string file, string blz4)
        {

            byte[] file_data = File.ReadAllBytes(file);
            byte[] pack_data;


            pack_data = BLZ4Utils.PackBLZ4Data(file_data);


            Console.WriteLine($"Pack:{file}=>{blz4}.");


            File.WriteAllBytes(blz4,pack_data);


        }

        public static void Unpack2(string blz, string file)
        {

            byte[] blz2_data = File.ReadAllBytes(blz);
            byte[] unpack_data;


            unpack_data = BLZ2Utils.UnpackBLZ2Data(blz2_data);


            Console.WriteLine($"Unpack:{blz}=>{file}.");


            File.WriteAllBytes(file, unpack_data);


        }

        public static void pack2(string file, string blz2)
        {

            byte[] file_data = File.ReadAllBytes(file);
            byte[] pack_data;


            pack_data = BLZ2Utils.PackBLZ2Data(file_data);


            Console.WriteLine($"Pack:{file}=>{blz2}.");


            File.WriteAllBytes(blz2, pack_data);


        }

    }
}
