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

namespace Gum.StaticAnalysis
{
    public partial class Analyzer
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
        
        internal bool AnalyzeVarDecl<TVarDecl>(Syntax.VarDecl varDecl, VarDeclVisitor<TVarDecl> varDeclVisitor, Context context, [MaybeNullWhen(false)] out TVarDecl outVarDecl)
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
                        context.ErrorCollector.Add(elem, $"{elem.VarName}의 타입을 추론할 수 없습니다");
                        return false;
                    }
                    else
                    {
                        varDeclVisitor.VisitElement(elem.VarName, declTypeValue, null, context);
                    }
                }
                else
                {
                    // var 처리
                    IR0.Exp? ir0InitExp = null;
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
                            context.ErrorCollector.Add(elem, $"타입 {initExpTypeValue}의 값은 타입 {varDecl.Type}의 변수 {elem.VarName}에 대입할 수 없습니다.");
                    }

                    varDeclVisitor.VisitElement(elem.VarName, typeValue, ir0InitExp, context);
                }
            }

            outVarDecl = varDeclVisitor.Build();
            return true;
        }        

        public bool AnalyzeStringExpElement(
            Syntax.StringExpElement elem, 
            Context context, 
            [NotNullWhen(true)] out IR0.StringExpElement? outElem)
        {
            if (elem is Syntax.ExpStringExpElement expElem)
            {
                // TODO: exp의 결과 string으로 변환 가능해야 하는 조건도 고려해야 한다
                if (AnalyzeExp(expElem.Exp, null, context, out var ir0Exp, out var expTypeValue))
                {
                    outElem = new IR0.ExpStringExpElement(ir0Exp, expTypeValue);
                    return true;
                }                
            }
            else if (elem is Syntax.TextStringExpElement textElem)
            {
                outElem = new IR0.TextStringExpElement(textElem.Text);
                return true;
            }

            throw new InvalidOperationException();
        }

        public bool AnalyzeLambda(
            Syntax.Stmt body,
            ImmutableArray<Syntax.LambdaExpParam> parameters, // LambdaExp에서 바로 받기 때문에 ImmutableArray로 존재하면 편하다
            Context context,
            [NotNullWhen(true)] out IR0.Stmt? outBody,
            [NotNullWhen(true)] out IR0.CaptureInfo? outCaptureInfo,
            [NotNullWhen(true)] out TypeValue.Func? outFuncTypeValue)
        {
            outBody = null;
            outCaptureInfo = null;
            outFuncTypeValue = null;            

            // capture에 필요한 정보를 가져옵니다
            if (!capturer.Capture(parameters.Select(param => param.Name), body, out var captureResult))
            {
                context.ErrorCollector.Add(body, "변수 캡쳐에 실패했습니다");
                return false;
            }

            // 람다 함수 컨텍스트를 만든다
            var lambdaFuncId = context.MakeLabmdaFuncId();

            // 캡쳐된 variable은 새 VarId를 가져야 한다
            var funcContext = new FuncContext(lambdaFuncId, null, false);

            // 필요한 변수들을 찾는다
            var elems = new List<IR0.CaptureInfo.Element>();
            foreach (var needCapture in captureResult.NeedCaptures)
            {
                if (context.GetIdentifierInfo(needCapture.VarName, ImmutableArray<TypeValue>.Empty, null, out var idInfo))
                {
                    if (idInfo is IdentifierInfo.Var varIdInfo)
                    {
                        switch (varIdInfo.StorageInfo)
                        {
                            // 지역 변수라면 
                            case StorageInfo.Local localStorage:
                                elems.Add(new IR0.CaptureInfo.Element(needCapture.Kind, localStorage.Index));
                                break;

                            case StorageInfo.ModuleGlobal moduleGlobalStorage:
                            case StorageInfo.PrivateGlobal privateGlobalStorage:
                                break;

                            default:
                                throw new InvalidOperationException();
                        }

                        continue;
                    }
                }

                context.ErrorCollector.Add(body, "캡쳐실패");
                return false;                
            }            

            var paramTypeValues = new List<TypeValue>(parameters.Length);
            foreach (var param in parameters)
            {
                if (param.Type == null)
                {
                    context.ErrorCollector.Add(param, "람다 인자 타입추론은 아직 지원하지 않습니다");
                    return false;
                }

                var paramTypeValue = context.GetTypeValueByTypeExp(param.Type);

                paramTypeValues.Add(paramTypeValue);
                funcContext.AddLocalVarInfo(param.Name, paramTypeValue);
            }

            bool bResult = true;
            IR0.Stmt? stmt = null;

            context.ExecInFuncScope(funcContext, () =>
            {
                if (!AnalyzeStmt(body, context, out stmt))
                    bResult = false;
            });

            if (!bResult)
                return false;

            outBody = stmt!;
            outCaptureInfo = new IR0.CaptureInfo(false, elems);
            outFuncTypeValue = TypeValue.MakeFunc(
                funcContext.GetRetTypeValue() ?? TypeValue.MakeVoid(),
                paramTypeValues);

            return true;
        }

        public bool AnalyzeExp(
            Syntax.Exp exp, 
            TypeValue? hintTypeValue, 
            Context context, 
            [NotNullWhen(true)] out IR0.Exp? outIR0Exp, 
            [NotNullWhen(true)] out TypeValue? outTypeValue)
        {
            return expAnalyzer.AnalyzeExp(exp, hintTypeValue, context, out outIR0Exp, out outTypeValue);
        }

        public bool AnalyzeStmt(Syntax.Stmt stmt, Context context, [NotNullWhen(true)] out IR0.Stmt? outStmt)
        {
            return stmtAnalyzer.AnalyzeStmt(stmt, context, out outStmt);
        }
        
        public bool AnalyzeFuncDecl(Syntax.FuncDecl funcDecl, Context context, out IR0.FuncDecl outFuncDecl)
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

                bResult &= AnalyzeStmt(funcDecl.Body, context);

                // TODO: Body가 실제로 리턴을 제대로 하는지 확인해야 할 필요가 있다
                context.AddTemplate(ScriptTemplate.MakeFunc(
                    funcInfo.FuncId,
                    funcInfo.bSeqCall ? funcInfo.RetTypeValue : null,
                    funcInfo.bThisCall, context.GetLocalVarCount(), funcDecl.Body));
            });

            outFuncDecl = new IR0.FuncDecl()

            return bResult;
        }

        public bool AnalyzeEnumDecl(EnumDecl enumDecl, Context context)
        {
            var enumInfo = context.GetEnumInfoByDecl(enumDecl);
            var defaultElemInfo = enumInfo.GetDefaultElemInfo();
            var defaultFields = defaultElemInfo.FieldInfos.Select(fieldInfo => (fieldInfo.Name, fieldInfo.TypeValue));

            context.AddTemplate(ScriptTemplate.MakeEnum(enumInfo.TypeId, enumInfo.GetDefaultElemInfo().Name, defaultFields));

            return true;
        }

        bool AnalyzeScript(Syntax.Script script, Context context, 
            [NotNullWhen(true)] out IR0.Script? outScript)
        {
            bool bResult = true;

            // 4. 최상위 script를 분석한다
            foreach (var elem in script.Elements)
            {
                switch (elem)
                {
                    case Syntax.Script.StmtElement stmtElem: 
                        bResult &= AnalyzeStmt(stmtElem.Stmt, context); 
                        break;
                }
            }

            // 5. 각 func body를 분석한다 (4에서 얻게되는 글로벌 변수 정보가 필요하다)
            foreach (var elem in script.Elements)
            {
                switch (elem)
                {
                    // TODO: classDecl
                    case Syntax.Script.FuncDeclElement funcElem: 
                        bResult &= AnalyzeFuncDecl(funcElem.FuncDecl, context);
                        break;

                    case Syntax.Script.EnumDeclElement enumElem:
                        bResult &= AnalyzeEnumDecl(enumElem.EnumDecl, context);
                        break;
                }
            }

            if (bResult)
            {
                outScript = new IR0.Script(context.GetPrivateGlobalVarCount(), context.GetLocalVarCount());
                return true;
            }
            else
            {
                outScript = null;
                return false;
            }
        }

        public (IR0.Script Script, 
            ImmutableArray<ScriptTemplate> Templates,
            TypeValueService TypeValueService,
            ScriptModuleInfo ModuleInfo)?
            AnalyzeScript(
            string moduleName,
            Syntax.Script script,
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
                context.GetTemplates().ToImmutableArray(), 
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
            return TypeValue.MakeNormal(ModuleItemId.Make("string")); ;
        }

        public bool CheckInstanceMember(
            MemberExp memberExp,
            TypeValue objTypeValue,
            Context context,
            [NotNullWhen(true)] out VarValue? outVarValue)
        {
            outVarValue = null;

            // TODO: Func추가
            TypeValue.Normal? objNormalTypeValue = objTypeValue as TypeValue.Normal;

            if (objNormalTypeValue == null)
            {
                context.ErrorCollector.Add(memberExp, "멤버를 가져올 수 없습니다");
                return false;
            }

            if (0 < memberExp.MemberTypeArgs.Length)
                context.ErrorCollector.Add(memberExp, "멤버변수에는 타입인자를 붙일 수 없습니다");

            if (!context.TypeValueService.GetMemberVarValue(objNormalTypeValue, Name.MakeText(memberExp.MemberName), out outVarValue))
            {
                context.ErrorCollector.Add(memberExp, $"{memberExp.MemberName}은 {objNormalTypeValue}의 멤버가 아닙니다");
                return false;
            }

            return true;
        }

        public bool CheckStaticMember(
            Syntax.MemberExp memberExp,
            TypeValue.Normal objNormalTypeValue,
            Context context,
            [NotNullWhen(true)] out VarValue? outVarValue)
        {
            outVarValue = null;

            if (!context.TypeValueService.GetMemberVarValue(objNormalTypeValue, Name.MakeText(memberExp.MemberName), out outVarValue))
            {
                context.ErrorCollector.Add(memberExp, "멤버가 존재하지 않습니다");
                return false;
            }

            if (0 < memberExp.MemberTypeArgs.Length)
            {
                context.ErrorCollector.Add(memberExp, "멤버변수에는 타입인자를 붙일 수 없습니다");
                return false;
            }

            if (!Misc.IsVarStatic(outVarValue.VarId, context))
            {
                context.ErrorCollector.Add(memberExp, "정적 변수가 아닙니다");
                return false;
            }

            return true;
        }

        public bool CheckParamTypes(object objForErrorMsg, IReadOnlyList<TypeValue> parameters, IReadOnlyList<TypeValue> args, Context context)
        {
            if (parameters.Count != args.Count)
            {
                context.ErrorCollector.Add(objForErrorMsg, $"함수는 인자를 {parameters.Count}개 받는데, 호출 인자는 {args.Count} 개입니다");
                return false;
            }

            for (int i = 0; i < parameters.Count; i++)
            {
                if (!IsAssignable(parameters[i], args[i], context))
                {
                    context.ErrorCollector.Add(objForErrorMsg, $"함수의 {i + 1}번 째 매개변수 타입은 {parameters[i]} 인데, 호출 인자 타입은 {args[i]} 입니다");
                    return false;
                }
            }

            return true;
        }

        // TODO: Hint를 받을 수 있게 해야 한다
        public bool AnalyzeExps(IEnumerable<Syntax.Exp> exps, Context context, out ImmutableArray<(IR0.Exp Exp, TypeValue TypeValue)> outInfos)
        {
            outInfos = ImmutableArray<(IR0.Exp, TypeValue)>.Empty;

            var infos = new List<(IR0.Exp, TypeValue)>();

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
