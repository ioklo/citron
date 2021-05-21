using Gum;
using Gum.Infra;
using Gum.IR0Evaluator;
using Gum.IR0Translator;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Runner
{
    class DemoErrorCollector : IErrorCollector
    {
        List<IError> errors = new List<IError>();

        public DemoErrorCollector() { }

        DemoErrorCollector(DemoErrorCollector other, CloneContext cloneContext)
        {
            this.errors = new List<IError>(other.errors);
        }

        public bool HasError => errors.Count != 0;

        public void Add(IError error)
        {
            errors.Add(error);
        }

        public IErrorCollector Clone(CloneContext context)
        {
            return new DemoErrorCollector(this, context);
        }

        public void Update(IErrorCollector src_errorCollector, UpdateContext updateContext)
        {
            var src = (DemoErrorCollector)src_errorCollector;
            errors.Clear();
            errors.AddRange(src.errors);
        }

        public string GetMessages()
        {
            return string.Join("\r\n", errors.Select(error => error.Message));
        }
    }

    class DemoCommandProvider : ICommandProvider
    {
        public async Task ExecuteAsync(string text)
        {
            try
            {
                text = text.Trim();

                if (text.StartsWith("echo "))
                {
                    Console.WriteLine(text.Substring(5).Replace("\\n", "\n"));
                }
                else if (text.StartsWith("sleep "))
                {
                    var d = double.Parse(text.Substring(6));
                    await Task.Delay((int)(1000 * d));
                }
                else
                {
                    Console.WriteLine($"알 수 없는 명령어 입니다: {text}\n");
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

            var testErrorCollector = new DemoErrorCollector();
            var rscript = Translator.Translate("TestModule", default, scriptResult.Elem, testErrorCollector);
            if (rscript == null)
            {
                Console.WriteLine("에러 (컴파일 실패)\n");
                Console.WriteLine(testErrorCollector.GetMessages());
                return 1;
            }

            var commandProvider = new DemoCommandProvider();
            var evaluator = new Evaluator(commandProvider, rscript);
            var retValue = await evaluator.EvalAsync();

            return retValue;
        }
    }
}
