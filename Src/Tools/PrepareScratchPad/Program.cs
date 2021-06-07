using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace PrepareScratchPad
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 1) return;

            var baseDirPath = Path.GetFullPath(args[0]);
            Console.WriteLine($"base path: {baseDirPath}");

            var inputDirPath = Path.Combine(baseDirPath, "Src", "Test", "EvalTest", "Input");
            var outputDirPath = Path.Combine(baseDirPath, "Src", "App", "ScratchPad", "wwwroot", "demo_files"); // 조심;
            
            if (Directory.Exists(outputDirPath))
                Directory.Delete(outputDirPath, true); // 삭제, 조심;
            Directory.CreateDirectory(outputDirPath);

            var inputFileRelPaths = new List<string>();
            foreach (var filePath in Directory.EnumerateFiles(inputDirPath, "*.qs", SearchOption.AllDirectories))
            {
                var inputFileRelPath = Path.GetRelativePath(inputDirPath, filePath);
                if (inputFileRelPath.Contains("_TODO_")) continue;

                var outputFilePath = Path.Combine(outputDirPath, inputFileRelPath);

                // Copy
                Directory.CreateDirectory(Path.GetDirectoryName(outputFilePath));
                File.Copy(filePath, outputFilePath);
                inputFileRelPaths.Add(inputFileRelPath.Replace('\\', '/'));
            }
            
            var text = JsonSerializer.Serialize(inputFileRelPaths);
            var filesPath = Path.Combine(outputDirPath, "files.json");
            File.WriteAllText(filesPath, text);
        }
    }
}
