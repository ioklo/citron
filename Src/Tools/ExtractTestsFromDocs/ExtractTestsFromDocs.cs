using System.Collections.Immutable;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace Citron;

record struct TestInfo(string TestName, string Code, string Result); // 현재는 string Result이지만, 에러코드가 들어가면 달라진다
record struct MDFileInfo(string FileName, int TotalCount, ImmutableArray<TestInfo> Tests, int NotTestCount, int TodoCount);

class ExtractTestsFromDocs
{
    // annotation
    static string notTestAnnotation = @"(?<NotTest>%%NOTTEST%%)[\r\n]+";
    static string todoAnnotation = @"(?<Todo>%%TODO%%)[\r\n]+";
    static string testAnnotation = @"(?<Test>%%TEST\((?<TestName>[^,]+), (?<TestResult>[^\)]*)\)%%)[\r\n]+";
    static Regex regex = new Regex(@$"(({notTestAnnotation})|({todoAnnotation})|({testAnnotation}))?```(\w+)?\s+(?<TestCode>([^`]+|`[^`]|``[^`])*)```");

    static void DeleteFile(string path)
    {
        if (path.Length != 0 && !path.Contains("*") && path != "\\")
            File.Delete(path);
    }

    static void WriteFileIfChanged(string path, string text)
    {
        // 정규화
        text = text.ReplaceLineEndings("\r\n");

        if (File.Exists(path))
        {
            var prevText = File.ReadAllText(path);
            if (prevText == text) return;
        }

        File.WriteAllText(path, text);
    }

    void MakeCTFiles(string ctDirPath, IReadOnlyList<MDFileInfo> fileInfos)
    {
        // 미리 만들어 두었던 CT파일들을 모아본다
        HashSet<string> prevExtractedCTFiles = new HashSet<string>();

        // ct를 생성할 예정
        foreach (var prevExtractedTestCode in Directory.GetFiles(ctDirPath, "*.ct", SearchOption.TopDirectoryOnly))
        {
            var relPath = Path.GetRelativePath(ctDirPath, prevExtractedTestCode);
            prevExtractedCTFiles.Add(relPath);
        }

        if (!Directory.Exists(ctDirPath))
            Directory.CreateDirectory(ctDirPath);

        foreach (var fileInfo in fileInfos)
        {
            foreach (var testInfo in fileInfo.Tests)
            {
                var fileName = $"{fileInfo.FileName}_{testInfo.TestName}.ct";

                // TODO: 디렉토리까지도 포함
                prevExtractedCTFiles.Remove(fileName);
                var testCodePath = Path.Combine(ctDirPath, fileName);

                WriteFileIfChanged(testCodePath, testInfo.Code);
            }
        }

        if (prevExtractedCTFiles.Count != 0)
        {
            foreach (var prevExtractedCTFile in prevExtractedCTFiles)
            {
                Console.WriteLine($"deleting orphan test codes {prevExtractedCTFile}");
                DeleteFile(Path.Combine(ctDirPath, prevExtractedCTFile));
            }
        }
    }

    string ConvertSpaceToUnderscore(string text)
    {
        return text.Replace(" ", "_");
    }

    string ConvertUnderscoreToCamelCase(string text)
    {
        return text.Replace("_", "");
    }

    // @""으로 변경
    string EncodeCSLiteral(string text)
    {
        // 1. crlf로 통일
        // 2. text에서 "가 나오면 모두 ""로 바꾼다

        return "@\"" + text.Replace("\"", "\"\"") + "\"";
    }

    void MakeTestCSs(string testProjGenPath, IReadOnlyList<MDFileInfo> fileInfos)
    {
        // 미리 만들어 두었던 CT파일들을 모아본다
        HashSet<string> orphanCSFiles = new HashSet<string>();

        if (Directory.Exists(testProjGenPath))
        {
            var attr = File.GetAttributes(testProjGenPath);
            if (!attr.HasFlag(FileAttributes.Directory))
                return;

            // g.cs를 생성할 예정
            foreach (var prevCSFile in Directory.GetFiles(testProjGenPath, "*.g.cs", SearchOption.TopDirectoryOnly))
            {
                var relPath = Path.GetRelativePath(testProjGenPath, prevCSFile);
                orphanCSFiles.Add(relPath);
            }
        }

        if (!Directory.Exists(testProjGenPath))
            Directory.CreateDirectory(testProjGenPath);

        foreach(var fileInfo in fileInfos)
        {
            if (fileInfo.Tests.Length == 0) continue;

            // 파일이름 결정            
            var csFileName = $"Tests_{fileInfo.FileName}.g.cs";
            var csFilePath = Path.Combine(testProjGenPath, csFileName);

            var testClassName = fileInfo.FileName;
            var testsBuilder = new StringBuilder();

            orphanCSFiles.Remove(csFileName);

            foreach (var testInfo in fileInfo.Tests)
            {
                var testCode = $@"
    [Fact]
    public Task Test_{ConvertSpaceToUnderscore(testInfo.TestName)}()
    {{
        var input = {EncodeCSLiteral(testInfo.Code)};
        var expected = {EncodeCSLiteral(testInfo.Result)};

        return IR0EvaluationTester.TestAsync(input, expected).AsTask();
    }}";
                if (testsBuilder.Length != 0)
                    testsBuilder.AppendLine();

                testsBuilder.Append(testCode);
            }

            var template = $@"using Xunit;
using System.Threading.Tasks;

namespace EvalTest;

public class Tests_{ConvertSpaceToUnderscore(testClassName)}
{{
{testsBuilder}
}}
";
            WriteFileIfChanged(csFilePath, template);
        }

        foreach(var orphanCSFile in orphanCSFiles)
        {
            var orphanFilePath = Path.Combine(testProjGenPath, orphanCSFile);

            Console.WriteLine($"deleting orphan test codes {orphanFilePath}");
            File.Delete(orphanFilePath);
        }
    }

    // mdFilePath: "...\Docs\List_Iterator_Expression.md"
    // ListIterExpressionTests.g.cs
    // 
    MDFileInfo MakeMDFileInfo(string mdFilePath)
    {
        string text = File.ReadAllText(mdFilePath);
        string mdFileName = Path.GetFileNameWithoutExtension(mdFilePath);
        
        int totalCount = 0;
        int handledCount = 0;
        int notTestCount = 0;
        int todoCount = 0;
        var testInfosBuilder = ImmutableArray.CreateBuilder<TestInfo>();
        
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
                    var testCode = match.Groups["TestCode"].Value;
                    var testResult = match.Groups["TestResult"].Value;

                    var testInfo = new TestInfo(testName, testCode, testResult);
                    testInfosBuilder.Add(testInfo);
                }
            }

            match = regex.Match(text, match.Index + match.Length);
        }

        return new MDFileInfo(mdFileName, totalCount, testInfosBuilder.ToImmutable(), notTestCount, todoCount);

        // Console.WriteLine($"{mdFilePath}: TEST: {handledCount}, NOTTEST: {notTestCount}, TODO: {todoCount}, TOTAL: {totalCount}, {totalCount == handledCount + notTestCount + todoCount}");
    }

    void Run(string docsDirPath, string testDirPath)
    {
        docsDirPath = Path.GetFullPath(docsDirPath);
        testDirPath = Path.GetFullPath(testDirPath);

        Console.WriteLine($"Docs Path: {docsDirPath}");
        Console.WriteLine($"Test Source Path: {testDirPath}");

        var mdFileInfos = new List<MDFileInfo>();
        foreach (var file in Directory.GetFiles(docsDirPath, "*.md", SearchOption.AllDirectories))
        {
            var mdFileInfo = MakeMDFileInfo(file);
            mdFileInfos.Add(mdFileInfo);
        }

        // 1. 이걸로 ct파일을 만들수도 있고,
        // MakeCTFiles(testDirPath, mdFileInfos);
        MakeTestCSs(testDirPath, mdFileInfos);
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

