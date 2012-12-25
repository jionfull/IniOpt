using System;
using System.Collections.Generic;
using System.Text;
using Lon.IO;

namespace TstLib
{
    class Program
    {
        static void Main(string[] args)
        {
            IniFile a = new IniFile("abc.ini");
            a.Load();
            foreach(string str in a.GetAllSectionName() )
            {
                Console.WriteLine(str);
                foreach(string rec in a.GetSectionItemName(str))
                {
                    Console.WriteLine(rec);
                }
            }
            a.WriteString("ghi", "3", "4");
            a.Save();
            IniFile b = new IniFile("b.ini");
            b.WriteString("g", "3", "4");
            
            b.Save();
          
          
            Console.ReadKey();
        }
    }
}
