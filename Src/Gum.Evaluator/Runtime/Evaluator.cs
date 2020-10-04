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
using Gum.StaticAnalysis;
using Gum;

namespace Gum.Runtime
{
    // 레퍼런스용 Big Step Evaluator, 
    // TODO: Small Step으로 가야하지 않을까 싶다 (yield로 실행 point 잡는거 해보면 재미있을 것 같다)
    public class Evaluator
    {
        private Analyzer analyzer;
        private ExpEvaluator expValueEvaluator;
        private StmtEvaluator stmtEvaluator;        

        public Evaluator(Analyzer analyzer, ICommandProvider commandProvider)
        {
            this.analyzer = analyzer;
            this.expValueEvaluator = new ExpEvaluator(this);
            this.stmtEvaluator = new StmtEvaluator(this, commandProvider);
        }        
        
        public ValueTask EvaluateStringExpAsync(StringExp command, Value result, EvalContext context)
        {
            return expValueEvaluator.EvalStringExpAsync(command, result, context);
        }

        TypeArgumentList ApplyTypeArgumentList(TypeArgumentList typeArgList, ImmutableDictionary<TypeValue.TypeVar, TypeValue> typeEnv)
        {
            TypeArgumentList? appliedOuter = null;

            if (typeArgList.Outer != null)
                appliedOuter = ApplyTypeArgumentList(typeArgList.Outer, typeEnv);

            var appliedArgs = typeArgList.Args.Select(arg => ApplyTypeValue(arg, typeEnv));

            return TypeArgumentList.Make(appliedOuter, appliedArgs);
        }

        TypeValue ApplyTypeValue(TypeValue typeValue, ImmutableDictionary<TypeValue.TypeVar, TypeValue> typeEnv)
        {
            switch(typeValue)
            {
                case TypeValue.TypeVar typeVar: 
                    return typeEnv[typeVar];

                case TypeValue.Normal ntv:
                    {
                        var appliedTypeArgList = ApplyTypeArgumentList(ntv.TypeArgList, typeEnv);
                        return TypeValue.MakeNormal(ntv.TypeId, appliedTypeArgList);
                    }

                case TypeValue.Void vtv: 
                    return typeValue;

                case TypeValue.Func ftv:
                    {
                        var appliedReturn = ApplyTypeValue(ftv.Return, typeEnv);
                        var appliedParams = ftv.Params.Select(param => ApplyTypeValue(param, typeEnv));

                        return TypeValue.MakeFunc(appliedReturn, appliedParams);
                    }

                default:
                    throw new NotImplementedException();
            }            
        }

        // xType이 y타입인가 묻는 것
        public bool IsType(TypeValue xTypeValue, TypeValue yTypeValue, EvalContext context)
        {
            TypeValue? curTypeValue = xTypeValue;

            while (curTypeValue != null)
            {
                if (EqualityComparer<TypeValue?>.Default.Equals(curTypeValue, yTypeValue))
                    return true;

                if (!context.TypeValueService.GetBaseTypeValue(curTypeValue, out var baseTypeValue))
                    throw new InvalidOperationException();

                if (baseTypeValue == null)
                    break;

                curTypeValue = baseTypeValue;
            }

            return false;
        }

        // DefaultValue란 무엇인가, 그냥 선언만 되어있는 상태        
        public Value GetDefaultValue(TypeValue typeValue, EvalContext context)
        {
            var typeInst = context.DomainService.GetTypeInst(typeValue);
            return typeInst.MakeDefaultValue();
        }

        public IEnumerable<Value> MakeCaptures(ImmutableArray<CaptureInfo.Element> captureElems, EvalContext context)
        {
            var captures = new List<Value>(captureElems.Length);
            foreach (var captureElem in captureElems)
            {
                Value origValue = context.GetLocalValue(captureElem.LocalVarIndex);

                Value value;
                if (captureElem.CaptureKind == CaptureKind.Copy)
                {
                    value = origValue!.MakeCopy();
                }
                else
                {
                    Debug.Assert(captureElem.CaptureKind == CaptureKind.Ref);
                    value = origValue!;
                }

                captures.Add(value);
            }

            return captures;
        }

        async IAsyncEnumerable<Value> EvaluateScriptFuncInstSeqAsync(
            ScriptFuncInst scriptFuncInst,
            IReadOnlyList<Value> args,
            EvalContext context)
        {
            // NOTICE: args가 미리 할당되서 나온 상태
            for (int i = 0; i < args.Count; i++)
                context.AddLocalVar(i, args[i]);

            await foreach (var value in EvaluateStmtAsync(scriptFuncInst.Body, context))
            {
                yield return value;
            }
        }

        public async ValueTask EvaluateLocalVarDeclAsync(LocalVarDecl localVarDecl, EvalContext context)
        {
            foreach (var elem in localVarDecl.Elems)
            {
                var value = GetDefaultValue(localVarDecl.Type, context);
                context.AddLocalVar(elem.Name, value);

                // InitExp가 있으면 
                if (elem.InitExp != null)
                    await expValueEvaluator.EvalAsync(elem.InitExp, value, context);
            }
        }
        
        public ValueTask EvaluateFuncInstAsync(Value? thisValue, FuncInst funcInst, IReadOnlyList<Value> args, Value result, EvalContext context)
        {            
            if (funcInst is ScriptFuncInst scriptFuncInst)
            {
                async ValueTask InnerBodyAsync()
                {
                    await foreach (var _ in EvaluateStmtAsync(scriptFuncInst.Body, context)) { }
                }

                // (Capture한 곳의 this), (MemberExp의 this), Static의 경우 this
                if (scriptFuncInst.CapturedThis != null)
                    thisValue = scriptFuncInst.CapturedThis;
                else if (!scriptFuncInst.bThisCall)
                    thisValue = null;

                var localVars = new Value?[scriptFuncInst.LocalVarCount];
                for (int i = 0; i < scriptFuncInst.Captures.Length; i++)
                    localVars[i] = scriptFuncInst.Captures[i];

                int argEndIndex = scriptFuncInst.Captures.Length + args.Count;
                for (int i = scriptFuncInst.Captures.Length; i < argEndIndex; i++)
                    localVars[i] = args[i];
                
                // Seq Call 이라면
                if (scriptFuncInst.SeqElemTypeValue != null)
                {
                    // yield에 사용할 공간
                    var yieldValue = GetDefaultValue(scriptFuncInst.SeqElemTypeValue, context);

                    // context 복제
                    var newContext = new EvalContext(
                        context,
                        localVars,
                        EvalFlowControl.None,
                        ImmutableArray<Task>.Empty,
                        thisValue,
                        yieldValue);

                    var asyncEnum = EvaluateScriptFuncInstSeqAsync(scriptFuncInst, args, newContext);
                    context.RuntimeModule.SetEnumerable(context.DomainService, result, scriptFuncInst.SeqElemTypeValue, asyncEnum);

                    return new ValueTask(Task.CompletedTask);
                }

                return context.ExecInNewFuncFrameAsync(localVars, EvalFlowControl.None, ImmutableArray<Task>.Empty, thisValue, result, InnerBodyAsync);
            }
            else if (funcInst is NativeFuncInst nativeFuncInst)
            {
                return nativeFuncInst.CallAsync(thisValue, args, result);
            }
            else
            {
                throw new InvalidOperationException();
            }
        }

        public ValueTask EvalExpAsync(Exp exp, Value result, EvalContext context)
        {
            return expValueEvaluator.EvalAsync(exp, result, context);
        }

        public IAsyncEnumerable<Value> EvaluateStmtAsync(Stmt stmt, EvalContext context)
        {
            return stmtEvaluator.EvaluateStmtAsync(stmt, context);
        }
        
        async ValueTask<int> EvaluateScriptAsync(Script script, EvalContext context)
        {
            async ValueTask InnerBodyAsync()
            {
                foreach (var elem in script.Elements)
                {
                    if (elem is Script.StmtElement statementElem)
                    {
                        await foreach (var value in stmtEvaluator.EvaluateStmtAsync(statementElem.Stmt, context))
                        {
                        }
                    }

                    if (context.GetFlowControl() == EvalFlowControl.Return)
                        break;
                }
            }
            
            var retValue = context.RuntimeModule.MakeInt(0);

            await context.ExecInNewFuncFrameAsync(
                new Value?[script.LocalVarCount], 
                EvalFlowControl.None, 
                ImmutableArray<Task>.Empty, 
                null, 
                retValue, 
                InnerBodyAsync);

            return context.RuntimeModule.GetInt(retValue);
        }

        public async ValueTask<int?> EvaluateScriptAsync(
            string moduleName,
            Syntax.Script script,             
            IRuntimeModule runtimeModule,
            IEnumerable<IModuleInfo> moduleInfos,
            IErrorCollector errorCollector)
        {
            // 4. stmt를 분석하고, 전역 변수 타입 목록을 만든다 (3의 함수정보가 필요하다)
            var optionalAnalyzeResult = analyzer.AnalyzeScript(moduleName, script, moduleInfos, errorCollector);
            if (optionalAnalyzeResult == null)
                return null;

            var analyzeResult = optionalAnalyzeResult.Value;

            var scriptModule = new ScriptModule(
                analyzeResult.ModuleInfo,
                scriptModule => new TypeValueApplier(new ModuleInfoService(moduleInfos.Append(scriptModule))),
                analyzeResult.Templates);

            var domainService = new DomainService();

            domainService.LoadModule(runtimeModule);
            domainService.LoadModule(scriptModule);

            var context = new EvalContext(
                runtimeModule, 
                domainService, 
                analyzeResult.TypeValueService,                 
                analyzeResult.Script.PrivateGlobalVarCount);

            return await EvaluateScriptAsync(analyzeResult.Script, context);
        }
        
        internal Value GetMemberValue(Value value, Name varName)
        {
            if (value is ObjectValue objValue)
                return objValue.GetMemberValue(varName);
            else
                throw new InvalidOperationException();
        }
    }
}