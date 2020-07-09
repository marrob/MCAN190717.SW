using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.IO.Ports;
using System.CodeDom;
using Microsoft.VisualBasic;

namespace MCAN190717
{
    class Program
    {
        static void Main(string[] args)
        {
            /*
             * -COM port - ALTMUX or HVMUX -sample - WaitMs -Log File Dir
             */

            try
            {
                string port = (string)ArgFormatter(args[0], "string", 0);
                string mux = (string)ArgFormatter(args[1], "string", 1);
                int repeat = (int)ArgFormatter(args[2], "int", 2);
                int interval = (int)ArgFormatter(args[3], "int", 3);
                string path = (string)ArgFormatter(args[4], "path", 4);
                new App(port, mux, repeat, interval, path);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error:" + ex.Message);
            }
            Console.ReadLine();
        }

        static object ArgFormatter(string arg,  string type, int index)
        {
            if (arg[0] != '-')
                throw new ArgumentException("Az arg formátuma nem jó", "Indexe:" + index);
            arg = arg.Substring(1);

            if (type == "string")
                return arg;
            else if (type == "int")
            {
                int temp;
                if (!int.TryParse(arg, out temp))
                    throw new ArgumentException("Az arg nem egész szám.", "Indexe:" + index);
                return temp;
            }
            else if (type == "path")
            {
                arg.Replace('"',' ');
                return arg;
            }
            return string.Empty;
        }
    }

    class App
    {
        SerialPort sp;
        public App(
                string port,
                string mux,
                int samples,
                int waitMs,
                string path
            )
        {

            Console.WriteLine("port:" + port +", mux:" + mux + ", samples:" + samples.ToString() + ", waitMs:" + waitMs.ToString() +", path:"  + path);
            sp = new SerialPort(port);
            sp.Open();
            string[] lines = new string[samples];

            sp.WriteLine(mux);
            if (sp.ReadLine() != "OK")
                throw new IOException("Az eszköz nem válaszol");


            for (int i = 0; i < samples; i++)
            {
                sp.WriteLine("VCELLS?");
                string line = sp.ReadLine();
                var values =  line.Split(new[] { ',' });
                if (values.Length != 12)
                    Console.WriteLine("Error: Nem jött meg az" + i + ". lekérdezésre az összes válasz");
                else
                {
                    Console.WriteLine(i.ToString() +": " + line);
                    lines[i] = line.Replace(',', ';');
                }
                System.Threading.Thread.Sleep(waitMs);
            }

            Console.WriteLine("Mission complete");
            FileWrite(lines, path + "RBMS-MASTER-PLATFORM-VCEL-LOG" + " mux_" + mux +" samples_"+ samples.ToString() + " wait_"+ waitMs.ToString() + " " + GetFileName() + ".csv");
            sp.Close();
        }
        void FileWrite(string[] lines,  string path )
        {
            using (StreamWriter sw = new StreamWriter(path, false))
            {
                for (int i = 0; i < lines.Length; i++)
                {
                    sw.WriteLine(lines[i]);
                }
            }
        }

        public string GetFileName()
        {
            return DateTime.Now.ToString("yyMMdd HHmmss");
        }
    }
 }
