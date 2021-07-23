using Gum;
using Gum.Infra;
using Gum.IR0Evaluator;
using Gum.IR0Translator;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace ScratchPad
{
    public class Program
    {
        static IJSRuntime jsRuntime;

        static async Task WriteAsync(string msg)
        {
            await jsRuntime.InvokeVoidAsync("writeConsole", msg);
        }

        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("#app");

            builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

            var host = builder.Build();

            jsRuntime = host.Services.GetService<IJSRuntime>();

            await host.RunAsync();
        }

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
                        await WriteAsync(text.Substring(5).Replace("\\n", "\n"));
                    }
                    else if (text.StartsWith("sleep "))
                    {
                        var d = double.Parse(text.Substring(6));
                        await Task.Delay((int)(1000 * d));
                    }
                    else
                    {
                        await WriteAsync(text + "\n");
                    }
                }
                catch (Exception e)
                {
                    await WriteAsync(e.ToString() + "\n");
                }
            }
        }


        [JSInvokable]
        public static async Task<bool> RunAsync(string input)
        {
            var lexer = new Lexer();
            var parser = new Parser(lexer);

            var buffer = new Gum.Buffer(new StringReader(input));
            var bufferPos = await buffer.MakePosition().NextAsync();
            var lexerContext = LexerContext.Make(bufferPos);
            var parserContext = ParserContext.Make(lexerContext);

            var scriptResult = await parser.ParseScriptAsync(parserContext);
            if (!scriptResult.HasValue)
            {
                await WriteAsync("에러 (파싱 실패)\n");
                return false;
            }

            var testErrorCollector = new DemoErrorCollector();
            var rscript = Translator.Translate("TestModule", default, scriptResult.Elem, testErrorCollector);
            if (rscript == null)
            {
                await WriteAsync("에러 (컴파일 실패)\n");
                await WriteAsync(testErrorCollector.GetMessages());
                return false;
            }

            var commandProvider = new DemoCommandProvider();
            var retValue = await Evaluator.EvalAsync(default, commandProvider, rscript);
            await WriteAsync($"\n리턴 값: {retValue}");

            return true;
        }
    }
}
