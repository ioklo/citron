using Gum.Infra;
using Gum.Runtime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Gum.Runtime
{
    class TestCmdProvider : ICommandProvider
    {
        public bool Error = false;
        public string Output { get => sb.ToString(); }
        StringBuilder sb = new StringBuilder();

        public async Task ExecuteAsync(string cmdText)
        {
            if (cmdText == "yield")
            {
                // TODO: 좋은 방법이 있으면 교체한다
                await Task.Delay(500);
                return;
            }

            sb.Append(cmdText);
        }
    }

    class TestErrorCollector : IErrorCollector
    {
        List<(object, string)> messages = new List<(object, string)>();

        public bool HasError => messages.Count != 0;

        public void Add(object obj, string msg)
        {
            messages.Add((obj, msg));
        }

        public string GetMessages()
        {
            return string.Join("\r\n", messages.Select(message => message.Item2));
        }
    }

    public class EvalTest
    {
        // [Theory]
        // [ClassData(typeof(EvalTestDataFactory))]
        public Task TestEvaluateScript(EvalTestData data)
        {
            throw new NotImplementedException();

            // TODO: 소스코드를 읽어서 잘 실행되는 지 확인하는 테스트는 다른데서 해야할 것 같다
            
            //var cmdProvider = new TestCmdProvider();
            //var app = new DefaultApplication(cmdProvider);

            //string text;
            //using(var reader = new StreamReader(data.Path))
            //{
            //    text = reader.ReadToEnd();
            //}

            //string expected;
            //if (data.OverriddenResult != null)
            //{
            //    expected = data.OverriddenResult;
            //}
            //else
            //{
            //    Assert.StartsWith("// ", text);

            //    int firstLineEnd = text.IndexOfAny(new char[] { '\r', '\n' });
            //    Assert.True(firstLineEnd != -1);

            //    expected = text.Substring(3, firstLineEnd - 3);
            //}

            //var runtimeModule = new RuntimeModule(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), Directory.GetCurrentDirectory());
            //var errorCollector = new TestErrorCollector();
            //var runResult = await app.RunAsync(Path.GetFileNameWithoutExtension(data.Path), text, runtimeModule, Enumerable.Empty<IModule>(), errorCollector);

            //Assert.True((runResult == null) == (errorCollector.HasError), "실행은 중간에 멈췄는데 에러로그가 남지 않았습니다");
            //Assert.False(errorCollector.HasError, errorCollector.GetMessages());
            //Assert.Equal(expected, cmdProvider.Output);
        }
    }
    
    public class EvalTestData : IXunitSerializable
    {
        public string Path { get; private set; }
        public string? OverriddenResult { get; private set; }

        public EvalTestData()
        {
            Path = string.Empty;
            OverriddenResult = null;
        }

        public EvalTestData(string path, string? overridenResult = null)
        {
            Path = path;
            OverriddenResult = overridenResult;
        }

        public override string ToString()
        {
            return Path;
        }

        public void Deserialize(IXunitSerializationInfo info)
        {
            Path = info.GetValue<string>("Path");
            OverriddenResult = info.GetValue<string?>("OverriddenResult");
        }

        public void Serialize(IXunitSerializationInfo info)
        {
            info.AddValue("Path", Path);
            info.AddValue("OverriddenResult", OverriddenResult);
        }
    }

    class EvalTestDataFactory : IEnumerable<object[]>
    {
        Dictionary<string, string> overriddenResults = new Dictionary<string, string>
        {
            [Path.Combine("Input", "Env", "01_Env.qs")] = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)
        };

        public IEnumerator<object[]> GetEnumerator()
        {
            var curDir = Directory.GetCurrentDirectory();

            foreach (var path in Directory.EnumerateFiles(curDir, "*.qs", SearchOption.AllDirectories))
            {
                var relPath = Path.GetRelativePath(curDir, path);

                var result = overriddenResults.GetValueOrDefault(relPath);
                yield return new object[] { new EvalTestData(relPath, result) };                
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
