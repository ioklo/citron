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

    HashSet<string> prevExtractedTestCodes = new HashSet<string>();

    // mdFilePath: "...\Docs\List_Iterator_Expression.md"
    // ListIterExpressionTests.g.cs
    // 
    void HandleFile(string mdFilePath, string testDirPath)
    {
        string text = File.ReadAllText(mdFilePath);
        string mdFileName = Path.GetFileNameWithoutExtension(mdFilePath);
        
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

                    if (!Directory.Exists(testDirPath))
                        Directory.CreateDirectory(testDirPath);

                    var testCode = match.Groups["TestCode"].Value;
                    var fileName = $"{mdFileName}_{testName}.ct";
                    var testCodePath = Path.Combine(testDirPath, fileName);

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

        Console.WriteLine($"{mdFilePath}: TEST: {handledCount}, NOTTEST: {notTestCount}, TODO: {todoCount}, TOTAL: {totalCount}, {totalCount == handledCount + notTestCount + todoCount}");
    }

    void Run(string docsDirPath, string testDirPath)
    {
        docsDirPath = Path.GetFullPath(docsDirPath);
        testDirPath = Path.GetFullPath(testDirPath);

        Console.WriteLine($"Docs Path: {docsDirPath}");
        Console.WriteLine($"Test Source Path: {testDirPath}");

        // g.cs를 생성할 예정
        foreach (var prevExtractedTestCode in Directory.GetFiles(testDirPath, "*.g.cs", SearchOption.TopDirectoryOnly))
        {
            var relPath = Path.GetRelativePath(testDirPath, prevExtractedTestCode);
            prevExtractedTestCodes.Add(relPath);
        }

        foreach (var file in Directory.GetFiles(docsDirPath, "*.md", SearchOption.AllDirectories))
        {
            HandleFile(file, testDirPath);
        }

        if (prevExtractedTestCodes.Count != 0)
        {
            foreach (var prevExtractedTestCode in prevExtractedTestCodes)
            {
                if (prevExtractedTestCode.Length != 0 && !prevExtractedTestCode.Contains("*") && prevExtractedTestCode != "\\")
                {
                    Console.WriteLine($"deleting useless test codes {prevExtractedTestCode}");
                    File.Delete(Path.Combine(testDirPath, prevExtractedTestCode));
                }
            }
        }
    }

    static void Main(string[] args)
    {
        // docs디렉토리와 EvalTest 디렉토리를 인자로 받는다
        if (args.Length < 2)
        {
            Console.WriteLine($"{Assembly.GetEntryAssembly()?.GetName().Name} [Docs Path] [Test Source Path]");
            return;
        }

        var instance = new ExtractTestsFromDocs();
        instance.Run(args[0], args[1]);
    }
}

