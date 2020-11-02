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
        ModuleInfoBuilder moduleInfoBuilder;
        Capturer capturer;

        ExpAnalyzer expAnalyzer;
        StmtAnalyzer stmtAnalyzer;        

        public Analyzer(ModuleInfoBuilder moduleInfoBuilder, Capturer capturer)
        {
            this.moduleInfoBuilder = moduleInfoBuilder;

            // 내부 전용 클래스는 new를 써서 직접 만들어도 된다 (DI, 인자로 받을 필요 없이)
            this.capturer = capturer;
            this.expAnalyzer = new ExpAnalyzer(this);
            this.stmtAnalyzer = new StmtAnalyzer(this);
        }
        
        internal bool AnalyzeVarDecl<TVarDecl>(S.VarDecl varDecl, VarDeclVisitor<TVarDecl> varDeclVisitor, Context context, [MaybeNullWhen(false)] out TVarDecl outVarDecl)
        {
            // 1. int x  // x를 추가
            // 2. int x = initExp // x 추가, initExp가 int인지 검사
            // 3. var x = initExp // initExp의 타입을 알아내고 x를 추가
            // 4. var x = 1, y = "string"; // 각각 한다

            outVarDecl = default;
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
                        varDeclVisitor.VisitElement(elem, elem.VarName, declTypeValue, null, context);
                    }
                }
                else
                {
                    // var 처리
                    Exp? ir0InitExp = null;
                    TypeValue typeValue;
                    if (declTypeValue is TypeValue.Var)
                    {
                        if (!AnalyzeExp(elem.InitExp, null, context, out ir0InitExp, out var initExpTypeValue))
                            return false;

                        typeValue = initExpTypeValue;
                    }
                    else
                    {
                        if (!AnalyzeExp(elem.InitExp, declTypeValue, context, out ir0InitExp, out var initExpTypeValue))
                            return false;

                        typeValue = declTypeValue;

                        if (!IsAssignable(declTypeValue, initExpTypeValue, context))
                            context.AddError(A0102_VarDecl_MismatchBetweenDeclTypeAndInitExpType, elem, $"타입 {initExpTypeValue}의 값은 타입 {varDecl.Type}의 변수 {elem.VarName}에 대입할 수 없습니다.");
                    }

                    varDeclVisitor.VisitElement(elem, elem.VarName, typeValue, ir0InitExp, context);
                }
            }

            outVarDecl = varDeclVisitor.Build();
            return true;
        }        

        public bool AnalyzeStringExpElement(
            S.StringExpElement elem, 
            Context context, 
            [NotNullWhen(true)] out StringExpElement? outElem)
        {
            if (elem is S.ExpStringExpElement expElem)
            {
                // TODO: exp의 결과 string으로 변환 가능해야 하는 조건도 고려해야 한다
                if (!AnalyzeExp(expElem.Exp, null, context, out var ir0Exp, out var expTypeValue))
                {
                    outElem = null;
                    return false;
                }
                
                var expTypeId = context.GetType(expTypeValue);
                outElem = new ExpStringExpElement(new ExpInfo(ir0Exp, expTypeId));
                return true;                
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
            Context context,
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
                if (!AnalyzeStmt(body, context, out ir0Body))
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
            outFuncTypeValue = TypeValue.MakeFunc(
                result.RetTypeValue ?? TypeValue.MakeVoid(),
                paramInfos.Select(paramInfo => paramInfo.TypeValue));

            return true;
        }

        public bool AnalyzeExp(
            S.Exp exp, 
            TypeValue? hintTypeValue, 
            Context context, 
            [NotNullWhen(true)] out Exp? outIR0Exp, 
            [NotNullWhen(true)] out TypeValue? outTypeValue)
        {
            return expAnalyzer.AnalyzeExp(exp, hintTypeValue, context, out outIR0Exp, out outTypeValue);
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
            Context context,             
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

            return AnalyzeExp(exp, hintTypeValue, context, out outExp, out outTypeValue);
        }

        public bool AnalyzeStmt(S.Stmt stmt, Context context, [NotNullWhen(true)] out Stmt? outStmt)
        {
            return stmtAnalyzer.AnalyzeStmt(stmt, context, out outStmt);
        }
        
        public bool AnalyzeFuncDecl(S.FuncDecl funcDecl, Context context)
        {
            var funcInfo = context.GetFuncInfoByDecl(funcDecl);
            var bResult = true;
            
            var funcContext = new FuncContext(funcInfo.FuncId, funcInfo.RetTypeValue, funcInfo.bSeqCall);
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

                if (AnalyzeStmt(funcDecl.Body, context, out var body))
                {
                    if( funcDecl.IsSequence)
                    {
                        // TODO: Body가 실제로 리턴을 제대로 하는지 확인해야 할 필요가 있다
                        var retTypeId = context.GetType(funcInfo.RetTypeValue);
                        context.AddSeqFunc(retTypeId, false, funcDecl.TypeParams, funcDecl.ParamInfo.Parameters.Select(param => param.Name), body);
                    }
                    else
                    {
                        // TODO: Body가 실제로 리턴을 제대로 하는지 확인해야 할 필요가 있다
                        context.AddFuncDecl(bThisCall: false, funcDecl.TypeParams, funcDecl.ParamInfo.Parameters.Select(param => param.Name), body);
                    }
                }
                else
                {
                    bResult = false;
                }
            });
            
            return bResult;
        }

        public bool AnalyzeEnumDecl(S.EnumDecl enumDecl, Context context)
        {
            var enumInfo = context.GetEnumInfoByDecl(enumDecl);
            var defaultElemInfo = enumInfo.GetDefaultElemInfo();
            var defaultFields = defaultElemInfo.FieldInfos.Select(fieldInfo => (fieldInfo.Name, fieldInfo.TypeValue));

            //context.AddEnum()
            throw new NotImplementedException();
        }

        bool AnalyzeScript(S.Script script, Context context, 
            [NotNullWhen(true)] out Script? outScript)
        {
            bool bResult = true;

            var topLevelStmts = new List<Stmt>();
            foreach (var elem in script.Elements)
            {
                switch (elem)
                {
                    case S.Script.StmtElement stmtElem:
                        if (!AnalyzeStmt(stmtElem.Stmt, context, out var topLevelStmt))
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
                    case S.Script.FuncDeclElement funcElem:
                        if (!AnalyzeFuncDecl(funcElem.FuncDecl, context))
                            bResult = false;
                        break;

                    case S.Script.EnumDeclElement enumElem:
                        if (!AnalyzeEnumDecl(enumElem.EnumDecl, context))
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

        public (Script Script,
            TypeValueService TypeValueService,
            ScriptModuleInfo ModuleInfo)?
            AnalyzeScript(
            string moduleName,
            S.Script script,
            IEnumerable<IModuleInfo> moduleInfos,
            IErrorCollector errorCollector)
        {
            // 3. Type, Func만들기, ModuleInfoBuilder
            var buildResult = moduleInfoBuilder.BuildScript(moduleName, moduleInfos, script, errorCollector);
            if (buildResult == null)
                return null;

            var moduleInfoService = new ModuleInfoService(moduleInfos.Append(buildResult.ModuleInfo));
            var typeValueApplier = new TypeValueApplier(moduleInfoService);
            var typeValueService = new TypeValueService(moduleInfoService, typeValueApplier);

            var context = new Context(
                moduleInfoService,
                typeValueService,
                buildResult.TypeExpTypeValueService,
                buildResult.FuncInfosByDecl,
                buildResult.EnumInfosByDecl,
                errorCollector);

            if (!AnalyzeScript(script, context, out var ir0Script))
                return null;

            if (errorCollector.HasError)
                return null;

            return (ir0Script,
                typeValueService, 
                buildResult.ModuleInfo);
        }

        public bool IsAssignable(TypeValue toTypeValue, TypeValue fromTypeValue, Context context)
        {
            // B <- D
            // 지금은 fromType의 base들을 찾아가면서 toTypeValue와 맞는 것이 있는지 본다
            // TODO: toTypeValue가 interface라면, fromTypeValue의 interface들을 본다

            TypeValue? curType = fromTypeValue;
            while (curType != null)
            {
                if (EqualityComparer<TypeValue>.Default.Equals(toTypeValue, curType))
                    return true;

                if (!context.TypeValueService.GetBaseTypeValue(curType, out var outType))
                    return false;

                curType = outType;
            }

            return false;
        }

        public TypeValue GetIntTypeValue()
        {
            return TypeValue.MakeNormal(ModuleItemId.Make("int"));
        }

        public TypeValue GetBoolTypeValue()
        {
            return TypeValue.MakeNormal(ModuleItemId.Make("bool"));
        }

        public TypeValue GetStringTypeValue()
        {
            return TypeValue.MakeNormal(ModuleItemId.Make("string"));
        }

        public bool CheckInstanceMember(
            S.MemberExp memberExp,
            TypeValue objTypeValue,
            Context context,
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

            if (!context.TypeValueService.GetMemberVarValue(objNormalTypeValue, Name.MakeText(memberExp.MemberName), out outVarValue))
            {
                context.AddError(A0303_MemberExp_MemberVarNotFound, memberExp, $"{memberExp.MemberName}은 {objNormalTypeValue}의 멤버가 아닙니다");
                return false;
            }

            return true;
        }

        public bool CheckStaticMember(
            S.MemberExp memberExp,
            TypeValue.Normal objNormalTypeValue,
            Context context,
            [NotNullWhen(true)] out VarValue? outVarValue)
        {
            outVarValue = null;

            if (!context.TypeValueService.GetMemberVarValue(objNormalTypeValue, Name.MakeText(memberExp.MemberName), out outVarValue))
            {
                context.AddError(A0303_MemberExp_MemberVarNotFound, memberExp, "멤버가 존재하지 않습니다");
                return false;
            }

            if (0 < memberExp.MemberTypeArgs.Length)
            {
                context.AddError(A0302_MemberExp_TypeArgsForMemberVariableIsNotAllowed, memberExp, "멤버변수에는 타입인자를 붙일 수 없습니다");
                return false;
            }

            if (!Misc.IsVarStatic(outVarValue.VarId, context))
            {
                context.AddError(A0304_MemberExp_MemberVariableIsNotStatic, memberExp, "정적 변수가 아닙니다");
                return false;
            }

            return true;
        }

        public bool CheckParamTypes(S.ISyntaxNode nodeForErrorReport, IReadOnlyList<TypeValue> parameters, IReadOnlyList<TypeValue> args, Context context)
        {
            if (parameters.Count != args.Count)
            {
                context.AddError(A0401_Parameter_MismatchBetweenParamCountAndArgCount, nodeForErrorReport, $"함수는 인자를 {parameters.Count}개 받는데, 호출 인자는 {args.Count} 개입니다");
                return false;
            }

            for (int i = 0; i < parameters.Count; i++)
            {
                if (!IsAssignable(parameters[i], args[i], context))
                {
                    context.AddError(A0402_Parameter_MismatchBetweenParamTypeAndArgType, nodeForErrorReport, $"함수의 {i + 1}번 째 매개변수 타입은 {parameters[i]} 인데, 호출 인자 타입은 {args[i]} 입니다");
                    return false;
                }
            }

            return true;
        }

        // TODO: Hint를 받을 수 있게 해야 한다
        public bool AnalyzeExps(IEnumerable<S.Exp> exps, Context context, out ImmutableArray<(Exp Exp, TypeValue TypeValue)> outInfos)
        {
            outInfos = ImmutableArray<(Exp, TypeValue)>.Empty;

            var infos = new List<(Exp, TypeValue)>();

            foreach (var exp in exps)
            {
                if (!AnalyzeExp(exp, null, context, out var ir0Exp, out var typeValue))
                    return false;

                infos.Add((ir0Exp, typeValue));
            }

            outInfos = outInfos.ToImmutableArray();
            return true;
        }

    }
}
