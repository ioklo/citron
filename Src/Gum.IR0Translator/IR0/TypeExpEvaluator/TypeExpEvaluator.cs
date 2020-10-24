using Gum.CompileTime;
using Gum.Infra;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using static Gum.IR0.AnalyzeErrorCode;

using S = Gum.Syntax;

namespace Gum.IR0
{
    // TypeExp를 TypeValue로 바꿔서 저장합니다.
    partial class TypeExpEvaluator
    {
        TypeSkeletonCollector typeSkeletonCollector;

        public TypeExpEvaluator(TypeSkeletonCollector typeSkeletonCollector)
        {
            this.typeSkeletonCollector = typeSkeletonCollector;
        }

        private bool HandleBuiltInType(S.IdTypeExp exp, string name, Context context, [NotNullWhen(true)] out TypeValue? outTypeValue)
        {
            if (exp.TypeArgs.Length != 0)
            {
                context.AddError(T0101_IdTypeExp_TypeDoesntHaveTypeParams, exp, $"{name}은 타입 인자를 가질 수 없습니다");
                outTypeValue = null;
                return false;
            }

            outTypeValue = TypeValue.MakeNormal(ModuleItemId.Make(name));
            context.AddTypeValue(exp, outTypeValue);
            return true;
        }

        bool EvaluateIdTypeExp(S.IdTypeExp exp, Context context, [NotNullWhen(true)] out TypeValue? outTypeValue)
        {
            outTypeValue = null;

            if (exp.Name == "var")
            {
                if (exp.TypeArgs.Length != 0)
                {
                    context.AddError(T0102_IdTypeExp_VarTypeCantApplyTypeArgs, exp, "var는 타입 인자를 가질 수 없습니다");
                    return false;
                }

                outTypeValue = TypeValue.MakeVar();
                context.AddTypeValue(exp, outTypeValue);
                return true;
            }
            else if (exp.Name == "void")
            {
                if (exp.TypeArgs.Length != 0)
                {
                    context.AddError(T0101_IdTypeExp_TypeDoesntHaveTypeParams, exp, "void는 타입 인자를 가질 수 없습니다");
                    return false;
                }

                outTypeValue = TypeValue.MakeVoid();
                context.AddTypeValue(exp, outTypeValue);
                return true;
            }

            // built-in
            else if (exp.Name == "bool")
            {
                return HandleBuiltInType(exp, "bool", context, out outTypeValue);
            }
            else if (exp.Name == "int")
            {
                return HandleBuiltInType(exp, "int", context, out outTypeValue);
            }
            else if (exp.Name == "string")
            {
                return HandleBuiltInType(exp, "string", context, out outTypeValue);
            }

            // 1. TypeVar에서 먼저 검색
            if (context.GetTypeVar(exp.Name, out var typeVar))
            {   
                outTypeValue = typeVar;
                context.AddTypeValue(exp, typeVar);
                return true;
            }

            // TODO: 2. 현재 This Context에서 검색

            var typeArgs = new List<TypeValue>(exp.TypeArgs.Length);
            foreach (var typeArgExp in exp.TypeArgs)
            {
                if (!EvaluateTypeExp(typeArgExp, context, out var typeArg))
                    return false; // 그냥 진행하면 개수가 맞지 않을 것이므로

                typeArgs.Add(typeArg);
            }

            var typeArgList = TypeArgumentList.Make(null, typeArgs);
            var itemId = ModuleItemId.Make(exp.Name, typeArgs.Count);

            // 3-1. GlobalSkeleton에서 검색
            List <TypeValue> candidates = new List<TypeValue>();
            if (context.GetSkeleton(itemId, out var skeleton))
            {
                // global이니까 outer는 null
                candidates.Add(TypeValue.MakeNormal(skeleton.TypeId, typeArgList));
            }

            // 3-2. Reference에서 검색, GlobalTypeSkeletons에 이름이 겹치지 않아야 한다.. ModuleInfo들 끼리도 이름이 겹칠 수 있다
            foreach (var type in context.GetTypeInfos(itemId))
                candidates.Add(TypeValue.MakeNormal(type.TypeId, typeArgList));

            if (candidates.Count == 1)
            {
                outTypeValue = candidates[0];
                context.AddTypeValue(exp, outTypeValue);
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
        
        bool EvaluateMemberTypeExp(S.MemberTypeExp exp, Context context, [NotNullWhen(true)] out TypeValue? typeValue)
        {
            typeValue = null;

            if (!EvaluateTypeExp(exp.Parent, context, out var parentTypeValue))
                return false;

            var parentNTV = parentTypeValue as TypeValue.Normal;
            if (parentNTV == null)
            {
                context.AddError(T0201_MemberTypeExp_TypeIsNotNormalType, exp.Parent, "멤버가 있는 타입이 아닙니다");
                return false;
            }

            var typeArgs = new List<TypeValue>(exp.TypeArgs.Length);
            foreach (var typeArgExp in exp.TypeArgs)
            {
                if (!EvaluateTypeExp(typeArgExp, context, out var typeArg))
                    return false;

                typeArgs.Add(typeArg);
            }

            if (!GetMemberTypeValue(context, parentNTV, exp.MemberName, typeArgs, out typeValue))
            {
                context.AddError(T0202_MemberTypeExp_MemberTypeNotFound, exp, $"{parentTypeValue}에서 {exp.MemberName}을 찾을 수 없습니다");
                return false;
            }

            context.AddTypeValue(exp, typeValue);
            return true;
        }

        // Error를 만들지 않습니다
        private bool GetMemberTypeValue(
            Context context,
            TypeValue.Normal parent, 
            string memberName, 
            IReadOnlyCollection<TypeValue> typeArgs, 
            [NotNullWhen(true)] out TypeValue? outTypeValue)
        {
            outTypeValue = null;
            
            if (!(parent is TypeValue.Normal normalParent))
                return false;
            
            if (!context.GetSkeleton(normalParent.TypeId, out var parentSkeleton))
                return false;

            if (parentSkeleton.GetMemberTypeId(memberName, typeArgs.Count, out var childTypeId))
            {
                outTypeValue = TypeValue.MakeNormal(childTypeId, TypeArgumentList.Make(parent.TypeArgList, typeArgs));
                return true;
            }
            else if (parentSkeleton.ContainsEnumElem(memberName))
            {
                outTypeValue = TypeValue.MakeEnumElem(parent, memberName);
                return true;
            }

            return false;
        }

        bool EvaluateTypeExp(S.TypeExp exp, Context context, [NotNullWhen(true)] out TypeValue? typeValue)
        {
            if (exp is S.IdTypeExp idExp)
                return EvaluateIdTypeExp(idExp, context, out typeValue);

            else if (exp is S.MemberTypeExp memberExp)
                return EvaluateMemberTypeExp(memberExp, context, out typeValue);

            else 
                throw new NotImplementedException();
        }

        void EvaluateEnumDecl(S.EnumDecl enumDecl, Context context)
        {
            var typeId = context.GetTypeId(enumDecl);

            context.ExecInScope(typeId, enumDecl.TypeParams, () =>
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

        void EvaluateFuncDecl(S.FuncDecl funcDecl, Context context)
        {
            EvaluateTypeExp(funcDecl.RetType, context, out var _);

            foreach(var param in funcDecl.ParamInfo.Parameters)
                EvaluateTypeExp(param.Type, context, out var _);

            var funcId = context.GetFuncId(funcDecl);

            context.ExecInScope(funcId, funcDecl.TypeParams, () =>
            {   
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

        void EvaluateMemberCallExp(S.MemberCallExp memberCallExp, Context context)
        {
            EvaluateExp(memberCallExp.Object, context);
            EvaluateTypeExps(memberCallExp.MemberTypeArgs, context);

            foreach (var arg in memberCallExp.Args)
                EvaluateExp(arg, context);
        }

        void EvaluateMemberExp(S.MemberExp memberExp, Context context)
        {
            EvaluateExp(memberExp.Object, context);

            EvaluateTypeExps(memberExp.MemberTypeArgs, context);
        }

        void EvaluateListExp(S.ListExp listExp, Context context)
        {
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
            EvaluateExp(foreachStmt.Obj, context);
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
                    case S.Script.EnumDeclElement enumDeclElem: EvaluateEnumDecl(enumDeclElem.EnumDecl, context); break;
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

            var moduleInfoService = new ModuleInfoService(moduleInfos);

            var context = new Context(
                moduleInfoService,
                collectResult.Value.SyntaxNodeModuleItemService,
                collectResult.Value.TypeSkeletons,
                errorCollector);

            EvaluateScript(script, context);

            if (errorCollector.HasError)
            {
                return null;
            }

            var typeExpTypeValueService = new TypeExpTypeValueService(context.GetTypeValuesByTypeExp());

            return (collectResult.Value.SyntaxNodeModuleItemService, typeExpTypeValueService);
        }

    }
}
