using Gum.CompileTime;
using Gum.Infra;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using static Gum.IR0.AnalyzeErrorCode;

using S = Gum.Syntax;

namespace Gum.IR0
{
    // TypeExp를 TypeValue로 바꿔서 저장합니다.
    internal partial class TypeExpEvaluator
    {
        TypeSkeletonCollector typeSkeletonCollector;

        public TypeExpEvaluator(TypeSkeletonCollector typeSkeletonCollector)
        {
            this.typeSkeletonCollector = typeSkeletonCollector;
        }

        private bool HandleBuiltInType(S.IdTypeExp exp, string name, ItemId itemId, Context context, [NotNullWhen(true)] out TypeExpInfo? outInfo)
        {
            if (exp.TypeArgs.Length != 0)
            {
                context.AddError(T0101_IdTypeExp_TypeDoesntHaveTypeParams, exp, $"{name}은 타입 인자를 가질 수 없습니다");
                outInfo = null;
                return false;
            }

            outInfo = new TypeExpInfo.NoMember(new TypeValue.Normal(itemId));
            context.AddTypeValue(exp, new TypeValue.Normal(itemId)); // NOTICE: no type args
            return true;
        }
        
        private IEnumerable<TypeExpInfo> GetTypeExpInfos(AppliedItemPath appliedItemPath, Context context)
        {
            var itemPath = appliedItemPath.GetItemPath();

            if (context.GetSkeleton(itemPath, out var skeleton))
            {
                // global이니까 outer는 null
                yield return new TypeExpInfo.Internal(skeleton, new TypeValue.Normal(ModuleName.Internal, appliedItemPath));
            }

            // 3-2. Reference에서 검색, GlobalTypeSkeletons에 이름이 겹치지 않아야 한다.. ModuleInfo들 끼리도 이름이 겹칠 수 있다
            foreach (var typeInfo in context.GetReferenceTypeInfos(itemPath))
                yield return new TypeExpInfo.External(typeInfo, new TypeValue.Normal(typeInfo.GetId().ModuleName, appliedItemPath));
        }
        
        bool EvaluateIdTypeExp(S.IdTypeExp exp, Context context, [NotNullWhen(true)] out TypeExpInfo? outInfo)
        {
            outInfo = null;

            if (exp.Name == "var")
            {
                if (exp.TypeArgs.Length != 0)
                {
                    context.AddError(T0102_IdTypeExp_VarTypeCantApplyTypeArgs, exp, "var는 타입 인자를 가질 수 없습니다");
                    return false;
                }

                outInfo = new TypeExpInfo.NoMember(TypeValue.Var.Instance);
                context.AddTypeValue(exp, TypeValue.Var.Instance);
                return true;
            }
            else if (exp.Name == "void")
            {
                if (exp.TypeArgs.Length != 0)
                {
                    context.AddError(T0101_IdTypeExp_TypeDoesntHaveTypeParams, exp, "void는 타입 인자를 가질 수 없습니다");
                    return false;
                }

                outInfo = new TypeExpInfo.NoMember(TypeValue.Void.Instance);
                context.AddTypeValue(exp, TypeValue.Void.Instance);
                return true;
            }

            // built-in
            else if (exp.Name == "bool")
            {
                return HandleBuiltInType(exp, "bool", ItemIds.Bool, context, out outInfo);
            }
            else if (exp.Name == "int")
            {
                return HandleBuiltInType(exp, "int", ItemIds.Int, context, out outInfo);
            }
            else if (exp.Name == "string")
            {
                return HandleBuiltInType(exp, "string", ItemIds.String, context, out outInfo);
            }

            // 1. TypeVar에서 먼저 검색
            if (context.GetTypeVar(exp.Name, out var typeVar))
            {   
                outInfo = new TypeExpInfo.NoMember(typeVar);
                context.AddTypeValue(exp, typeVar);
                return true;
            }

            // TODO: 2. 현재 This Context에서 검색
            var typeArgs = new List<TypeValue>(exp.TypeArgs.Length);
            foreach (var typeArgExp in exp.TypeArgs)
            {
                if (!EvaluateTypeExp(typeArgExp, context, out var typeArg))
                    return false; // 그냥 진행하면 개수가 맞지 않을 것이므로

                typeArgs.Add(typeArg.GetTypeValue());
            }

            // 3. 전역에서 검색, 
            // TODO: 현재 namespace 상황에 따라서 Namespace.Root대신 인자를 집어넣어야 한다.

            var candidates = new List<TypeExpInfo>();

            var path = new AppliedItemPath(NamespacePath.Root, new AppliedItemPathEntry(exp.Name, string.Empty, typeArgs));

            foreach (var typeExpInfo in GetTypeExpInfos(path, context))
                candidates.Add(typeExpInfo);

            if (candidates.Count == 1)
            {
                outInfo = candidates[0];
                context.AddTypeValue(exp, candidates[0].GetTypeValue());
                return true;
            }
            else if (1 < candidates.Count)
            {
                context.AddError(T0103_IdTypeExp_MultipleTypesOfSameName, exp, $"이름이 같은 {exp} 타입이 여러개 입니다");
                return false;
            }
            else
            {
                context.AddError(T0104_IdTypeExp_TypeNotFound, exp, $"{exp}를 찾지 못했습니다");
                return false;
            }
        }
        
        // X<T>.Y<U, V>
        bool EvaluateMemberTypeExp(S.MemberTypeExp exp, Context context, [NotNullWhen(true)] out TypeExpInfo? outInfo)
        {
            outInfo = null;

            // X<T>
            if (!EvaluateTypeExp(exp.Parent, context, out var parentInfo))
                return false;

            // U, V
            var typeArgs = new List<TypeValue>(exp.TypeArgs.Length);
            foreach (var typeArg in exp.TypeArgs)
            {
                if (!EvaluateTypeExp(typeArg, context, out var typeArgInfo))
                    return false;

                typeArgs.Add(typeArgInfo.GetTypeValue());
            }

            if (parentInfo is TypeExpInfo.NoMember)
            {
                context.AddError(T0201_MemberTypeExp_TypeIsNotNormalType, exp.Parent, "멤버가 있는 타입이 아닙니다");
                return false;
            }

            var memberInfo = parentInfo.GetMemberInfo(exp.MemberName, typeArgs);
            if (memberInfo == null)
            {
                context.AddError(T0202_MemberTypeExp_MemberTypeNotFound, exp, $"{parentInfo.GetTypeValue()}에서 {exp.MemberName}을 찾을 수 없습니다");
                return false;
            }

            outInfo = memberInfo;
            context.AddTypeValue(exp, memberInfo.GetTypeValue());
            return true;
        }
        
        bool EvaluateTypeExp(S.TypeExp exp, Context context, [NotNullWhen(true)] out TypeExpInfo? outInfo)
        {
            if (exp is S.IdTypeExp idExp)
                return EvaluateIdTypeExp(idExp, context, out outInfo);

            else if (exp is S.MemberTypeExp memberExp)
                return EvaluateMemberTypeExp(memberExp, context, out outInfo);

            else 
                throw new NotImplementedException();
        }

        void EvaluateTypeDecl(S.TypeDecl typeDecl, Context context)
        {
            switch(typeDecl)
            {
                case S.EnumDecl enumDecl:
                    EvaluateEnumDecl(enumDecl, context);
                    return;

                case S.StructDecl structDecl:
                    EvaluateStructDecl(structDecl, context);
                    return;

                default:
                    throw new InvalidOperationException();
            }
        }

        void EvaluateEnumDecl(S.EnumDecl enumDecl, Context context)
        {
            var typePath = context.GetTypePath(enumDecl);

            context.ExecInScope(typePath, enumDecl.TypeParams, () =>
            {
                foreach (var elem in enumDecl.Elems)
                {
                    foreach (var param in elem.Params)
                    {
                        // 성공여부와 상관없이 계속 진행한다
                        EvaluateTypeExp(param.Type, context, out var _);
                    }
                }
            });
        }
        
        void EvaluateStructDecl(S.StructDecl structDecl, Context context)
        {
            var typePath = context.GetTypePath(structDecl);
            context.ExecInScope(typePath, structDecl.TypeParams, () =>
            {
                foreach(var elem in structDecl.Elems)
                {
                    switch (elem)
                    {
                        case S.StructDecl.TypeDeclElement typeElem:
                            EvaluateTypeDecl(typeElem.TypeDecl, context);
                            break;

                        case S.StructDecl.FuncDeclElement funcElem:
                            EvaluateStructFuncDeclElem(funcElem, context);
                            break;

                        case S.StructDecl.VarDeclElement varElem:
                            EvaluateTypeExp(varElem.VarType, context, out var _);
                            break;
                    }
                }
            });
        }

        void EvaluateFuncDecl(S.FuncDecl funcDecl, Context context)
        {
            var funcPath = context.GetFuncPath(funcDecl);

            context.ExecInScope(funcPath, funcDecl.TypeParams, () =>
            {
                EvaluateTypeExp(funcDecl.RetType, context, out var _);

                foreach (var param in funcDecl.ParamInfo.Parameters)
                    EvaluateTypeExp(param.Type, context, out var _);

                EvaluateStmt(funcDecl.Body, context);
            });
        }

        void EvaluateStructFuncDeclElem(S.StructDecl.FuncDeclElement funcDecl, Context context)
        {   
            var funcPath = context.GetFuncPath(funcDecl);

            context.ExecInScope(funcPath, funcDecl.TypeParams, () =>
            {
                EvaluateTypeExp(funcDecl.RetType, context, out var _);

                foreach (var param in funcDecl.ParamInfo.Parameters)
                    EvaluateTypeExp(param.Type, context, out var _);


                EvaluateStmt(funcDecl.Body, context);
            });
        }

        void EvaluateVarDecl(S.VarDecl varDecl, Context context)
        {
            EvaluateTypeExp(varDecl.Type, context, out var _);

            foreach (var elem in varDecl.Elems)
                if (elem.InitExp != null)
                    EvaluateExp(elem.InitExp, context);
        }

        void EvaluateStringExpElements(ImmutableArray<S.StringExpElement> elems, Context context)
        {
            foreach (var elem in elems)
            {
                switch (elem)
                {
                    case S.TextStringExpElement textElem: break;
                    case S.ExpStringExpElement expElem: EvaluateExp(expElem.Exp, context); break;
                    default: throw new NotImplementedException();
                }
            }
        }

        void EvaluateTypeExps(ImmutableArray<S.TypeExp> typeExps, Context context)
        {
            foreach (var typeExp in typeExps)
                EvaluateTypeExp(typeExp, context, out var _);
        }

        bool EvaluateTypeExps(ImmutableArray<S.TypeExp> typeExps, Context context, [NotNullWhen(true)] out ImmutableArray<TypeValue> outInfos)
        {
            bool bResult = true;

            var builder = ImmutableArray.CreateBuilder<TypeValue>();            
            foreach(var typeExp in typeExps)
            {
                if (EvaluateTypeExp(typeExp, context, out var typeInfo))
                    builder.Add(typeInfo.GetTypeValue());
                else
                    bResult = false; // 에러 수집을 위해서 바로 종료시키지 않는다
            }

            outInfos = builder.ToImmutable();
            return bResult;
        }

        void EvaluateIdExp(S.IdentifierExp idExp, Context context) 
        {
            EvaluateTypeExps(idExp.TypeArgs, context);
        }

        void EvaluateBoolLiteralExp(S.BoolLiteralExp boolExp, Context context) 
        {

        }

        void EvaluateIntLiteralExp(S.IntLiteralExp intExp, Context context) 
        { 

        }

        void EvaluateStringExp(S.StringExp stringExp, Context context)
        {
            EvaluateStringExpElements(stringExp.Elements, context);
        }

        void EvaluateUnaryOpExp(S.UnaryOpExp unaryOpExp, Context context)
        {
            EvaluateExp(unaryOpExp.Operand, context);
        }

        void EvaluateBinaryOpExp(S.BinaryOpExp binaryOpExp, Context context)
        {
            EvaluateExp(binaryOpExp.Operand0, context);
            EvaluateExp(binaryOpExp.Operand1, context);
        }

        void EvaluateCallExp(S.CallExp callExp, Context context)
        {
            EvaluateExp(callExp.Callable, context);
            EvaluateTypeExps(callExp.TypeArgs, context);

            foreach (var arg in callExp.Args)
                EvaluateExp(arg, context);
        }

        void EvaluateLambdaExp(S.LambdaExp lambdaExp, Context context) 
        {
            foreach (var param in lambdaExp.Params)
                if(param.Type != null)
                    EvaluateTypeExp(param.Type, context, out var _);

            EvaluateStmt(lambdaExp.Body, context);
        }

        void EvaluateIndexerExp(S.IndexerExp exp, Context context)
        {
            EvaluateExp(exp.Object, context);

            EvaluateExp(exp.Index, context);
        }
       
        // 타입이었다면 TypeExpInfo를 리턴한다
        TypeExpInfo? EvaluateMemberExpParent(S.Exp exp, Context context)
        {
            // IdentifierExp, MemberExp일 경우만 따로 처리, 나머지
            if (exp is S.IdentifierExp idExp)
            {
                // 내부에서 에러가 생겼을 때, 더 진행하면 정보가 부정확해지므로 그만둔다
                if (!EvaluateTypeExps(idExp.TypeArgs, context, out var typeArgs))
                    return null; // TODO: 타입이 아니라고 null을 리턴한건지, 에러가 생겨서 null을 리턴한건지 명확하지 않다

                // TODO: NamespacePath.Root 부분은 네임 스페이스 선언 상황에 따라 달라질 수 있다
                var infos = GetTypeExpInfos(
                    new AppliedItemPath(
                        NamespacePath.Root, 
                        new AppliedItemPathEntry(idExp.Value, string.Empty, typeArgs)),
                    context).ToList();

                if (infos.Count == 1)
                    return infos[0];

                if (infos.Count != 0)
                    context.AddError(T0103_IdTypeExp_MultipleTypesOfSameName, exp, $"이름이 같은 {exp} 타입이 여러개 입니다");
                
                return null;
            }            
            else if (exp is S.MemberExp memberExp)
            {
                // NOTICE: EvaluateMemberExp랑 return memberInfo하는 부분 빼고 같다. 수정할때 같이 수정해줘야 한다
                var parentInfo = EvaluateMemberExpParent(memberExp.Parent, context);
                if (parentInfo == null)
                {
                    EvaluateTypeExps(memberExp.MemberTypeArgs, context);
                    return null;
                }
                else
                {
                    EvaluateTypeExps(memberExp.MemberTypeArgs, context, out var typeArgs);
                    var memberInfo = parentInfo.GetMemberInfo(memberExp.MemberName, typeArgs);
                    if (memberInfo != null)
                    {
                        // 타입이었다면 상위에 바로 알려준다
                        return memberInfo;                        
                    }
                    else
                    {
                        // 부모는 타입인데 나는 타입이 아니라면, 부모 타입이 최외각이므로 매핑을 추가한다
                        context.AddTypeValue(memberExp.Parent, parentInfo.GetTypeValue());
                        return null;
                    }
                }
            }
            else
            {
                EvaluateExp(exp, context);
                return null;
            }
        }

        void EvaluateMemberCallExp(S.MemberCallExp memberCallExp, Context context)
        {
            EvaluateMemberExpParent(memberCallExp.Object, context);
            EvaluateTypeExps(memberCallExp.MemberTypeArgs, context);

            foreach (var arg in memberCallExp.Args)
                EvaluateExp(arg, context);
        }

        void EvaluateMemberExp(S.MemberExp memberExp, Context context)
        {
            // NOTICE: EvaluateMemberExpParent의 memberExp 처리 부분이랑 거의 같다. 수정할때 같이 수정해줘야 한다
            var parentInfo = EvaluateMemberExpParent(memberExp.Parent, context);
            if (parentInfo == null) 
            {
                EvaluateTypeExps(memberExp.MemberTypeArgs, context);
            }
            else
            {
                EvaluateTypeExps(memberExp.MemberTypeArgs, context, out var typeArgs);
                var memberInfo = parentInfo.GetMemberInfo(memberExp.MemberName, typeArgs);
                if (memberInfo != null)
                {
                    // 최상위 부분이므로, 타입이었다면 에러 
                    context.AddError(T0203_MemberTypeExp_ExpShouldNotBeType, memberExp, "식이 들어갈 부분이 타입으로 계산되었습니다");
                }
                else
                {
                    context.AddTypeValue(memberExp.Parent, parentInfo.GetTypeValue());
                }
            }            
        }

        void EvaluateListExp(S.ListExp listExp, Context context)
        {
            if (listExp.ElemType != null)
                EvaluateTypeExp(listExp.ElemType, context, out var _);

            foreach (var elem in listExp.Elems)
                EvaluateExp(elem, context);
        }

        void EvaluateExp(S.Exp exp, Context context)
        {
            switch(exp)
            {
                case S.IdentifierExp idExp: EvaluateIdExp(idExp, context); break;
                case S.BoolLiteralExp boolExp: EvaluateBoolLiteralExp(boolExp, context); break;
                case S.IntLiteralExp intExp: EvaluateIntLiteralExp(intExp, context); break;
                case S.StringExp stringExp: EvaluateStringExp(stringExp, context); break;
                case S.UnaryOpExp unaryOpExp: EvaluateUnaryOpExp(unaryOpExp, context); break;
                case S.BinaryOpExp binaryOpExp: EvaluateBinaryOpExp(binaryOpExp, context); break;
                case S.CallExp callExp: EvaluateCallExp(callExp, context); break;
                case S.LambdaExp lambdaExp: EvaluateLambdaExp(lambdaExp, context); break;
                case S.IndexerExp indexerExp: EvaluateIndexerExp(indexerExp, context); break;
                case S.MemberCallExp memberCallExp: EvaluateMemberCallExp(memberCallExp, context); break;
                case S.MemberExp memberExp: EvaluateMemberExp(memberExp, context); break;
                case S.ListExp listExp: EvaluateListExp(listExp, context); break;
                default: throw new NotImplementedException();
            }
        }
        
        void EvaluateCommandStmt(S.CommandStmt cmdStmt, Context context)
        {
            foreach (var cmd in cmdStmt.Commands)
                EvaluateStringExpElements(cmd.Elements, context);
        }

        void EvaluateVarDeclStmt(S.VarDeclStmt varDeclStmt, Context context) 
        {
            EvaluateVarDecl(varDeclStmt.VarDecl, context);
        }

        void EvaluateIfStmt(S.IfStmt ifStmt, Context context)
        {
            EvaluateExp(ifStmt.Cond, context);

            if (ifStmt.TestType != null)
                EvaluateTypeExp(ifStmt.TestType, context, out var _);

            EvaluateStmt(ifStmt.Body, context);

            if (ifStmt.ElseBody != null)
                EvaluateStmt(ifStmt.ElseBody, context);
        }

        void EvaluateForStmtInitializer(S.ForStmtInitializer initializer, Context context)
        {
            switch(initializer)
            {
                case S.ExpForStmtInitializer expInit: EvaluateExp(expInit.Exp, context); break;
                case S.VarDeclForStmtInitializer varDeclInit: EvaluateVarDecl(varDeclInit.VarDecl, context); break;
                default: throw new NotImplementedException();
            }
        }

        void EvaluateForStmt(S.ForStmt forStmt, Context context)
        {
            if (forStmt.Initializer != null)
                EvaluateForStmtInitializer(forStmt.Initializer, context);

            if (forStmt.CondExp != null)
                EvaluateExp(forStmt.CondExp, context);

            if (forStmt.ContinueExp != null)
                EvaluateExp(forStmt.ContinueExp, context);

            EvaluateStmt(forStmt.Body, context);
        }

        void EvaluateContinueStmt(S.ContinueStmt continueStmt, Context context)
        {
        }

        void EvaluateBreakStmt(S.BreakStmt breakStmt, Context context)
        {
        }

        void EvaluateReturnStmt(S.ReturnStmt returnStmt, Context context) 
        {
            if (returnStmt.Value != null)
                EvaluateExp(returnStmt.Value, context);
        }

        void EvaluateBlockStmt(S.BlockStmt blockStmt, Context context)
        {
            foreach (var stmt in blockStmt.Stmts)
                EvaluateStmt(stmt, context);
        }

        void EvaluateExpStmt(S.ExpStmt expStmt, Context context)
        {
            EvaluateExp(expStmt.Exp, context);
        }

        void EvaluateTaskStmt(S.TaskStmt taskStmt, Context context)
        {
            EvaluateStmt(taskStmt.Body, context);
        }

        void EvaluateAwaitStmt(S.AwaitStmt awaitStmt, Context context)
        {
            EvaluateStmt(awaitStmt.Body, context);
        }

        void EvaluateAsyncStmt(S.AsyncStmt asyncStmt, Context context)
        {
            EvaluateStmt(asyncStmt.Body, context);
        }

        void EvaluateForeachStmt(S.ForeachStmt foreachStmt, Context context) 
        {
            EvaluateTypeExp(foreachStmt.Type, context, out var _);
            EvaluateExp(foreachStmt.Iterator, context);
            EvaluateStmt(foreachStmt.Body, context);
        }

        void EvaluateYieldStmt(S.YieldStmt yieldStmt, Context context)
        {
            EvaluateExp(yieldStmt.Value, context);
        }

        void EvaluateStmt(S.Stmt stmt, Context context)
        {
            switch (stmt)
            {
                case S.CommandStmt cmdStmt: EvaluateCommandStmt(cmdStmt, context); break;
                case S.VarDeclStmt varDeclStmt: EvaluateVarDeclStmt(varDeclStmt, context); break;
                case S.IfStmt ifStmt: EvaluateIfStmt(ifStmt, context); break;
                case S.ForStmt forStmt: EvaluateForStmt(forStmt, context); break;                        
                case S.ContinueStmt continueStmt: EvaluateContinueStmt(continueStmt, context); break;
                case S.BreakStmt breakStmt: EvaluateBreakStmt(breakStmt, context); break;
                case S.ReturnStmt returnStmt: EvaluateReturnStmt(returnStmt, context); break;
                case S.BlockStmt blockStmt: EvaluateBlockStmt(blockStmt, context); break;
                case S.BlankStmt blankStmt: break;
                case S.ExpStmt expStmt: EvaluateExpStmt(expStmt, context); break;
                case S.TaskStmt taskStmt: EvaluateTaskStmt(taskStmt, context); break;
                case S.AwaitStmt awaitStmt: EvaluateAwaitStmt(awaitStmt, context); break;
                case S.AsyncStmt asyncStmt: EvaluateAsyncStmt(asyncStmt, context); break;
                case S.ForeachStmt foreachStmt: EvaluateForeachStmt(foreachStmt, context); break;
                case S.YieldStmt yieldStmt: EvaluateYieldStmt(yieldStmt, context); break;                        
                default: throw new NotImplementedException();
            };
        }

        void EvaluateScript(S.Script script, Context context)
        {
            foreach(var elem in script.Elements)
            {
                switch(elem)
                {
                    case S.Script.TypeDeclElement typeDeclElem: EvaluateTypeDecl(typeDeclElem.TypeDecl, context); break;
                    case S.Script.FuncDeclElement funcDeclElem: EvaluateFuncDecl(funcDeclElem.FuncDecl, context);  break;
                    case S.Script.StmtElement stmtDeclElem: EvaluateStmt(stmtDeclElem.Stmt, context);  break;
                    default:
                        throw new NotImplementedException();
                }
            }
        }

        public (SyntaxNodeModuleItemService SyntaxNodeModuleItemService, TypeExpTypeValueService TypeExpTypeValueService)? 
            EvaluateScript(
            S.Script script,
            IEnumerable<IModuleInfo> moduleInfos,
            IErrorCollector errorCollector)
        {
            var collectResult = typeSkeletonCollector.CollectScript(script, errorCollector);
            if (collectResult == null)
                return null;

            var moduleInfoRepo = new ModuleInfoRepository(moduleInfos);
            var itemInfoService = new ItemInfoRepository(moduleInfoRepo);

            var context = new Context(
                itemInfoService,
                collectResult.Value.SyntaxNodeModuleItemService,
                collectResult.Value.TypeSkeletons,
                errorCollector);

            EvaluateScript(script, context);

            if (errorCollector.HasError)
            {
                return null;
            }

            var typeExpTypeValueService = new TypeExpTypeValueService(context.GetTypeValuesByTypeExp(), context.GetTypeValuesByExp());

            return (collectResult.Value.SyntaxNodeModuleItemService, typeExpTypeValueService);
        }

    }
}
