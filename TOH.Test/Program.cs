using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace TOH.Test
{

    public class Test
    {
        public int Id { get; set; }
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

            var x = new ConcurrentDictionary<int, Test>();

            var y = x.FirstOrDefault(t => t.Value.Id == 1);


            Console.WriteLine("Hello World!");
        }
    }
}
