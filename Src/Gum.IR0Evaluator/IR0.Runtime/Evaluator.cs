using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gum.Infra;
using Gum.IR0;
using Gum;
using System.Diagnostics.CodeAnalysis;
using static Gum.Infra.CollectionExtensions;

namespace Gum.IR0.Runtime
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

        public bool GetBaseType(Type type, EvalContext context, [NotNullWhen(true)] out Type? outBaseType)
        {
            throw new NotImplementedException();
            //var typeInst = context.GetTypeInst(type);

            //if (typeInst is ClassInst classInst)
            //{
            //    var baseType = classInst.GetBaseType();
            //    if (baseType != null)
            //    {
            //        outBaseType = baseType;
            //        return true;
            //    }
            //}

            //outBaseType = null;
            //return false;
        }
        
        // xType이 y타입인가 묻는 것
        public bool IsType(Type xType, Type yType, EvalContext context)
        {
            Type? curType = xType;

            while (curType != null)
            {
                if (EqualityComparer<Type?>.Default.Equals(curType, yType))
                    return true;

                if (!GetBaseType(curType.Value, context, out var baseTypeValue))
                    throw new InvalidOperationException();

                if (baseTypeValue == null)
                    break;

                curType = baseTypeValue;
            }

            return false;
        }

        public TValue AllocValue<TValue>(Type type, EvalContext context)
            where TValue : Value
        {
            return (TValue)AllocValue(type, context);
        }

        // type은 ir0 syntax의 일부분이다
        public Value AllocValue(Type type, EvalContext context)
        {
            switch(type.DeclId.Value)
            {
                case (int)TypeDeclId.PredefinedValue.Void:
                    return VoidValue.Instance;

                case (int)TypeDeclId.PredefinedValue.Bool:
                    return new BoolValue();

                case (int)TypeDeclId.PredefinedValue.Int:
                    return new IntValue();

                case (int)TypeDeclId.PredefinedValue.String:
                    return new StringValue();

                // TODO: typeArgs
                case (int)TypeDeclId.PredefinedValue.Enumerable:
                    return new AsyncEnumerableValue();

                case (int)TypeDeclId.PredefinedValue.Lambda:
                    return new LambdaValue();

                // TODO: typeArgs
                case (int)TypeDeclId.PredefinedValue.List:
                    return new ListValue();
            }

            throw new NotImplementedException();
        }        

        public ImmutableDictionary<string, Value> MakeCaptures(ImmutableArray<CaptureInfo.Element> captureElems, EvalContext context)
        {
            var capturesBuilder = ImmutableDictionary.CreateBuilder<string, Value>();
            foreach (var captureElem in captureElems)
            {
                var origValue = context.GetLocalValue(captureElem.LocalVarName);
                var value = AllocValue(captureElem.Type, context);
                value.SetValue(origValue);
                capturesBuilder.Add(captureElem.LocalVarName, value);
            }

            return capturesBuilder.ToImmutable();
        }

        public async ValueTask<ImmutableDictionary<string, Value>> EvalArgumentsAsync(ImmutableArray<string> paramNames, ImmutableArray<ExpInfo> expInfos, EvalContext context)
        {
            var argsBuilder = ImmutableDictionary.CreateBuilder<string, Value>();

            foreach (var (paramName, expInfo) in Zip(paramNames, expInfos))
            {
                var argValue = AllocValue(expInfo.Type, context);
                argsBuilder.Add(paramName, argValue);

                await expEvaluator.EvalAsync(expInfo.Exp, argValue, context);
            }

            return argsBuilder.ToImmutable();
        }

        public async ValueTask EvalLambdaAsync(Lambda lambda, ImmutableArray<ExpInfo> args, Value result, EvalContext context)
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
                var value = AllocValue(elem.Type, context);
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
            var retValue = AllocValue<IntValue>(Type.Int, context);

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
            var context = new EvalContext(script.FuncDecls);
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