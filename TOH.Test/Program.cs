using System;
using System.Collections.Generic;

namespace TOH.Test
{

    public class Test
    {
        public List<string> List { get; set; } = new List<string>();

        public Test()
        {
            if (List.Count > 0)
            {
                Console.WriteLine("List has data");
            }
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            var test = new Test() { List = new List<string>() { "1", "2" } };


            Console.WriteLine("Hello World!");
        }
    }
}
