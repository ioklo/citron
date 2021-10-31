using Gum;
using Gum.Infra;
using Gum.IR0Evaluator;
using Gum.IR0Translator;
using Gum.Log;
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

using ILogger = Gum.Log.ILogger;

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

            var testLogger = new DemoLogger();
            var rscript = Translator.Translate("TestModule", default, scriptResult.Elem, testLogger);
            if (rscript == null)
            {
                await WriteAsync("에러 (컴파일 실패)\n");
                await WriteAsync(testLogger.GetMessages());
                return false;
            }

            var commandProvider = new DemoCommandProvider();
            var retValue = await Evaluator.EvalAsync(default, commandProvider, rscript);
            await WriteAsync($"\n리턴 값: {retValue}");

            return true;
        }
    }
}
