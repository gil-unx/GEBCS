using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace ProcessTester
{
    internal class Program
    {




        static void Main(string[] args)
        {

            Process p = new Process();

            p.StartInfo.FileName = Environment.CurrentDirectory +"\\GE2RB.exe";

            p.StartInfo.WorkingDirectory = Environment.CurrentDirectory + ".\\Chinese";


            p.Start();

        }
    }
}
