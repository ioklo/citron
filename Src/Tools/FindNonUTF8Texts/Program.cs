namespace Citron;

class Program
{
    static void Main(string[] args)
    {
        var buffer = new byte[3];

        var curDirPath = Directory.GetCurrentDirectory();
        foreach(var filePath in Directory.EnumerateFiles(curDirPath, "*", System.IO.SearchOption.AllDirectories))
        {
            var relFilePath = Path.GetRelativePath(curDirPath, filePath);

            if (relFilePath.Contains("obj\\", System.StringComparison.CurrentCultureIgnoreCase))
                continue;

            if (filePath.EndsWith("g.cs"))
                continue;

            if (filePath.Contains("PretuneGenerated"))
                continue;

            var extension = Path.GetExtension(filePath);
            if (extension != ".cs") continue;

            // Console.WriteLine(filePath);

            using (var stream = File.OpenRead(filePath))
            {
                int readBytes = stream.Read(buffer, 0, 3);
                if (readBytes < 3 || !(buffer[0] == 0xEF && buffer[1] == 0xBB && buffer[2] == 0xBF))
                    Console.WriteLine($"{filePath} doesn't have utf-8 bom");
            }
        }

        Console.WriteLine("Completed");
    }
}