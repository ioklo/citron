using System;
using System.Collections.Generic;
using System.Text.Json;

namespace PrepareScratchPad
{
    class Program
    {
        static void Main(string[] args)
        {
            var files = new List<string>();
            files.Add("hello");
            files.Add("world");
            var text = JsonSerializer.Serialize(files);

            Console.WriteLine(text);
        }
    }
}
