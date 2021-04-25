using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gum.Infra;
using Gum.IR0;
using Gum;
using System.Diagnostics.CodeAnalysis;
using static Gum.Infra.CollectionExtensions;
using Gum.Collections;

namespace Gum.IR0.Runtime
{
    // 레퍼런스용 Big Step Evaluator, 
    // TODO: Small Step으로 가야하지 않을까 싶다 (yield로 실행 point 잡는거 해보면 재미있을 것 같다)
    public class Evaluator
    {
        ExpEvaluator expEvaluator;
        StmtEvaluator stmtEvaluator;
        LocEvaluator locEvaluator;

        public Evaluator(ICommandProvider commandProvider)
        {
            this.expEvaluator = new ExpEvaluator(this);
            this.stmtEvaluator = new StmtEvaluator(this, commandProvider);
            this.locEvaluator = new LocEvaluator(this);
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

                if (!GetBaseType(curType, context, out var baseTypeValue))
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
            switch(type)
            {
                case VoidType: 
                    return VoidValue.Instance;

                case StructType when type == Type.Bool:
                    return new BoolValue();

                case StructType when type == Type.Int:
                    return new IntValue();

                case ClassType when type == Type.String:
                    return new StringValue();

                case ClassType classType:
                    if (classType.Outer.Equals(new RootOuterType("System.Runtime", new NamespacePath("System"))) && classType.Name.Equals("List"))
                        return new ListValue();

                    throw new NotImplementedException();

                case AnonymousLambdaType lambdaType:
                    var lambdaDecl = context.GetDecl<LambdaDecl>(lambdaType.DeclId);

                    Value? capturedThis = null;
                    if (lambdaDecl.CapturedThisType != null)
                        capturedThis = AllocValue(lambdaDecl.CapturedThisType, context);

                    var capturesBuilder = ImmutableDictionary.CreateBuilder<string, Value>();
                    foreach (var (elemType, elemName) in lambdaDecl.CaptureInfo)
                    {
                        var elemValue = AllocValue(elemType, context);
                        capturesBuilder.Add(elemName, elemValue);
                    }

                    return new LambdaValue(lambdaType.DeclId, capturedThis, capturesBuilder.ToImmutable());
                
                case AnonymousSeqType _:
                    return new SeqValue();

                default:
                    throw new NotImplementedException();

            }           

            
            //switch(type.DeclId.Value)
            //{
            //    case (int)TypeDeclId.PredefinedValue.Void:
            //        return VoidValue.Instance;

            //    case (int)TypeDeclId.PredefinedValue.Bool:
            //        return new BoolValue();

            //    case (int)TypeDeclId.PredefinedValue.Int:
            //        return new IntValue();

            //    case (int)TypeDeclId.PredefinedValue.String:
            //        return new StringValue();

            //    // TODO: typeArgs
            //    case (int)TypeDeclId.PredefinedValue.Enumerable:
            //        return new AsyncEnumerableValue();

            //    case (int)TypeDeclId.PredefinedValue.Lambda:
            //        return new LambdaValue();

            //    // TODO: typeArgs
            //    case (int)TypeDeclId.PredefinedValue.List:
            //        return new ListValue();
            //}
        }        

        internal void Capture(LambdaValue lambdaValue, bool captureThis, ImmutableArray<string> captureLocalVars, EvalContext context)
        {
            if (captureThis)
                lambdaValue.CapturedThis!.SetValue(context.GetThisValue()!);

            foreach (var captureLocalVar in captureLocalVars)
            {
                var origValue = context.GetLocalValue(captureLocalVar);
                lambdaValue.Captures[captureLocalVar].SetValue(origValue);
            }
        }

        public async ValueTask<ImmutableDictionary<string, Value>> EvalArgumentsAsync(
            ImmutableDictionary<string, Value> origDict,
            ImmutableArray<ParamInfo> paramInfos, 
            ImmutableArray<Exp> exps, 
            EvalContext context)
        {
            var argsBuilder = origDict.ToBuilder();

            Debug.Assert(paramInfos.Length == exps.Length);
            for(int i = 0; i < paramInfos.Length; i++)
            {
                var argValue = AllocValue(paramInfos[i].Type, context);
                argsBuilder.Add(paramInfos[i].Name, argValue);

                await expEvaluator.EvalAsync(exps[i], argValue, context);
            }

            return argsBuilder.ToImmutable();
        }

        internal async ValueTask EvalLambdaAsync(LambdaValue lambdaValue, ImmutableArray<Exp> args, Value result, EvalContext context)
        {
            var lambdaDecl = context.GetDecl<LambdaDecl>(lambdaValue.LambdaDeclId);

            var thisValue = lambdaValue.CapturedThis;
            var localVars = await EvalArgumentsAsync(lambdaValue.Captures, lambdaDecl.ParamInfos, args, context);

            await context.ExecInNewFuncFrameAsync(localVars, EvalFlowControl.None, ImmutableArray<Task>.Empty, thisValue, result, async () =>
            {
                await foreach (var _ in EvalStmtAsync(lambdaDecl.Body, context)) { }
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
        
        public ValueTask<Value> EvalLocAsync(Loc loc, EvalContext context)
        {
            return locEvaluator.EvalLocAsync(loc, context);
        }

        public IAsyncEnumerable<Gum.Infra.Void> EvalStmtAsync(Stmt stmt, EvalContext context)
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
            var context = new EvalContext(script.Decls);
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