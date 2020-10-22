using Gum;
using Gum.Infra;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gum.IR0.Runtime
{
    // TODO: Core로 옮기던가, Test로 옮기던가
    //public class DefaultApplication
    //{
    //    Parser parser;
    //    Evaluator evaluator;

    //    public DefaultApplication(ICommandProvider commandProvider)
    //    {
    //        Lexer lexer = new Lexer();
    //        parser = new Parser(lexer);

    //        var typeSkeletonCollector = new TypeSkeletonCollector();
    //        var typeExpEvaluator = new TypeExpEvaluator(typeSkeletonCollector);
    //        var typeAndFuncBuilder = new ModuleInfoBuilder(typeExpEvaluator);

    //        var capturer = new Capturer();
    //        var analyzer = new Analyzer(typeAndFuncBuilder, capturer);
            
    //        evaluator = new Evaluator(analyzer, commandProvider);        
    //    }
        
    //    public async ValueTask<int?> RunAsync(
    //        string moduleName, string input, IRuntimeModule runtimeModule, IEnumerable<IModule> modulesExceptRuntimeModule, IErrorCollector errorCollector) // 레퍼런스를 포함
    //    {
    //        IEnumerable<IModuleInfo> RuntimeModuleInfos() { yield return runtimeModule; }
    //        var moduleInfos = RuntimeModuleInfos().Concat(modulesExceptRuntimeModule);

    //        // 파싱 ParserContext -> Script
    //        var script = await parser.ParseScriptAsync(input);
    //        if (script == null)
    //            return null;
            
    //        return await evaluator.EvaluateScriptAsync(moduleName, script, runtimeModule, moduleInfos, errorCollector);
    //    }
    //}

}
