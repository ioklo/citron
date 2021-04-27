using System;
using System.Collections.Generic;
using Gum.Collections;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Text;
using M = Gum.CompileTime;
using Gum.Infra;
using Pretune;
using static Gum.IR0Translator.AnalyzeErrorCode;

using S = Gum.Syntax;
using R = Gum.IR0;

namespace Gum.IR0Translator
{
    partial class Analyzer
    {
        InternalBinaryOperatorQueryService internalBinOpQueryService;
        Context context;

        public static R.Script Analyze(
            S.Script script,
            ItemValueFactory itemValueFactory,
            GlobalItemValueFactory globalItemValueFactory,
            TypeExpInfoService typeExpTypeValueService,
            IErrorCollector errorCollector)
        {
            var context = new Context(itemValueFactory, globalItemValueFactory, typeExpTypeValueService, errorCollector);
            var internalBinOpQueryService = new InternalBinaryOperatorQueryService(itemValueFactory);
            var analyzer = new Analyzer(internalBinOpQueryService, context);

            // pass1, pass2
            var pass1 = new CollectingGlobalVarPass(analyzer);
            IR0Translator.Misc.VisitScript(script, pass1);

            var pass2 = new TypeCheckingAndTranslatingPass(analyzer);
            IR0Translator.Misc.VisitScript(script, pass2);

            // 5. 각 func body를 분석한다 (4에서 얻게되는 글로벌 변수 정보가 필요하다)
            return new R.Script(analyzer.context.GetDecls(), analyzer.context.GetTopLevelStmts());
        }

        Analyzer(InternalBinaryOperatorQueryService internalBinOpQueryService, Context context)
        {
            this.internalBinOpQueryService = internalBinOpQueryService;
            this.context = context;
        }

        [AutoConstructor]
        partial struct VarDeclElementCoreResult
        {
            public R.VarDeclElement Elem { get; }
            public TypeValue TypeValue { get; }
        }

        VarDeclElementCoreResult AnalyzeVarDeclElementCore(S.VarDeclElement elem, TypeValue declType)
        {
            if (elem.InitExp == null)
            {
                // var x; 체크
                if (declType is VarTypeValue)
                    context.AddFatalError(A0101_VarDecl_CantInferVarType, elem);

                var rtype = declType.GetRType();
                return new VarDeclElementCoreResult(new R.VarDeclElement(elem.VarName, rtype, null), declType);
            }
            else
            {
                // var 처리
                if (declType is VarTypeValue)
                {
                    var initExpResult = AnalyzeExp_Exp(elem.InitExp, ResolveHint.None);
                    var rtype = initExpResult.TypeValue.GetRType();
                    return new VarDeclElementCoreResult(new R.VarDeclElement(elem.VarName, rtype, initExpResult.Exp), initExpResult.TypeValue);
                }
                else
                {
                    var initExpResult = AnalyzeExp_Exp(elem.InitExp, ResolveHint.Make(declType));

                    if (!context.IsAssignable(declType, initExpResult.TypeValue))
                        context.AddFatalError(A0102_VarDecl_MismatchBetweenDeclTypeAndInitExpType, elem);

                    var rtype = declType.GetRType();
                    return new VarDeclElementCoreResult(new R.VarDeclElement(elem.VarName, rtype, initExpResult.Exp), declType);
                }
            }
        }

        [AutoConstructor]
        partial struct VarDeclElementResult
        {
            public R.VarDeclElement VarDeclElement { get; }
        }

        VarDeclElementResult AnalyzeInternalGlobalVarDeclElement(S.VarDeclElement elem, TypeValue declType)
        {
            var name = elem.VarName;

            if (context.DoesInternalGlobalVarNameExist(name))            
                context.AddFatalError(A0104_VarDecl_GlobalVariableNameShouldBeUnique, elem);

            var result = AnalyzeVarDeclElementCore(elem, declType);

            context.AddInternalGlobalVarInfo(name, result.TypeValue);
            return new VarDeclElementResult(result.Elem);
        }

        VarDeclElementResult AnalyzeLocalVarDeclElement(S.VarDeclElement elem, TypeValue declType)
        {
            var name = elem.VarName;

            if (context.DoesLocalVarNameExistInScope(name))
                context.AddFatalError(A0103_VarDecl_LocalVarNameShouldBeUniqueWithinScope, elem);

            var result = AnalyzeVarDeclElementCore(elem, declType);

            context.AddLocalVarInfo(name, result.TypeValue);
            return new VarDeclElementResult(result.Elem);
        }

        [AutoConstructor]
        partial struct GlobalVarDeclResult
        {
            public ImmutableArray<R.VarDeclElement> Elems { get; }
        }

        GlobalVarDeclResult AnalyzeGlobalVarDecl(S.VarDecl varDecl)
        {
            var declType = context.GetTypeValueByTypeExp(varDecl.Type);

            var elems = new List<R.VarDeclElement>();
            foreach (var elem in varDecl.Elems)
            {
                var result = AnalyzeInternalGlobalVarDeclElement(elem, declType);
                elems.Add(result.VarDeclElement);
            }

            return new GlobalVarDeclResult(elems.ToImmutableArray());
        }

        [AutoConstructor]
        partial struct LocalVarDeclResult
        {
            public R.LocalVarDecl VarDecl { get; }
        }

        LocalVarDeclResult AnalyzeLocalVarDecl(S.VarDecl varDecl)
        {
            var declType = context.GetTypeValueByTypeExp(varDecl.Type);

            var elems = new List<R.VarDeclElement>();
            foreach (var elem in varDecl.Elems)
            {
                var result = AnalyzeLocalVarDeclElement(elem, declType);
                elems.Add(result.VarDeclElement);
            }

            return new LocalVarDeclResult(new R.LocalVarDecl(elems.ToImmutableArray()));
        }

        R.StringExpElement AnalyzeStringExpElement(S.StringExpElement elem)
        {
            if (elem is S.ExpStringExpElement expElem)
            {
                var expResult = AnalyzeExp_Exp(expElem.Exp, ResolveHint.None);

                // 캐스팅이 필요하다면 
                if (context.IsIntType(expResult.TypeValue))
                {
                    return new R.ExpStringExpElement(
                        new R.CallInternalUnaryOperatorExp(
                            R.InternalUnaryOperator.ToString_Int_String,
                            expResult.Exp
                        )
                    );
                }
                else if (context.IsBoolType(expResult.TypeValue))
                {
                    return new R.ExpStringExpElement(
                            new R.CallInternalUnaryOperatorExp(
                            R.InternalUnaryOperator.ToString_Bool_String,
                            expResult.Exp
                        )
                    );
                }
                else if (context.IsStringType(expResult.TypeValue))
                {
                    return new R.ExpStringExpElement(expResult.Exp);
                }
                else
                {
                    context.AddFatalError(A1901_StringExp_ExpElementShouldBeBoolOrIntOrString, expElem.Exp);
                }
            }
            else if (elem is S.TextStringExpElement textElem)
            {
                return new R.TextStringExpElement(textElem.Text);
            }

            throw new UnreachableCodeException();
        }

        [AutoConstructor]
        partial struct LambdaResult
        {
            public bool bCaptureThis { get; }
            public ImmutableArray<string> CaptureLocalVars { get; }
            public LambdaTypeValue TypeValue { get; }
        }

        LambdaResult AnalyzeLambda(S.ISyntaxNode nodeForErrorReport, S.Stmt body, ImmutableArray<S.LambdaExpParam> parameters)
        {
            // TODO: 리턴 타입은 타입 힌트를 반영해야 한다
            TypeValue? retTypeValue = null;

            // 파라미터는 람다 함수의 지역변수로 취급한다
            var paramInfos = new List<(string Name, TypeValue TypeValue)>();
            foreach (var param in parameters)
            {
                if (param.Type == null)
                    context.AddFatalError(A9901_NotSupported_LambdaParameterInference, nodeForErrorReport);

                var paramTypeValue = context.GetTypeValueByTypeExp(param.Type);
                paramInfos.Add((param.Name, paramTypeValue));
            }

            var (bCaptureThis, bodyResult, capturedLocalVars, newRetTypeValue) = context.ExecInLambdaScope(retTypeValue, () => {

                // 람다 파라미터를 지역 변수로 추가한다
                foreach(var paramInfo in paramInfos)
                    context.AddLocalVarInfo(paramInfo.Name, paramInfo.TypeValue);

                // 본문 분석
                var bodyResult = AnalyzeStmt(body);

                // 성공했으면, 리턴 타입 갱신
                var bCaptureThis = context.NeedCaptureThis();
                var retTypeValue = context.GetRetTypeValue();
                var capturedLocalVars = context.GetCapturedLocalVars();

                return (bCaptureThis, bodyResult, capturedLocalVars, retTypeValue);
            });

            var paramTypes = paramInfos.Select(paramInfo => paramInfo.TypeValue).ToImmutableArray();
            var rparamInfos = paramInfos.Select(paramInfo => new R.ParamInfo(paramInfo.TypeValue.GetRType(), paramInfo.Name)).ToImmutableArray();

            var lambdaDeclId = context.AddLambdaDecl(null, capturedLocalVars, rparamInfos, bodyResult.Stmt);

            var lambdaTypeValue = context.NewLambdaTypeValue(
                lambdaDeclId,
                retTypeValue ?? VoidTypeValue.Instance,
                paramTypes
            );

            var captureLocalVarNames = ImmutableArray.CreateRange(capturedLocalVars, c => c.Name);
            return new LambdaResult(bCaptureThis, captureLocalVarNames, lambdaTypeValue);
        }

        bool IsTopLevelExp(S.Exp exp)
        {
            switch (exp)
            {
                case S.UnaryOpExp unOpExp:
                    return unOpExp.Kind == S.UnaryOpKind.PostfixInc ||
                        unOpExp.Kind == S.UnaryOpKind.PostfixDec ||
                        unOpExp.Kind == S.UnaryOpKind.PrefixInc ||
                        unOpExp.Kind == S.UnaryOpKind.PrefixDec;

                case S.BinaryOpExp binOpExp:
                    return binOpExp.Kind == S.BinaryOpKind.Assign;

                case S.CallExp _:
                    return true;

                default:
                    return false;
            }
        }

        ExpExpResult AnalyzeTopLevelExp_Exp(S.Exp exp, ResolveHint hint, AnalyzeErrorCode code)
        {
            if (!IsTopLevelExp(exp))
                context.AddFatalError(code, exp);

            return AnalyzeExp_Exp(exp, hint);
        }

        public void AnalyzeFuncDecl(S.FuncDecl funcDecl)
        {
            context.ExecInFuncScope(funcDecl, () =>
            {
                if (0 < funcDecl.TypeParams.Length || funcDecl.ParamInfo.VariadicParamIndex != null)
                    throw new NotImplementedException();
                
                // 파라미터 순서대로 추가
                foreach (var param in funcDecl.ParamInfo.Parameters)
                {
                    var paramTypeValue = context.GetTypeValueByTypeExp(param.Type);
                    context.AddLocalVarInfo(param.Name, paramTypeValue);
                }

                var bodyResult = AnalyzeStmt(funcDecl.Body);
                
                if (funcDecl.IsSequence)
                {
                    // TODO: Body가 실제로 리턴을 제대로 하는지 확인해야 한다
                    var retTypeValue = context.GetRetTypeValue();
                    Debug.Assert(retTypeValue != null, "문법상 Sequence 함수의 retValue가 없을수 없습니다");

                    var retRType = retTypeValue.GetRType();
                    var parameters = funcDecl.ParamInfo.Parameters.Select(param => param.Name).ToImmutableArray();

                    var rparamInfos = ImmutableArray.CreateRange(funcDecl.ParamInfo.Parameters, param =>
                    {
                        var typeValue = context.GetTypeValueByTypeExp(param.Type);
                        var rtype = typeValue.GetRType();

                        return new R.ParamInfo(rtype, param.Name);
                    });

                    context.AddSequenceFuncDecl(retRType, false, funcDecl.TypeParams, rparamInfos, bodyResult.Stmt);
                }
                else
                {
                    // TODO: Body가 실제로 리턴을 제대로 하는지 확인해야 한다
                    var parameters = funcDecl.ParamInfo.Parameters.Select(param =>
                    {
                        var paramTypeValue = context.GetTypeValueByTypeExp(param.Type);
                        return new R.ParamInfo(paramTypeValue.GetRType(), param.Name);
                    }).ToImmutableArray();

                    context.AddNormalFuncDecl(funcDecl.Name, bThisCall: false, funcDecl.TypeParams, parameters, bodyResult.Stmt);
                }
            });
        }
        
        void CheckParamTypes(S.ISyntaxNode nodeForErrorReport, ImmutableArray<TypeValue> parameters, ImmutableArray<TypeValue> args)
        {
            bool bFatal = false;

            if (parameters.Length != args.Length)
            {
                context.AddError(A0401_Parameter_MismatchBetweenParamCountAndArgCount, nodeForErrorReport);
                bFatal = true;
            }

            for (int i = 0; i < parameters.Length; i++)
            {
                if (!context.IsAssignable(parameters[i], args[i]))
                {
                    context.AddError(A0402_Parameter_MismatchBetweenParamTypeAndArgType, nodeForErrorReport);
                    bFatal = true;
                }
            }

            if (bFatal)
                throw new FatalAnalyzeException();
        }
    }
}
