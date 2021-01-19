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
using Pretune;
using static Gum.IR0.AnalyzeErrorCode;

using S = Gum.Syntax;

namespace Gum.IR0
{
    partial class Analyzer
    {        
        TypeSkeletonRepository skelRepo;
        Context context;

        public Analyzer(
            TypeSkeletonRepository skelRepo,            
            ItemInfoRepository itemInfoRepo,
            TypeValueService typeValueService,
            TypeExpTypeValueService typeExpTypeValueService,
            IErrorCollector errorCollector)
        {
            this.skelRepo = skelRepo;
            this.context = new Context(itemInfoRepo, typeValueService, typeExpTypeValueService, errorCollector);
        }

        public TypeValue AnalyzeTypeExp(S.TypeExp typeExp)
        {
            return context.GetTypeValueByTypeExp(typeExp);
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
                if (declType is TypeValue.Var)
                    context.AddFatalError(A0101_VarDecl_CantInferVarType, elem, $"{elem.VarName}의 타입을 추론할 수 없습니다");

                var type = context.GetType(declType);
                return new VarDeclElementCoreResult(new VarDeclElement(elem.VarName, type, null), declType);
            }
            else
            {
                // var 처리
                if (declType is TypeValue.Var)
                {
                    var initExpResult = AnalyzeExp(elem.InitExp, null);
                    var type = context.GetType(initExpResult.TypeValue);
                    return new VarDeclElementCoreResult(new VarDeclElement(elem.VarName, type, initExpResult.Exp), initExpResult.TypeValue);
                }
                else
                {
                    var initExpResult = AnalyzeExp(elem.InitExp, declType);

                    if (!IsAssignable(declType, initExpResult.TypeValue))
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

        VarDeclElementResult AnalyzePrivateGlobalVarDeclElement(S.VarDeclElement elem, TypeValue declType)
        {
            var name = elem.VarName;

            if (context.DoesPrivateGlobalVarNameExist(name))            
                context.AddFatalError(A0104_VarDecl_GlobalVariableNameShouldBeUnique, elem, $"전역 변수 {name}가 이미 선언되었습니다");

            var result = AnalyzeVarDeclElementCore(elem, declType);

            context.AddPrivateGlobalVarInfo(name, result.TypeValue);
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
        partial struct PrivateGlobalVarDeclResult
        {
            public ImmutableArray<VarDeclElement> Elems { get; }
        }

        PrivateGlobalVarDeclResult AnalyzePrivateGlobalVarDecl(S.VarDecl varDecl)
        {
            var declType = context.GetTypeValueByTypeExp(varDecl.Type);

            var elems = new List<VarDeclElement>();
            foreach (var elem in varDecl.Elems)
            {
                var result = AnalyzePrivateGlobalVarDeclElement(elem, declType);
                elems.Add(result.VarDeclElement);
            }

            return new PrivateGlobalVarDeclResult(elems.ToImmutableArray());
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
            public TypeValue.Func FuncTypeValue { get; }
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
                new TypeValue.Func(
                    retTypeValue ?? TypeValue.Void.Instance,
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

        public void AnalyzeFuncSkel(FuncSkeleton funcSkel)
        {
            context.ExecInFuncScope(funcSkel.FuncDecl, () =>
            {
                var funcDecl = funcSkel.FuncDecl;

                if (0 < funcDecl.TypeParams.Length || funcDecl.ParamInfo.VariadicParamIndex != null)
                    throw new NotImplementedException();
                
                // 파라미터 순서대로 추가
                foreach (var param in funcSkel.FuncDecl.ParamInfo.Parameters)
                {
                    var paramTypeValue = context.GetTypeValueByTypeExp(param.Type);
                    context.AddLocalVarInfo(param.Name, paramTypeValue);
                }

                var bodyResult = AnalyzeStmt(funcDecl.Body);
                
                if (funcDecl.IsSequence)
                {
                    // TODO: Body가 실제로 리턴을 제대로 하는지 확인해야 할 필요가 있다
                    var retType = context.GetType(context.GetRetTypeValue());
                    context.AddSeqFuncDecl(funcSkel.Path, retType, false, funcDecl.TypeParams, funcDecl.ParamInfo.Parameters.Select(param => param.Name), bodyResult.Stmt);
                }
                else
                {
                    // TODO: Body가 실제로 리턴을 제대로 하는지 확인해야 할 필요가 있다
                    context.AddFuncDecl(funcSkel.Path, bThisCall: false, funcDecl.TypeParams, funcDecl.ParamInfo.Parameters.Select(param => param.Name), bodyResult.Stmt);
                }
            });
        }
        
        // stmt가 존재하는 곳
        // GlobalFunc, MemberFunc, TopLevel
        bool AnalyzeScript(S.Script script, [NotNullWhen(true)] out Script? outScript)
        {
            bool bResult = true;

            var topLevelStmts = new List<Stmt>();
            foreach (var elem in script.Elements)
            {
                switch (elem)
                {
                    case S.Script.StmtElement stmtElem:
                        if (!AnalyzeStmt(stmtElem.Stmt, out var topLevelStmt))
                            bResult = false;
                        else
                            topLevelStmts.Add(topLevelStmt);
                        break;
                }
            }

            // 5. 각 func body를 분석한다 (4에서 얻게되는 글로벌 변수 정보가 필요하다)            

            if (bResult)
            {
                outScript = new Script(context.GetTypeDecls(), context.GetFuncDecls(), topLevelStmts);
                return true;
            }
            else
            {
                outScript = null;
                return false;
            }
        }
        
        public Script? AnalyzeScript(S.Script script, IErrorCollector errorCollector)
        {
            if (!AnalyzeScript(script, out var ir0Script))
                return null;

            if (errorCollector.HasError)
                return null;

            return ir0Script;
        }

        public bool IsAssignable(TypeValue toTypeValue, TypeValue fromTypeValue)
        {
            // B <- D
            // 지금은 fromType의 base들을 찾아가면서 toTypeValue와 맞는 것이 있는지 본다
            // TODO: toTypeValue가 interface라면, fromTypeValue의 interface들을 본다

            TypeValue? curType = fromTypeValue;
            while (curType != null)
            {
                if (ModuleInfoEqualityComparer.EqualsTypeValue(toTypeValue, curType))
                    return true;

                if (!context.TypeValueService.GetBaseTypeValue(curType, out var outType))
                    return false;

                curType = outType;
            }

            return false;
        }        
        
        public bool CheckInstanceMember(
            S.MemberExp memberExp,
            TypeValue objTypeValue,
            [NotNullWhen(true)] out VarValue? outVarValue)
        {
            outVarValue = null;

            // TODO: Func추가
            TypeValue.Normal? objNormalTypeValue = objTypeValue as TypeValue.Normal;

            if (objNormalTypeValue == null)
            {
                context.AddError(A0301_MemberExp_InstanceTypeIsNotNormalType, memberExp, "멤버를 가져올 수 있는 타입이 아닙니다");
                return false;
            }

            if (0 < memberExp.MemberTypeArgs.Length)
                context.AddError(A0302_MemberExp_TypeArgsForMemberVariableIsNotAllowed, memberExp, "멤버변수에는 타입인자를 붙일 수 없습니다");

            if (!context.TypeValueService.GetMemberVarValue(objNormalTypeValue, memberExp.MemberName, out outVarValue))
            {
                context.AddError(A0303_MemberExp_MemberVarNotFound, memberExp, $"{memberExp.MemberName}은 {objNormalTypeValue}의 멤버가 아닙니다");
                return false;
            }

            return true;
        }

        public bool CheckStaticMember(
            S.MemberExp memberExp,
            TypeValue.Normal objNormalTypeValue,
            [NotNullWhen(true)] out VarValue? outVarValue)
        {
            outVarValue = null;

            if (!context.TypeValueService.GetMemberVarValue(objNormalTypeValue, memberExp.MemberName, out outVarValue))
            {
                context.AddError(A0303_MemberExp_MemberVarNotFound, memberExp, "멤버가 존재하지 않습니다");
                return false;
            }

            if (0 < memberExp.MemberTypeArgs.Length)
            {
                context.AddError(A0302_MemberExp_TypeArgsForMemberVariableIsNotAllowed, memberExp, "멤버변수에는 타입인자를 붙일 수 없습니다");
                return false;
            }

            if (!Misc.IsVarStatic(outVarValue.GetItemId(), context))
            {
                context.AddError(A0304_MemberExp_MemberVariableIsNotStatic, memberExp, "정적 변수가 아닙니다");
                return false;
            }

            return true;
        }

        void CheckParamTypes(S.ISyntaxNode nodeForErrorReport, IReadOnlyList<TypeValue> parameters, IReadOnlyList<TypeValue> args)
        {
            bool bFatal = false;

            if (parameters.Count != args.Count)
            {
                context.AddError(A0401_Parameter_MismatchBetweenParamCountAndArgCount, nodeForErrorReport, $"함수는 인자를 {parameters.Count}개 받는데, 호출 인자는 {args.Count} 개입니다");
                bFatal = true;
            }

            for (int i = 0; i < parameters.Count; i++)
            {
                if (!IsAssignable(parameters[i], args[i]))
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

        public Script Run()
        {
            var topLevelStmts = new List<Stmt>();

            foreach (var stmt in skelRepo.GetTopLevelStmts())
            {
                var stmtResult = AnalyzeStmt(stmt);
                topLevelStmts.Add(stmtResult.Stmt);
            }

            // 현재 func밖에 없으므로
            foreach (var globalSkel in skelRepo.GetGlobalSkeletons())
            {
                if (globalSkel is FuncSkeleton funcSkel)
                    AnalyzeFuncSkel(funcSkel);
            }

            return new Script(context.GetTypeDecls(), context.GetFuncDecls(), topLevelStmts);
        }
    }
}
