using Gum;
using Gum.Infra;
using Gum.IR0Evaluator;
using Gum.IR0Translator;
using Gum.Log;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Runner
{
    class DemoLogger : ILogger
    {
        List<ILog> logs = new List<ILog>();

        public DemoLogger() { }

        DemoLogger(DemoLogger other, CloneContext cloneContext)
        {
            this.logs = new List<ILog>(other.logs);
        }

        public bool HasError => logs.Count != 0;

        public void Add(ILog error)
        {
            logs.Add(error);
        }

        public ILogger Clone(CloneContext context)
        {
            return new DemoLogger(this, context);
        }

        public void Update(ILogger src_logger, UpdateContext updateContext)
        {
            var src = (DemoLogger)src_logger;
            logs.Clear();
            logs.AddRange(src.logs);
        }

        public string GetMessages()
        {
            return string.Join("\r\n", logs.Select(error => error.Message));
        }
    }

    class DemoCommandProvider : ICommandProvider
    {
        public async Task ExecuteAsync(string text)
        {
            try
            {
                text = text.Trim();

                /*if (text.StartsWith("echo "))
                {
                    Console.WriteLine(text.Substring(5).Replace("\\n", "\n"));
                }
                else */if (text.StartsWith("sleep "))
                {
                    var d = double.Parse(text.Substring(6));
                    await Task.Delay((int)(1000 * d));
                }
                else
                {
                    // Console.WriteLine($"알 수 없는 명령어 입니다: {text}\n");
                    Console.Write(text);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString() + "\n");
            }
        }
    }

    class Program
    {
        static async Task<int> Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("Runner file.qs");
                return 0;
            }

            var lexer = new Lexer();
            var parser = new Parser(lexer);

            var buffer = new Gum.Buffer(new StreamReader(args[0]));
            var bufferPos = await buffer.MakePosition().NextAsync();
            var lexerContext = LexerContext.Make(bufferPos);
            var parserContext = ParserContext.Make(lexerContext);

            var scriptResult = await parser.ParseScriptAsync(parserContext);
            if (!scriptResult.HasValue)
            {
                Console.WriteLine("에러 (파싱 실패)\n");
                return 1;
            }

            var testLogger = new DemoLogger();
            var rscript = Translator.Translate("TestModule", default, scriptResult.Elem, testLogger);
            if (rscript == null)
            {
                Console.WriteLine("에러 (컴파일 실패)\n");
                Console.WriteLine(testLogger.GetMessages());
                return 1;
            }

            var commandProvider = new DemoCommandProvider();
            var retValue = await Evaluator.EvalAsync(default, commandProvider, rscript);

            return retValue;
        }
    }
}
