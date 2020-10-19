using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gum.CompileTime;
using Gum.Infra;
using Gum.IR0;
using Gum.Runtime;
using Gum;
using System.Diagnostics.CodeAnalysis;
using static Gum.Infra.CollectionExtensions;

namespace Gum.Runtime
{
    // 레퍼런스용 Big Step Evaluator, 
    // TODO: Small Step으로 가야하지 않을까 싶다 (yield로 실행 point 잡는거 해보면 재미있을 것 같다)
    public class Evaluator
    {
        private ExpEvaluator expEvaluator;
        private StmtEvaluator stmtEvaluator;        

        public Evaluator(ICommandProvider commandProvider)
        {
            this.expEvaluator = new ExpEvaluator(this);
            this.stmtEvaluator = new StmtEvaluator(this, commandProvider);
        }        
        
        internal ValueTask EvalStringExpAsync(StringExp command, Value result, EvalContext context)
        {
            return expEvaluator.EvalStringExpAsync(command, result, context);
        }

        public bool GetBaseType(TypeId typeId, EvalContext context, [NotNullWhen(true)] out TypeId? outBaseTypeId)
        {
            var typeInst = context.GetTypeInst(typeId);

            if (typeInst is ClassInst classInst)
            {
                var baseTypeId = classInst.GetBaseTypeId();
                if (baseTypeId != null)
                {
                    outBaseTypeId = baseTypeId;
                    return true;
                }
            }

            outBaseTypeId = null;
            return false;
        }
        
        // xType이 y타입인가 묻는 것
        public bool IsType(TypeId xTypeId, TypeId yTypeId, EvalContext context)
        {
            TypeId? curType = xTypeId;

            while (curType != null)
            {
                if (EqualityComparer<TypeId?>.Default.Equals(curType, yTypeId))
                    return true;

                if (!GetBaseType(curType.Value, context, out var baseTypeValue))
                    throw new InvalidOperationException();

                if (baseTypeValue == null)
                    break;

                curType = baseTypeValue;
            }

            return false;
        }

        public TValue AllocValue<TValue>(TypeId typeId, EvalContext context)
            where TValue : Value
        {
            return (TValue)AllocValue(typeId, context);
        }

        // typeId는 ir0 syntax의 일부분이다
        public Value AllocValue(TypeId typeId, EvalContext context)
        {
            switch(typeId.Value)
            {
                case (int)TypeId.PredefinedValue.Void:
                    return VoidValue.Instance;

                case (int)TypeId.PredefinedValue.Bool:
                    return new BoolValue();

                case (int)TypeId.PredefinedValue.Int:
                    return new IntValue();

                case (int)TypeId.PredefinedValue.String:
                    return new StringValue();

                case (int)TypeId.PredefinedValue.Enumerable:
                    return new AsyncEnumerableValue();
            }

            throw new NotImplementedException();
        }        

        public ImmutableDictionary<string, Value> MakeCaptures(ImmutableArray<CaptureInfo.Element> captureElems, EvalContext context)
        {
            var capturesBuilder = ImmutableDictionary.CreateBuilder<string, Value>();
            foreach (var captureElem in captureElems)
            {
                if (captureElem is CaptureInfo.CopyLocalElement copyElem)
                {
                    var origValue = context.GetLocalValue(copyElem.LocalVarName);
                    var value = AllocValue(copyElem.TypeId, context);
                    value.SetValue(origValue);
                    capturesBuilder.Add(copyElem.LocalVarName, value);
                }
                else
                {
                    throw new InvalidOperationException();
                }
            }

            return capturesBuilder.ToImmutable();
        }

        public async ValueTask<ImmutableDictionary<string, Value>> EvalArgumentsAsync(IEnumerable<string> paramNames, IEnumerable<ExpInfo> expInfos, EvalContext context)
        {
            var argsBuilder = ImmutableDictionary.CreateBuilder<string, Value>();

            foreach (var (paramName, expInfo) in Zip(paramNames, expInfos))
            {
                var argValue = AllocValue(expInfo.TypeId, context);
                argsBuilder.Add(paramName, argValue);

                await expEvaluator.EvalAsync(expInfo.Exp, argValue, context);
            }

            return argsBuilder.ToImmutable();
        }

        public async ValueTask EvalLambdaAsync(Lambda lambda, IEnumerable<ExpInfo> args, Value result, EvalContext context)
        {
            var argVars = await EvalArgumentsAsync(lambda.ParamNames, args, context);

            var thisValue = lambda.CapturedThis;
            var localVars = lambda.Captures.AddRange(argVars);

            await context.ExecInNewFuncFrameAsync(localVars, EvalFlowControl.None, ImmutableArray<Task>.Empty, thisValue, result, async () =>
            {
                await foreach (var _ in EvalStmtAsync(lambda.Body, context)) { }
            });
        }

        public async ValueTask EvalLocalVarDeclAsync(LocalVarDecl localVarDecl, EvalContext context)
        {
            foreach (var elem in localVarDecl.Elems)
            {
                var value = AllocValue(elem.TypeId, context);
                context.AddLocalVar(elem.Name, value);

                // InitExp가 있으면 
                if (elem.InitExp != null)
                    await expEvaluator.EvalAsync(elem.InitExp, value, context);
            }
        }

        public ValueTask EvalExpAsync(Exp exp, Value result, EvalContext context)
        {
            return expEvaluator.EvalAsync(exp, result, context);
        }

        public IAsyncEnumerable<Value> EvalStmtAsync(Stmt stmt, EvalContext context)
        {
            return stmtEvaluator.EvalStmtAsync(stmt, context);
        }
        
        async ValueTask<int> EvalScriptAsync(Script script, EvalContext context)
        {
            var retValue = AllocValue<IntValue>(TypeId.Int, context);

            await context.ExecInNewFuncFrameAsync(
                ImmutableDictionary<string, Value>.Empty, 
                EvalFlowControl.None, 
                ImmutableArray<Task>.Empty, 
                null, 
                retValue, 
                async () =>
                {
                    foreach (var topLevelStmt in script.TopLevelStmts)
                    {
                        await foreach (var value in stmtEvaluator.EvalStmtAsync(topLevelStmt, context))
                        {
                        }

                        if (context.GetFlowControl() == EvalFlowControl.Return)
                            break;
                    }
                });

            return retValue.GetInt();
        }

        public ValueTask<int> EvalScriptAsync(Script script)
        {
            var context = new EvalContext(script.Funcs, script.SeqFuncs);
            return EvalScriptAsync(script, context);
        }

        // TODO: DefaultApplication이랑 같이 어디론가 옮긴다
        //public async ValueTask<int?> EvaluateScriptAsync(
        //    string moduleName,
        //    Syntax.Script script,             
        //    IRuntimeModule runtimeModule,
        //    IEnumerable<IModuleInfo> moduleInfos,
        //    IErrorCollector errorCollector)
        //{
        //    // 4. stmt를 분석하고, 전역 변수 타입 목록을 만든다 (3의 함수정보가 필요하다)
        //    var optionalAnalyzeResult = analyzer.AnalyzeScript(moduleName, script, moduleInfos, errorCollector);
        //    if (optionalAnalyzeResult == null)
        //        return null;

        //    var analyzeResult = optionalAnalyzeResult.Value;

        //    var scriptModule = new ScriptModule(
        //        analyzeResult.ModuleInfo,
        //        scriptModule => new TypeValueApplier(new ModuleInfoService(moduleInfos.Append(scriptModule))),
        //        analyzeResult.Templates);

        //    var domainService = new DomainService();

        //    domainService.LoadModule(runtimeModule);
        //    domainService.LoadModule(scriptModule);

        //    var context = new EvalContext(
        //        runtimeModule, 
        //        domainService, 
        //        analyzeResult.TypeValueService,                 
        //        analyzeResult.Script.PrivateGlobalVarCount);

        //    return await EvaluateScriptAsync(analyzeResult.Script, context);
        //}
    }
}