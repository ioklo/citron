using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Text;
using Gum.CompileTime;
using Gum.Infra;
using Gum.Misc;
using Pretune;
using static Gum.IR0.AnalyzeErrorCode;

using S = Gum.Syntax;

namespace Gum.IR0
{
    partial class Analyzer
    {           
        Context context;

        public static Script Analyze(
            S.Script script,
            ItemInfoRepository itemInfoRepo,
            TypeValueService typeValueService,
            TypeExpInfoService typeExpTypeValueService,
            IErrorCollector errorCollector)
        {
            var context = new Context(itemInfoRepo, typeValueService, typeExpTypeValueService, errorCollector);
            var analyzer = new Analyzer(context);

            // pass1, pass2
            var pass1 = new CollectingGlobalVarPass(analyzer);
            Gum.IR0.Misc.VisitScript(script, pass1);

            var pass2 = new TypeCheckingAndTranslatingPass(analyzer);
            Gum.IR0.Misc.VisitScript(script, pass2);

            // 5. 각 func body를 분석한다 (4에서 얻게되는 글로벌 변수 정보가 필요하다)
            return new Script(analyzer.context.GetTypeDecls(), analyzer.context.GetFuncDecls(), analyzer.context.GetTopLevelStmts());
        }

        Analyzer(Context context)
        {
            this.context = context;
        }

        [AutoConstructor]
        partial struct VarDeclElementCoreResult
        {
            public VarDeclElement Elem { get; }
            public TypeValue TypeValue { get; }
        }

        VarDeclElementCoreResult AnalyzeVarDeclElementCore(S.VarDeclElement elem, TypeValue declType)
        {
            if (elem.InitExp == null)
            {
                // var x; 체크
                if (declType is VarTypeValue)
                    context.AddFatalError(A0101_VarDecl_CantInferVarType, elem, $"{elem.VarName}의 타입을 추론할 수 없습니다");

                var type = context.GetType(declType);
                return new VarDeclElementCoreResult(new VarDeclElement(elem.VarName, type, null), declType);
            }
            else
            {
                // var 처리
                if (declType is VarTypeValue)
                {
                    var initExpResult = AnalyzeExp(elem.InitExp, null);
                    var type = context.GetType(initExpResult.TypeValue);
                    return new VarDeclElementCoreResult(new VarDeclElement(elem.VarName, type, initExpResult.Exp), initExpResult.TypeValue);
                }
                else
                {
                    var initExpResult = AnalyzeExp(elem.InitExp, declType);

                    if (!context.IsAssignable(declType, initExpResult.TypeValue))
                        context.AddFatalError(A0102_VarDecl_MismatchBetweenDeclTypeAndInitExpType, elem, $"타입 {initExpResult.TypeValue}의 값은 타입 {declType}의 변수 {elem.VarName}에 대입할 수 없습니다.");

                    var type = context.GetType(declType);
                    return new VarDeclElementCoreResult(new VarDeclElement(elem.VarName, type, initExpResult.Exp), declType);
                }
            }
        }

        [AutoConstructor]
        partial struct VarDeclElementResult
        {
            public VarDeclElement VarDeclElement { get; }
        }

        VarDeclElementResult AnalyzeInternalGlobalVarDeclElement(S.VarDeclElement elem, TypeValue declType)
        {
            var name = elem.VarName;

            if (context.DoesInternalGlobalVarNameExist(name))            
                context.AddFatalError(A0104_VarDecl_GlobalVariableNameShouldBeUnique, elem, $"전역 변수 {name}가 이미 선언되었습니다");

            var result = AnalyzeVarDeclElementCore(elem, declType);

            context.AddInternalGlobalVarInfo(name, result.TypeValue);
            return new VarDeclElementResult(result.Elem);
        }

        VarDeclElementResult AnalyzeLocalVarDeclElement(S.VarDeclElement elem, TypeValue declType)
        {
            var name = elem.VarName;

            if (context.DoesLocalVarNameExistInScope(name))
                context.AddFatalError(A0103_VarDecl_LocalVarNameShouldBeUniqueWithinScope, elem, $"지역 변수 {name}이 같은 범위에 이미 선언되었습니다");

            var result = AnalyzeVarDeclElementCore(elem, declType);

            context.AddLocalVarInfo(name, result.TypeValue);
            return new VarDeclElementResult(result.Elem);
        }

        [AutoConstructor]
        partial struct GlobalVarDeclResult
        {
            public ImmutableArray<VarDeclElement> Elems { get; }
        }

        GlobalVarDeclResult AnalyzeGlobalVarDecl(S.VarDecl varDecl)
        {
            var declType = context.GetTypeValueByTypeExp(varDecl.Type);

            var elems = new List<VarDeclElement>();
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
            public LocalVarDecl VarDecl { get; }
        }

        LocalVarDeclResult AnalyzeLocalVarDecl(S.VarDecl varDecl)
        {
            var declType = context.GetTypeValueByTypeExp(varDecl.Type);

            var elems = new List<VarDeclElement>();
            foreach (var elem in varDecl.Elems)
            {
                var result = AnalyzeLocalVarDeclElement(elem, declType);
                elems.Add(result.VarDeclElement);
            }

            return new LocalVarDeclResult(new LocalVarDecl(elems.ToImmutableArray()));
        }

        StringExpElement AnalyzeStringExpElement(S.StringExpElement elem)
        {
            if (elem is S.ExpStringExpElement expElem)
            {
                var expResult = AnalyzeExp(expElem.Exp, null);

                // 캐스팅이 필요하다면 
                if (expResult.TypeValue == TypeValues.Int)
                {
                    return new ExpStringExpElement(
                        new CallInternalUnaryOperatorExp(
                            InternalUnaryOperator.ToString_Int_String,
                            new ExpInfo(expResult.Exp, Type.Int)
                        )
                    );
                }
                else if (expResult.TypeValue == TypeValues.Bool)
                {
                    return new ExpStringExpElement(
                            new CallInternalUnaryOperatorExp(
                            InternalUnaryOperator.ToString_Bool_String,
                            new ExpInfo(expResult.Exp, Type.Bool)
                        )
                    );
                }
                else if (expResult.TypeValue == TypeValues.String)
                {
                    return new ExpStringExpElement(expResult.Exp);
                }
                else
                {
                    context.AddFatalError(A1901_StringExp_ExpElementShouldBeBoolOrIntOrString, expElem.Exp, "문자열 내부에서 사용되는 식은 bool, int, string 이어야 합니다");
                }
            }
            else if (elem is S.TextStringExpElement textElem)
            {
                return new TextStringExpElement(textElem.Text);
            }

            throw new UnreachableCodeException();
        }

        [AutoConstructor]
        partial struct LambdaResult
        {
            public Stmt Body { get; }
            public CaptureInfo CaptureInfo { get; }
            public FuncTypeValue FuncTypeValue { get; }
        }

        LambdaResult AnalyzeLambda(S.ISyntaxNode nodeForErrorReport, S.Stmt body, ImmutableArray<S.LambdaExpParam> parameters)
        {   
            // 람다 안에서 캡쳐해야할 변수들 목록
            var capturedLocalVars = new List<CaptureInfo.Element>();

            // TODO: 리턴 타입은 타입 힌트를 반영해야 한다
            TypeValue? retTypeValue = null;

            // 파라미터는 람다 함수의 지역변수로 취급한다
            var paramInfos = new List<(string Name, TypeValue TypeValue)>();
            foreach (var param in parameters)
            {
                if (param.Type == null)
                    context.AddFatalError(A9901_NotSupported_LambdaParameterInference, nodeForErrorReport, "람다 인자 타입추론은 아직 지원하지 않습니다");

                var paramTypeValue = context.GetTypeValueByTypeExp(param.Type);

                paramInfos.Add((param.Name, paramTypeValue));
            }

            var bodyResult = new StmtResult(); // suppress CS0165

            context.ExecInLambdaScope(retTypeValue, () => {

                // 람다 파라미터를 지역 변수로 추가한다
                foreach(var paramInfo in paramInfos)
                    context.AddLocalVarInfo(paramInfo.Name, paramInfo.TypeValue);

                // 본문 분석
                bodyResult = AnalyzeStmt(body);
                
                // 성공했으면, 리턴 타입 갱신
                retTypeValue = context.GetRetTypeValue();

                // CapturedLocalVar 작성
                foreach (var localVarOutsideLambdaInfo in context.GetLocalVarsOutsideLambda())
                {
                    if (localVarOutsideLambdaInfo.bNeedCapture)
                    {
                        var typeId = context.GetType(localVarOutsideLambdaInfo.LocalVarInfo.TypeValue);
                        capturedLocalVars.Add(new CaptureInfo.Element(typeId, localVarOutsideLambdaInfo.LocalVarInfo.Name));
                    }
                }
            });

            return new LambdaResult(
                bodyResult.Stmt, 
                new CaptureInfo(false, capturedLocalVars),
                new FuncTypeValue(
                    retTypeValue ?? VoidTypeValue.Instance,
                    paramInfos.Select(paramInfo => paramInfo.TypeValue)));
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
                case S.MemberCallExp _:
                    return true;

                default:
                    return false;
            }
        }

        ExpResult AnalyzeTopLevelExp(S.Exp exp, TypeValue? hintTypeValue, AnalyzeErrorCode code)
        {
            if (!IsTopLevelExp(exp))
                context.AddFatalError(code, exp, "대입, 함수 호출만 구문으로 사용할 수 있습니다");

            return AnalyzeExp(exp, hintTypeValue);
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
                var funcPath = context.GetCurFuncPath();
                Debug.Assert(funcPath != null);
                
                if (funcDecl.IsSequence)
                {
                    // TODO: Body가 실제로 리턴을 제대로 하는지 확인해야 한다
                    var retTypeValue = context.GetRetTypeValue();
                    Debug.Assert(retTypeValue != null, "문법상 Sequence 함수의 retValue가 없을수 없습니다");

                    var retType = context.GetType(retTypeValue);
                    var parameters = funcDecl.ParamInfo.Parameters.Select(param => param.Name).ToImmutableArray();
                    context.AddSeqFuncDecl(funcPath.Value, retType, false, funcDecl.TypeParams, parameters, bodyResult.Stmt);
                }
                else
                {
                    // TODO: Body가 실제로 리턴을 제대로 하는지 확인해야 한다
                    var parameters = funcDecl.ParamInfo.Parameters.Select(param => param.Name).ToImmutableArray();
                    context.AddFuncDecl(funcPath.Value, bThisCall: false, funcDecl.TypeParams, parameters, bodyResult.Stmt);
                }
            });
        }

        public MemberVarValue CheckStaticMember(S.MemberExp memberExp, NormalTypeValue objNormalTypeValue)
        {
            var varValue = context.GetMemberVarValue(objNormalTypeValue, memberExp.MemberName);

            if (varValue == null)
                context.AddFatalError(A0303_MemberExp_MemberVarNotFound, memberExp, "멤버가 존재하지 않습니다");

            if (0 < memberExp.MemberTypeArgs.Length)
                context.AddFatalError(A0302_MemberExp_TypeArgsForMemberVariableIsNotAllowed, memberExp, "멤버변수에는 타입인자를 붙일 수 없습니다");

            if (!Misc.IsVarStatic(varValue.GetItemId(), context))
                context.AddFatalError(A0304_MemberExp_MemberVariableIsNotStatic, memberExp, "정적 변수가 아닙니다");
            
            return varValue;
        }

        void CheckParamTypes(S.ISyntaxNode nodeForErrorReport, ImmutableArray<TypeValue> parameters, ImmutableArray<TypeValue> args)
        {
            bool bFatal = false;

            if (parameters.Length != args.Length)
            {
                context.AddError(A0401_Parameter_MismatchBetweenParamCountAndArgCount, nodeForErrorReport, $"함수는 인자를 {parameters.Length}개 받는데, 호출 인자는 {args.Length} 개입니다");
                bFatal = true;
            }

            for (int i = 0; i < parameters.Length; i++)
            {
                if (!context.IsAssignable(parameters[i], args[i]))
                {
                    context.AddError(A0402_Parameter_MismatchBetweenParamTypeAndArgType, nodeForErrorReport, $"함수의 {i + 1}번 째 매개변수 타입은 {parameters[i]} 인데, 호출 인자 타입은 {args[i]} 입니다");
                    bFatal = true;
                }
            }

            if (bFatal)
                throw new FatalAnalyzeException();
        }

        // TODO: Hint를 받을 수 있게 해야 한다
        ImmutableArray<ExpResult> AnalyzeExps(IEnumerable<S.Exp> exps)
        {
            var results = new List<ExpResult>();

            foreach (var exp in exps)
            {
                var expResult = AnalyzeExp(exp, null);
                results.Add(expResult);
            }

            return results.ToImmutableArray();
        }
    }
}
