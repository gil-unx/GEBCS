using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;

namespace GEPSVRIFFWORKER
{
    internal class Program
    {

        static string DATA_HEADER = "RIFF";
        static DirectoryInfo dir;

        static void Main(string[] args)
        {



            Console.WriteLine("CODE EATER nus3bank 提取器 BY 兰德里奥（HaoJun0823）");
            Console.WriteLine("https://blog.haojun0823.xyz/");
            Console.WriteLine("https://github.com/HaoJun0823/GECV");
            
            


            if (args.Length != 0)
            {
                dir = new DirectoryInfo(args[0]);


                var files = dir.GetFiles("*.nus3bank",SearchOption.AllDirectories);

                Parallel.ForEach<FileInfo>(files, file => {


                    var wav_data = GetAudio(file);


                    if (wav_data.Length != 0)
                    {

                        var file_name = Path.GetDirectoryName(file.FullName) + "\\" + Path.GetFileNameWithoutExtension(file.FullName) + ".at9";
                        Console.WriteLine($"输出文件:{file_name}，大小:{wav_data.Length}");


                        File.WriteAllBytes(file_name, wav_data);

                    }
                
                
                });




            }
            else
            {
                Console.WriteLine($"你需要输入一个文件夹作为第一个参数。");
            }

            Console.WriteLine("完成！按任意键结束！");
            Console.ReadKey();


        }




        public static byte[] GetAudio(FileInfo file)
        {


            List<byte> audio = new List<byte>();

            

            using(FileStream fs = file.OpenRead())
            {
                using(BinaryReader br = new BinaryReader(fs))
                {

                    int count = 0;

                    for(var i = 0; i < br.BaseStream.Length; i++)
                    {
                        char c = Convert.ToChar(br.ReadByte());

                        if (c == DATA_HEADER[count])
                        {
                            //Console.WriteLine($"找到了{DATA_HEADER[count]}");
                            count++;
                        }
                        else
                        {
                            count = 0;
                        }



                        if (count + 1 == DATA_HEADER.Length)
                        {

                            Console.WriteLine($"找到了{file.FullName}的标签{DATA_HEADER}。");
                            br.BaseStream.Seek(-3, SeekOrigin.Current);

                            return br.ReadBytes(Convert.ToInt32(br.BaseStream.Length - br.BaseStream.Position));
                            


                        }



                    }






                }
            }


            Console.WriteLine($"没有找到{file.FullName}的标签{DATA_HEADER}！");

            return audio.ToArray();


        }


        



    }
}
