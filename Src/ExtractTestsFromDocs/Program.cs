using System.Reflection;
using System.Text.RegularExpressions;

namespace Citron;

class ExtractTestsFromDocs
{
    // annotation

    static string notTestAnnotation = @"(?<NotTest>%%NOTTEST%%)[\r\n]+";
    static string todoAnnotation = @"(?<Todo>%%TODO%%)[\r\n]+";
    static string testAnnotation = @"(?<Test>%%TEST\((?<TestName>[^,]+), (?<TestResult>[^\)]*)\)%%)[\r\n]+";

    static Regex regex = new Regex(@$"(({notTestAnnotation})|({todoAnnotation})|({testAnnotation}))?```(\w+)?\s+(?<TestCode>([^`]+|`[^`]|``[^`])*)```");

    static HashSet<string> prevExtractedTestCodes = new HashSet<string>();

    static void HandleFile(string file, string outputDirPath)
    {
        string text = File.ReadAllText(file);

        string mdFileName = Path.GetFileNameWithoutExtension(file);
        
        int totalCount = 0;
        int handledCount = 0;
        int notTestCount = 0;
        int todoCount = 0;
        
        var match = regex.Match(text, 0);
        while (match.Success)
        {
            totalCount++;

            if (match.Groups["NotTest"].Success)
            {
                notTestCount++;
            }
            else if (match.Groups["Todo"].Success)
            {
                todoCount++;
            }
            else if (match.Groups["Test"].Success)
            {
                if (match.Groups["TestName"].Success && match.Groups["TestResult"].Success)
                {
                    handledCount++;
                    var testName = match.Groups["TestName"].Value;

                    if (!Directory.Exists(outputDirPath))
                        Directory.CreateDirectory(outputDirPath);

                    var testCode = match.Groups["TestCode"].Value;
                    var fileName = $"{mdFileName}_{testName}.ct";
                    var testCodePath = Path.Combine(outputDirPath, fileName);

                    // TODO: 디렉토리까지도 포함
                    prevExtractedTestCodes.Remove(fileName);

                    if (File.Exists(testCodePath))
                    {
                        var prevTestCode = File.ReadAllText(testCodePath);
                        if (prevTestCode != testCode)
                            File.WriteAllText(testCodePath, testCode);
                    }
                    else
                    {
                        File.WriteAllText(testCodePath, testCode);
                    }
                }
            }

            match = regex.Match(text, match.Index + match.Length);
        }

        Console.WriteLine($"{file}: TEST: {handledCount}, NOTTEST: {notTestCount}, TODO: {todoCount}, TOTAL: {totalCount}, {totalCount == handledCount + notTestCount + todoCount}");
    }

    static void Main(string[] args)
    {
        string executablePath = Assembly.GetEntryAssembly()!.Location;
        string docsPath = Path.GetFullPath(Path.Combine(executablePath, "../../../../../../Docs"));
        string testCodePath = Path.GetFullPath(Path.Combine(executablePath, "../../../../../../Src/Test/EvalTest/Input/Generated"));

        Console.WriteLine(testCodePath);

        foreach(var prevExtractedTestCode in Directory.GetFiles(testCodePath, "*.ct", SearchOption.TopDirectoryOnly))
        {
            var relPath = Path.GetRelativePath(testCodePath, prevExtractedTestCode);
            prevExtractedTestCodes.Add(relPath);
        }

        foreach (var file in Directory.GetFiles(docsPath, "*.md", SearchOption.AllDirectories))
        {
            HandleFile(file, testCodePath);
        }

        if (prevExtractedTestCodes.Count != 0)
        {   
            foreach (var prevExtractedTestCode in prevExtractedTestCodes)
            {
                if (prevExtractedTestCode.Length != 0 && !prevExtractedTestCode.Contains("*") && prevExtractedTestCode != "\\")
                {
                    Console.WriteLine($"deleting useless test codes {prevExtractedTestCode}");
                    File.Delete(Path.Combine(testCodePath, prevExtractedTestCode));
                }
            }
        }

    }
}

