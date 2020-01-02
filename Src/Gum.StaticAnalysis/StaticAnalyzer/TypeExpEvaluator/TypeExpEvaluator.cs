using Gum.CompileTime;
using Gum.Infra;
using Gum.Syntax;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection.Emit;
using System.Text;

namespace Gum.StaticAnalysis
{
    // TypeExp를 TypeValue로 바꿔서 저장합니다.
    public partial class TypeExpEvaluator
    {
        TypeSkeletonCollector typeSkeletonCollector;

        public TypeExpEvaluator(TypeSkeletonCollector typeSkeletonCollector)
        {
            this.typeSkeletonCollector = typeSkeletonCollector;
        }

        bool EvaluateIdTypeExp(IdTypeExp exp, Context context, [NotNullWhen(returnValue: true)] out TypeValue? outTypeValue)
        {
            outTypeValue = null;

            if (exp.Name == "var")
            {
                if (exp.TypeArgs.Length != 0)
                {
                    context.AddError(exp, "var는 타입 인자를 가질 수 없습니다");
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
                    context.AddError(exp, "var는 타입 인자를 가질 수 없습니다");
                    return false;
                }

                outTypeValue = TypeValue.MakeVoid();
                context.AddTypeValue(exp, outTypeValue);
                return true;
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
                context.AddError(exp, $"이름이 같은 {exp} 타입이 여러개 입니다");
                return false;
            }
            else
            {
                context.AddError(exp, $"{exp}를 찾지 못했습니다");
                return false;
            }
        }
        
        bool EvaluateMemberTypeExp(MemberTypeExp exp, Context context, [NotNullWhen(returnValue: true)] out TypeValue? typeValue)
        {
            typeValue = null;

            if (!EvaluateTypeExp(exp.Parent, context, out var parentTypeValue))
                return false;

            var parentNTV = parentTypeValue as TypeValue.Normal;
            if (parentNTV == null)
            {
                context.AddError(exp.Parent, "멤버가 있는 타입이 아닙니다");
                return false;
            }

            var typeArgsBuilder = ImmutableArray.CreateBuilder<TypeValue>(exp.TypeArgs.Length);
            foreach (var typeArgExp in exp.TypeArgs)
            {
                if (!EvaluateTypeExp(typeArgExp, context, out var typeArg))
                    return false;

                typeArgsBuilder.Add(typeArg);
            }

            if (!GetMemberTypeValue(context, parentNTV, exp.MemberName, typeArgsBuilder.MoveToImmutable(), out typeValue))
            {
                context.AddError(exp, $"{parentTypeValue}에서 {exp.MemberName}을 찾을 수 없습니다");
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
            ImmutableArray<TypeValue> typeArgs, 
            [NotNullWhen(returnValue: true)] out TypeValue? outTypeValue)
        {
            outTypeValue = null;
            
            if (!(parent is TypeValue.Normal normalParent))
                return false;
            
            if (!context.GetSkeleton(normalParent.TypeId, out var parentSkeleton))
                return false;

            if (parentSkeleton.GetMemberTypeId(memberName, typeArgs.Length, out var childTypeId))
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

        bool EvaluateTypeExp(TypeExp exp, Context context, [NotNullWhen(returnValue:true)] out TypeValue? typeValue)
        {
            if (exp is IdTypeExp idExp)
                return EvaluateIdTypeExp(idExp, context, out typeValue);

            else if (exp is MemberTypeExp memberExp)
                return EvaluateMemberTypeExp(memberExp, context, out typeValue);

            else 
                throw new NotImplementedException();
        }

        void EvaluateEnumDecl(EnumDecl enumDecl, Context context)
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

        void EvaluateFuncDecl(FuncDecl funcDecl, Context context)
        {
            EvaluateTypeExp(funcDecl.RetType, context, out var _);

            foreach(var param in funcDecl.Params)
                EvaluateTypeExp(param.Type, context, out var _);

            var funcId = context.GetFuncId(funcDecl);

            context.ExecInScope(funcId, funcDecl.TypeParams, () =>
            {   
                EvaluateStmt(funcDecl.Body, context);
            });
        }

        void EvaluateVarDecl(VarDecl varDecl, Context context)
        {
            EvaluateTypeExp(varDecl.Type, context, out var _);

            foreach (var elem in varDecl.Elems)
                if (elem.InitExp != null)
                    EvaluateExp(elem.InitExp, context);
        }

        void EvaluateStringExpElements(ImmutableArray<StringExpElement> elems, Context context)
        {
            foreach (var elem in elems)
            {
                switch (elem)
                {
                    case TextStringExpElement textElem: break;
                    case ExpStringExpElement expElem: EvaluateExp(expElem.Exp, context); break;
                    default: throw new NotImplementedException();
                }
            }
        }

        void EvaluateTypeExps(ImmutableArray<TypeExp> typeExps, Context context)
        {
            foreach (var typeExp in typeExps)
                EvaluateTypeExp(typeExp, context, out var _);
        }

        void EvaluateIdExp(IdentifierExp idExp, Context context) 
        {
            EvaluateTypeExps(idExp.TypeArgs, context);
        }

        void EvaluateBoolLiteralExp(BoolLiteralExp boolExp, Context context) 
        {

        }

        void EvaluateIntLiteralExp(IntLiteralExp intExp, Context context) 
        { 

        }

        void EvaluateStringExp(StringExp stringExp, Context context)
        {
            EvaluateStringExpElements(stringExp.Elements, context);
        }

        void EvaluateUnaryOpExp(UnaryOpExp unaryOpExp, Context context)
        {
            EvaluateExp(unaryOpExp.Operand, context);
        }

        void EvaluateBinaryOpExp(BinaryOpExp binaryOpExp, Context context)
        {
            EvaluateExp(binaryOpExp.Operand0, context);
            EvaluateExp(binaryOpExp.Operand1, context);
        }

        void EvaluateCallExp(CallExp callExp, Context context)
        {
            EvaluateExp(callExp.Callable, context);
            EvaluateTypeExps(callExp.TypeArgs, context);

            foreach (var arg in callExp.Args)
                EvaluateExp(arg, context);
        }

        void EvaluateLambdaExp(LambdaExp lambdaExp, Context context) 
        {
            foreach (var param in lambdaExp.Params)
                if(param.Type != null)
                    EvaluateTypeExp(param.Type, context, out var _);

            EvaluateStmt(lambdaExp.Body, context);
        }

        void EvaluateIndexerExp(IndexerExp exp, Context context)
        {
            EvaluateExp(exp.Object, context);

            EvaluateExp(exp.Index, context);
        }

        void EvaluateMemberCallExp(MemberCallExp memberCallExp, Context context)
        {
            EvaluateExp(memberCallExp.Object, context);
            EvaluateTypeExps(memberCallExp.MemberTypeArgs, context);

            foreach (var arg in memberCallExp.Args)
                EvaluateExp(arg, context);
        }

        void EvaluateMemberExp(MemberExp memberExp, Context context)
        {
            EvaluateExp(memberExp.Object, context);

            EvaluateTypeExps(memberExp.MemberTypeArgs, context);
        }

        void EvaluateListExp(ListExp listExp, Context context)
        {
            foreach (var elem in listExp.Elems)
                EvaluateExp(elem, context);
        }

        void EvaluateExp(Exp exp, Context context)
        {
            switch(exp)
            {
                case IdentifierExp idExp: EvaluateIdExp(idExp, context); break;
                case BoolLiteralExp boolExp: EvaluateBoolLiteralExp(boolExp, context); break;
                case IntLiteralExp intExp: EvaluateIntLiteralExp(intExp, context); break;
                case StringExp stringExp: EvaluateStringExp(stringExp, context); break;
                case UnaryOpExp unaryOpExp: EvaluateUnaryOpExp(unaryOpExp, context); break;
                case BinaryOpExp binaryOpExp: EvaluateBinaryOpExp(binaryOpExp, context); break;
                case CallExp callExp: EvaluateCallExp(callExp, context); break;
                case LambdaExp lambdaExp: EvaluateLambdaExp(lambdaExp, context); break;
                case IndexerExp indexerExp: EvaluateIndexerExp(indexerExp, context); break;
                case MemberCallExp memberCallExp: EvaluateMemberCallExp(memberCallExp, context); break;
                case MemberExp memberExp: EvaluateMemberExp(memberExp, context); break;
                case ListExp listExp: EvaluateListExp(listExp, context); break;
                default: throw new NotImplementedException();
            }
        }
        
        void EvaluateCommandStmt(CommandStmt cmdStmt, Context context)
        {
            foreach (var cmd in cmdStmt.Commands)
                EvaluateStringExpElements(cmd.Elements, context);
        }

        void EvaluateVarDeclStmt(VarDeclStmt varDeclStmt, Context context) 
        {
            EvaluateVarDecl(varDeclStmt.VarDecl, context);
        }

        void EvaluateIfStmt(IfStmt ifStmt, Context context)
        {
            EvaluateExp(ifStmt.Cond, context);

            if (ifStmt.TestType != null)
                EvaluateTypeExp(ifStmt.TestType, context, out var _);

            EvaluateStmt(ifStmt.Body, context);

            if (ifStmt.ElseBody != null)
                EvaluateStmt(ifStmt.ElseBody, context);
        }

        void EvaluateForStmtInitializer(ForStmtInitializer initializer, Context context)
        {
            switch(initializer)
            {
                case ExpForStmtInitializer expInit: EvaluateExp(expInit.Exp, context); break;
                case VarDeclForStmtInitializer varDeclInit: EvaluateVarDecl(varDeclInit.VarDecl, context); break;
                default: throw new NotImplementedException();
            }
        }

        void EvaluateForStmt(ForStmt forStmt, Context context)
        {
            if (forStmt.Initializer != null)
                EvaluateForStmtInitializer(forStmt.Initializer, context);

            if (forStmt.CondExp != null)
                EvaluateExp(forStmt.CondExp, context);

            if (forStmt.ContinueExp != null)
                EvaluateExp(forStmt.ContinueExp, context);

            EvaluateStmt(forStmt.Body, context);
        }

        void EvaluateContinueStmt(ContinueStmt continueStmt, Context context)
        {
        }

        void EvaluateBreakStmt(BreakStmt breakStmt, Context context)
        {
        }

        void EvaluateReturnStmt(ReturnStmt returnStmt, Context context) 
        {
            if (returnStmt.Value != null)
                EvaluateExp(returnStmt.Value, context);
        }

        void EvaluateBlockStmt(BlockStmt blockStmt, Context context)
        {
            foreach (var stmt in blockStmt.Stmts)
                EvaluateStmt(stmt, context);
        }

        void EvaluateExpStmt(ExpStmt expStmt, Context context)
        {
            EvaluateExp(expStmt.Exp, context);
        }

        void EvaluateTaskStmt(TaskStmt taskStmt, Context context)
        {
            EvaluateStmt(taskStmt.Body, context);
        }

        void EvaluateAwaitStmt(AwaitStmt awaitStmt, Context context)
        {
            EvaluateStmt(awaitStmt.Body, context);
        }

        void EvaluateAsyncStmt(AsyncStmt asyncStmt, Context context)
        {
            EvaluateStmt(asyncStmt.Body, context);
        }

        void EvaluateForeachStmt(ForeachStmt foreachStmt, Context context) 
        {
            EvaluateTypeExp(foreachStmt.Type, context, out var _);
            EvaluateExp(foreachStmt.Obj, context);
            EvaluateStmt(foreachStmt.Body, context);
        }

        void EvaluateYieldStmt(YieldStmt yieldStmt, Context context)
        {
            EvaluateExp(yieldStmt.Value, context);
        }

        void EvaluateStmt(Stmt stmt, Context context)
        {
            switch (stmt)
            {
                case CommandStmt cmdStmt: EvaluateCommandStmt(cmdStmt, context); break;
                case VarDeclStmt varDeclStmt: EvaluateVarDeclStmt(varDeclStmt, context); break;
                case IfStmt ifStmt: EvaluateIfStmt(ifStmt, context); break;
                case ForStmt forStmt: EvaluateForStmt(forStmt, context); break;                        
                case ContinueStmt continueStmt: EvaluateContinueStmt(continueStmt, context); break;
                case BreakStmt breakStmt: EvaluateBreakStmt(breakStmt, context); break;
                case ReturnStmt returnStmt: EvaluateReturnStmt(returnStmt, context); break;
                case BlockStmt blockStmt: EvaluateBlockStmt(blockStmt, context); break;
                case BlankStmt blankStmt: break;
                case ExpStmt expStmt: EvaluateExpStmt(expStmt, context); break;
                case TaskStmt taskStmt: EvaluateTaskStmt(taskStmt, context); break;
                case AwaitStmt awaitStmt: EvaluateAwaitStmt(awaitStmt, context); break;
                case AsyncStmt asyncStmt: EvaluateAsyncStmt(asyncStmt, context); break;
                case ForeachStmt foreachStmt: EvaluateForeachStmt(foreachStmt, context); break;
                case YieldStmt yieldStmt: EvaluateYieldStmt(yieldStmt, context); break;                        
                default: throw new NotImplementedException();
            };
        }

        void EvaluateScript(Script script, Context context)
        {
            foreach(var elem in script.Elements)
            {
                switch(elem)
                {
                    case EnumDeclScriptElement enumDeclElem: EvaluateEnumDecl(enumDeclElem.EnumDecl, context); break;
                    case FuncDeclScriptElement funcDeclElem: EvaluateFuncDecl(funcDeclElem.FuncDecl, context);  break;
                    case StmtScriptElement stmtDeclElem: EvaluateStmt(stmtDeclElem.Stmt, context);  break;
                    default:
                        throw new NotImplementedException();
                }
            }
        }

        public (SyntaxNodeModuleItemService SyntaxNodeModuleItemService, TypeExpTypeValueService TypeExpTypeValueService)? 
            EvaluateScript(
            Script script,
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
