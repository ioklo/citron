using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gum.Infra;
using Gum;
using System.Diagnostics.CodeAnalysis;
using static Gum.Infra.CollectionExtensions;
using Gum.Collections;

using R = Gum.IR0;

namespace Gum.IR0Evaluator
{
    // 레퍼런스용 Big Step Evaluator, 
    // TODO: Small Step으로 가야하지 않을까 싶다 (yield로 실행 point 잡는거 해보면 재미있을 것 같다)
    public class Evaluator
    {
        EvalContext context;

        ExpEvaluator expEvaluator;
        StmtEvaluator stmtEvaluator;
        LocEvaluator locEvaluator;

        public Evaluator(ICommandProvider commandProvider)
        {
            this.expEvaluator = new ExpEvaluator(this);
            this.stmtEvaluator = new StmtEvaluator(this, commandProvider);
            this.locEvaluator = new LocEvaluator(this);
        }        
        
        internal ValueTask EvalStringExpAsync(R.StringExp command, Value result)
        {
            return expEvaluator.EvalStringExpAsync(command, result);
        }

        public bool GetBaseType(R.Type type, [NotNullWhen(true)] out R.Type? outBaseType)
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
        public bool IsType(R.Type xType, R.Type yType)
        {
            R.Type? curType = xType;

            while (curType != null)
            {
                if (EqualityComparer<R.Type?>.Default.Equals(curType, yType))
                    return true;

                if (!GetBaseType(curType, context, out var baseTypeValue))
                    throw new InvalidOperationException();

                if (baseTypeValue == null)
                    break;

                curType = baseTypeValue;
            }

            return false;
        }

        public TValue AllocValue<TValue>(R.Type type)
            where TValue : Value
        {
            return (TValue)AllocValue(type);
        }

        // type은 ir0 syntax의 일부분이다
        public Value AllocValue(R.Type type)
        {
            switch(type)
            {
                case R.VoidType: 
                    return VoidValue.Instance;

                case R.StructType when type == R.Type.Bool:
                    return new BoolValue();

                case R.StructType when type == R.Type.Int:
                    return new IntValue();

                case R.ClassType when type == R.Type.String:
                    return new StringValue();

                case R.ClassType classType:
                    if (classType.Outer.Equals(new R.RootOuterType("System.Runtime", new R.NamespacePath("System"))) && classType.Name.Equals("List"))
                        return new ListValue();

                    throw new NotImplementedException();

                case R.AnonymousLambdaType lambdaType:
                    var lambdaDecl = context.GetDecl<R.LambdaDecl>(lambdaType.DeclId);

                    Value? capturedThis = null;
                    if (lambdaDecl.CapturedThisType != null)
                        capturedThis = AllocValue(lambdaDecl.CapturedThisType);

                    var capturesBuilder = ImmutableDictionary.CreateBuilder<string, Value>();
                    foreach (var (elemType, elemName) in lambdaDecl.CaptureInfo)
                    {
                        var elemValue = AllocValue(elemType);
                        capturesBuilder.Add(elemName, elemValue);
                    }

                    return new LambdaValue(lambdaType.DeclId, capturedThis, capturesBuilder.ToImmutable());
                
                case R.AnonymousSeqType _:
                    return new SeqValue();

                default:
                    throw new NotImplementedException();

            }           

            
            //switch(type.DeclId.Value)
            //{
            //    case (int)R.TypeDeclId.PredefinedValue.Void:
            //        return VoidValue.Instance;

            //    case (int)R.TypeDeclId.PredefinedValue.Bool:
            //        return new BoolValue();

            //    case (int)R.TypeDeclId.PredefinedValue.Int:
            //        return new IntValue();

            //    case (int)R.TypeDeclId.PredefinedValue.String:
            //        return new StringValue();

            //    // TODO: typeArgs
            //    case (int)R.TypeDeclId.PredefinedValue.Enumerable:
            //        return new AsyncEnumerableValue();

            //    case (int)R.TypeDeclId.PredefinedValue.Lambda:
            //        return new LambdaValue();

            //    // TODO: typeArgs
            //    case (int)R.TypeDeclId.PredefinedValue.List:
            //        return new ListValue();
            //}
        }        

        internal void Capture(LambdaValue lambdaValue, bool captureThis, ImmutableArray<string> captureLocalVars)
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
            ImmutableArray<R.ParamInfo> paramInfos, 
            ImmutableArray<R.Exp> exps)
        {
            var argsBuilder = origDict.ToBuilder();

            Debug.Assert(paramInfos.Length == exps.Length);
            for(int i = 0; i < paramInfos.Length; i++)
            {
                var argValue = AllocValue(paramInfos[i].Type);
                argsBuilder.Add(paramInfos[i].Name, argValue);

                await expEvaluator.EvalAsync(exps[i], argValue);
            }

            return argsBuilder.ToImmutable();
        }

        internal async ValueTask EvalLambdaAsync(LambdaValue lambdaValue, ImmutableArray<R.Exp> args, Value result)
        {
            var lambdaDecl = context.GetDecl<R.LambdaDecl>(lambdaValue.LambdaDeclId);

            var thisValue = lambdaValue.CapturedThis;
            var localVars = await EvalArgumentsAsync(lambdaValue.Captures, lambdaDecl.ParamInfos, args);

            await context.ExecInNewFuncFrameAsync(localVars, EvalFlowControl.None, ImmutableArray<Task>.Empty, thisValue, result, async () =>
            {
                await foreach (var _ in EvalStmtAsync(lambdaDecl.Body, context)) { }
            });
        }

        public async ValueTask EvalLocalVarDeclAsync(R.LocalVarDecl localVarDecl)
        {
            foreach (var elem in localVarDecl.Elems)
            {
                var value = AllocValue(elem.Type);
                context.AddLocalVar(elem.Name, value);

                // InitExp가 있으면 
                if (elem.InitExp != null)
                    await expEvaluator.EvalAsync(elem.InitExp, value);
            }
        }

        public ValueTask EvalExpAsync(R.Exp exp, Value result)
        {
            return expEvaluator.EvalAsync(exp, result);
        }
        
        public ValueTask<Value> EvalLocAsync(R.Loc loc)
        {
            return locEvaluator.EvalLocAsync(loc);
        }

        public IAsyncEnumerable<Gum.Infra.Void> EvalStmtAsync(R.Stmt stmt)
        {
            return stmtEvaluator.EvalStmtAsync(stmt);
        }
        
        async ValueTask<int> EvalScriptAsync(R.Script script)
        {
            var retValue = AllocValue<IntValue>(R.Type.Int);

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

        public ValueTask<int> EvalScriptAsync(R.Script script)
        {
            var context = new EvalContext(script.Decls);
            return EvalScriptAsync(script);
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
        //        scriptModule => new R.TypeValueApplier(new ModuleInfoService(moduleInfos.Append(scriptModule))),
        //        analyzeResult.Templates);

        //    var domainService = new DomainService();

        //    domainService.LoadModule(runtimeModule);
        //    domainService.LoadModule(scriptModule);

        //    var context = new EvalContext(
        //        runtimeModule, 
        //        domainService, 
        //        analyzeResult.TypeValueService,                 
        //        analyzeResult.Script.PrivateGlobalVarCount);

        //    return await EvaluateScriptAsync(analyzeResult.Script);
        //}

        Evaluator(ICommandProvider commandProvider, EvalContext context, Value? thisValue, ImmutableDictionary<string, Value> localVars)
        {
            this.expEvaluator = new ExpEvaluator(this);
            this.stmtEvaluator = new StmtEvaluator(this, commandProvider);
            this.locEvaluator = new LocEvaluator(this);

            this.context = new EvalContext(
                    context,
                    localVars,
                    EvalFlowControl.None,
                    ImmutableArray<Task>.Empty,
                    thisValue,
                    VoidValue.Instance);
        }

        Evaluator CloneWithNewContext(Value? thisValue, ImmutableDictionary<string, Value> localVars)
        {
            return new Evaluator(context, thisValue, localVars);
        }
    }
}