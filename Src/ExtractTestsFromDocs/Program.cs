using System.Reflection;
using System.Text.RegularExpressions;

namespace Citron;

class ExtractTestsFromDocs
{
    static Regex regex = new Regex(@"(%%Test\((?<TestName>[^,]+), (?<TestResult>[^\)]*)\)%%\s+)?```(\w+)?\s+(?<TestCode>[^`]*)```");

    static void HandleFile(string file, string outputDirPath)
    {
        string text = File.ReadAllText(file);

        int totalCount = 0;
        int handledCount = 0;
        
        var match = regex.Match(text, 0);
        while (match.Success)
        {
            totalCount++;

            if (match.Groups["TestName"].Success && match.Groups["TestResult"].Success)
            {
                handledCount++;
                var testName = match.Groups["TestName"].Value;

                if (!Directory.Exists(outputDirPath))
                    Directory.CreateDirectory(outputDirPath);

                var testCode = match.Groups["TestCode"].Value;
                var testCodePath = Path.Combine(outputDirPath, $"{testName}.ct");
                File.WriteAllText(testCodePath, testCode);
            }

            match = regex.Match(text, match.Index + match.Length);
        }

        Console.WriteLine($"${file}: {handledCount}/{totalCount}");
    }

    static void Main(string[] args)
    {
        string executablePath = Assembly.GetEntryAssembly()!.Location;
        string docsPath = Path.GetFullPath(Path.Combine(executablePath, "../../../../../../Docs"));
        string testCodePath = Path.GetFullPath(Path.Combine(executablePath, "../../../../../../Src/Test/EvalTest/Input/Generated"));

        Console.WriteLine(testCodePath);

        foreach (var file in Directory.GetFiles(docsPath, "*.md", SearchOption.AllDirectories))
        {
            HandleFile(file, testCodePath);
        }
    }
}

