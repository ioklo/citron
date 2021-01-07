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

using static Gum.IR0.AnalyzeErrorCode;

using S = Gum.Syntax;

namespace Gum.IR0
{
    partial class Analyzer
    {   
        Context context;

        public Analyzer(Context context)
        {
            this.context = context;
        }
        
        internal bool AnalyzeVarDecl<TVarDecl>(S.VarDecl varDecl, VarDeclVisitor<TVarDecl> varDeclVisitor, [MaybeNullWhen(false)] out TVarDecl outVarDecl)
        {
            // 1. int x  // x를 추가
            // 2. int x = initExp // x 추가, initExp가 int인지 검사
            // 3. var x = initExp // initExp의 타입을 알아내고 x를 추가
            // 4. var x = 1, y = "string"; // 각각 한다

            var declTypeValue = context.GetTypeValueByTypeExp(varDecl.Type);
            
            foreach (var elem in varDecl.Elems)
            {
                if (elem.InitExp == null)
                {
                    if (declTypeValue is TypeValue.Var)
                    {
                        context.AddError(A0101_VarDecl_CantInferVarType, elem, $"{elem.VarName}의 타입을 추론할 수 없습니다");
                        return false;
                    }
                    else
                    {
                        varDeclVisitor.VisitElement(elem, elem.VarName, declTypeValue, null);
                    }
                }
                else
                {
                    // var 처리
                    Exp? ir0InitExp;
                    TypeValue typeValue;
                    if (declTypeValue is TypeValue.Var)
                    {
                        if (!AnalyzeExp(elem.InitExp, null, out ir0InitExp, out var initExpTypeValue))
                            return false;

                        typeValue = initExpTypeValue;
                    }
                    else
                    {
                        if (!AnalyzeExp(elem.InitExp, declTypeValue, out ir0InitExp, out var initExpTypeValue))
                            return false;

                        typeValue = declTypeValue;

                        if (!IsAssignable(declTypeValue, initExpTypeValue))
                            context.AddError(A0102_VarDecl_MismatchBetweenDeclTypeAndInitExpType, elem, $"타입 {initExpTypeValue}의 값은 타입 {varDecl.Type}의 변수 {elem.VarName}에 대입할 수 없습니다.");
                    }

                    varDeclVisitor.VisitElement(elem, elem.VarName, typeValue, ir0InitExp);
                }
            }

            outVarDecl = varDeclVisitor.Build();
            return true;
        }        

        public bool AnalyzeStringExpElement(
            S.StringExpElement elem, 
            [NotNullWhen(true)] out StringExpElement? outElem)
        {
            if (elem is S.ExpStringExpElement expElem)
            {
                if (!AnalyzeExp(expElem.Exp, null, out var ir0Exp, out var expTypeValue))
                {
                    outElem = null;
                    return false;
                }                

                // 캐스팅이 필요하다면 
                if (expTypeValue == TypeValues.Int)
                {
                    outElem = new ExpStringExpElement(
                        new CallInternalUnaryOperatorExp(
                            InternalUnaryOperator.ToString_Int_String,
                            new ExpInfo(ir0Exp, Type.Int)
                        )
                    );
                    return true;
                }
                else if (expTypeValue == TypeValues.Bool)
                {
                    outElem = new ExpStringExpElement(
                            new CallInternalUnaryOperatorExp(
                            InternalUnaryOperator.ToString_Bool_String,
                            new ExpInfo(ir0Exp, Type.Bool)
                        )
                    );
                    return true;
                }
                else if (expTypeValue == TypeValues.String)
                {
                    outElem = new ExpStringExpElement(ir0Exp);
                    return true;
                }
                else
                {
                    context.AddError(A1901_StringExp_ExpElementShouldBeBoolOrIntOrString, expElem.Exp, "문자열 내부에서 사용되는 식은 bool, int, string 이어야 합니다");
                    outElem = null;
                    return false;
                }
            }
            else if (elem is S.TextStringExpElement textElem)
            {
                outElem = new TextStringExpElement(textElem.Text);
                return true;
            }

            throw new InvalidOperationException();
        }

        public bool AnalyzeLambda(
            S.ISyntaxNode nodeForErrorReport,
            S.Stmt body,
            IEnumerable<S.LambdaExpParam> parameters, 
            [NotNullWhen(true)] out Stmt? outBody,
            [NotNullWhen(true)] out CaptureInfo? outCaptureInfo,
            [NotNullWhen(true)] out TypeValue.Func? outFuncTypeValue)
        {
            outBody = null;
            outCaptureInfo = null;
            outFuncTypeValue = null;            

            // 람다 안에서 캡쳐해야할 변수들 목록
            var capturedLocalVars = new List<CaptureInfo.Element>();

            // TODO: 리턴 타입은 타입 힌트를 반영해야 한다
            TypeValue? retTypeValue = null;

            // 파라미터는 람다 함수의 지역변수로 취급한다
            var paramInfos = new List<(string Name, TypeValue TypeValue)>();
            foreach (var param in parameters)
            {
                if (param.Type == null)
                {
                    context.AddError(A9901_NotSupported_LambdaParameterInference, nodeForErrorReport, "람다 인자 타입추론은 아직 지원하지 않습니다");
                    return false;
                }

                var paramTypeValue = context.GetTypeValueByTypeExp(param.Type);

                paramInfos.Add((param.Name, paramTypeValue));
            }

            Stmt? ir0Body = null;
            var result = context.ExecInLambdaScope(retTypeValue, () => {

                // 람다 파라미터를 지역 변수로 추가한다
                foreach(var paramInfo in paramInfos)
                    context.AddLocalVarInfo(paramInfo.Name, paramInfo.TypeValue);

                // 본문 분석
                if (!AnalyzeStmt(body, out ir0Body))
                    return false;
                
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

                return true;
            });

            if (!result.IsSuccess)
                return false;

            Debug.Assert(ir0Body != null);

            outBody = ir0Body;
            outCaptureInfo = new CaptureInfo(false, capturedLocalVars);
            outFuncTypeValue = new TypeValue.Func(
                result.RetTypeValue ?? TypeValue.Void.Instance,
                paramInfos.Select(paramInfo => paramInfo.TypeValue));

            return true;
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

        public bool AnalyzeTopLevelExp(
            S.Exp exp, 
            TypeValue? hintTypeValue, 
            AnalyzeErrorCode code,
            [NotNullWhen(true)] out Exp? outExp, 
            [NotNullWhen(true)] out TypeValue? outTypeValue)
        {
            if (!IsTopLevelExp(exp))
            {
                context.AddError(code, exp, "대입, 함수 호출만 구문으로 사용할 수 있습니다");
                outExp = null;
                outTypeValue = null;
                return false;
            }

            return AnalyzeExp(exp, hintTypeValue, out outExp, out outTypeValue);
        }
        
        public bool AnalyzeFuncDecl(S.FuncDecl funcDecl)
        {
            var funcInfo = context.GetFuncInfoByDecl(funcDecl);
            var bResult = true;
            
            var funcContext = new FuncContext(funcInfo.RetTypeValue, funcInfo.bSeqCall);
            context.ExecInFuncScope(funcContext, () =>
            {   
                if (0 < funcDecl.TypeParams.Length || funcDecl.ParamInfo.VariadicParamIndex != null)
                    throw new NotImplementedException();
                
                // 파라미터 순서대로 추가
                foreach (var param in funcDecl.ParamInfo.Parameters)
                {
                    var paramTypeValue = context.GetTypeValueByTypeExp(param.Type);
                    context.AddLocalVarInfo(param.Name, paramTypeValue);
                }

                if (AnalyzeStmt(funcDecl.Body, out var body))
                {
                    if (funcDecl.IsSequence)
                    { 
                        // TODO: Body가 실제로 리턴을 제대로 하는지 확인해야 할 필요가 있다
                        var retTypeId = context.GetType(funcInfo.RetTypeValue);
                        context.AddSeqFunc(funcInfo.GetId(), retTypeId, false, funcDecl.TypeParams, funcDecl.ParamInfo.Parameters.Select(param => param.Name), body);
                    }
                    else
                    {
                        // TODO: Body가 실제로 리턴을 제대로 하는지 확인해야 할 필요가 있다
                        context.AddFuncDecl(funcInfo.GetId(), bThisCall: false, funcDecl.TypeParams, funcDecl.ParamInfo.Parameters.Select(param => param.Name), body);
                    }
                }
                else
                {
                    bResult = false;
                }
            });
            
            return bResult;
        }

        public bool AnalyzeTypeDecl(S.TypeDecl typeDecl)
        {
            switch(typeDecl)
            {
                case S.EnumDecl enumDecl:
                    return AnalyzeEnumDecl(enumDecl);

                case S.StructDecl structDecl:
                    throw new NotImplementedException();

                default:
                    throw new InvalidOperationException();
            }
        }

        public bool AnalyzeEnumDecl(S.EnumDecl enumDecl)
        {
            var enumInfo = context.GetTypeInfoByDecl<EnumInfo>(enumDecl);            

            var defaultElemInfo = enumInfo.GetDefaultElemInfo();
            var defaultFields = defaultElemInfo.FieldInfos.Select(fieldInfo => (fieldInfo.Name, fieldInfo.TypeValue));

            //context.AddEnum()
            throw new NotImplementedException();
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
            foreach (var elem in script.Elements)
            {
                switch (elem)
                {
                    // TODO: classDecl
                    case S.Script.GlobalFuncDeclElement funcElem:
                        if (!AnalyzeFuncDecl(funcElem.FuncDecl))
                            bResult = false;
                        break;

                    case S.Script.TypeDeclElement typeElem:
                        if (!AnalyzeTypeDecl(typeElem.TypeDecl))
                            bResult = false;
                        break;
                }
            }

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

        public AnalyzeScript(S.Script script, IErrorCollector errorCollector)
        {
            if (!AnalyzeScript(script, out var ir0Script))
                return null;

            if (errorCollector.HasError)
                return null;

            return (ir0Script,
                typeValueService, 
                buildResult.ModuleInfo);
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

            if (!Misc.IsVarStatic(outVarValue.GetItemId()))
            {
                context.AddError(A0304_MemberExp_MemberVariableIsNotStatic, memberExp, "정적 변수가 아닙니다");
                return false;
            }

            return true;
        }

        public bool CheckParamTypes(S.ISyntaxNode nodeForErrorReport, IReadOnlyList<TypeValue> parameters, IReadOnlyList<TypeValue> args)
        {
            if (parameters.Count != args.Count)
            {
                context.AddError(A0401_Parameter_MismatchBetweenParamCountAndArgCount, nodeForErrorReport, $"함수는 인자를 {parameters.Count}개 받는데, 호출 인자는 {args.Count} 개입니다");
                return false;
            }

            for (int i = 0; i < parameters.Count; i++)
            {
                if (!IsAssignable(parameters[i], args[i]))
                {
                    context.AddError(A0402_Parameter_MismatchBetweenParamTypeAndArgType, nodeForErrorReport, $"함수의 {i + 1}번 째 매개변수 타입은 {parameters[i]} 인데, 호출 인자 타입은 {args[i]} 입니다");
                    return false;
                }
            }

            return true;
        }

        // TODO: Hint를 받을 수 있게 해야 한다
        public bool AnalyzeExps(IEnumerable<S.Exp> exps, out ImmutableArray<(Exp Exp, TypeValue TypeValue)> outInfos)
        {
            outInfos = ImmutableArray<(Exp, TypeValue)>.Empty;

            var infos = new List<(Exp, TypeValue)>();

            foreach (var exp in exps)
            {
                if (!AnalyzeExp(exp, null, out var ir0Exp, out var typeValue))
                    return false;

                infos.Add((ir0Exp, typeValue));
            }

            outInfos = infos.ToImmutableArray();
            return true;
        }

    }
}
