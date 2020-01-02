using Gum;
using Gum.CompileTime;
using Gum.Infra;
using Gum.Runtime;
using Gum.StaticAnalysis;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gum.Runtime
{
    public class DefaultApplication
    {
        Parser parser;
        Evaluator evaluator;

        public DefaultApplication(ICommandProvider commandProvider)
        {
            Lexer lexer = new Lexer();
            parser = new Parser(lexer);

            var typeSkeletonCollector = new TypeSkeletonCollector();
            var typeExpEvaluator = new TypeExpEvaluator(typeSkeletonCollector);
            var typeAndFuncBuilder = new ModuleInfoBuilder(typeExpEvaluator);

            var capturer = new Capturer();
            var analyzer = new Analyzer(typeAndFuncBuilder, capturer);
            
            evaluator = new Evaluator(analyzer, commandProvider);        
        }
        
        public async ValueTask<int?> RunAsync(
            string moduleName, string input, IRuntimeModule runtimeModule, ImmutableArray<IModule> modulesExceptRuntimeModule, IErrorCollector errorCollector) // 레퍼런스를 포함
        {
            var moduleInfos = new List<IModuleInfo>(modulesExceptRuntimeModule.Length + 1);

            moduleInfos.Add(runtimeModule);
            foreach(var module in modulesExceptRuntimeModule)
                moduleInfos.Add(module);

            // 파싱 ParserContext -> Script
            var script = await parser.ParseScriptAsync(input);
            if (script == null)
                return null;
            
            return await evaluator.EvaluateScriptAsync(moduleName, script, runtimeModule, moduleInfos, errorCollector);
        }
    }

}
