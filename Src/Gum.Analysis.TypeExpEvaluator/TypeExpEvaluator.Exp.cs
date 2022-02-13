using Gum.Collections;
using Gum.Infra;
using System;
using System.Collections.Generic;
using System.Text;
using S = Gum.Syntax;
using M = Gum.CompileTime;

namespace Citron.Analysis
{
    public partial class TypeExpEvaluator
    {   
        struct TypeEnv
        {
            public static readonly TypeEnv Empty = new TypeEnv(default);

            ImmutableDictionary<string, Func<S.TypeExp, InternalTypeVarTypeExpInfo>> dict;

            TypeEnv(ImmutableDictionary<string, Func<S.TypeExp, InternalTypeVarTypeExpInfo>> dict)
            {
                this.dict = dict;
            }

            public InternalTypeVarTypeExpInfo? TryMakeTypeVar(string name, S.TypeExp typeExp)
            {
                if (dict.TryGetValue(name, out var constructor))
                    return constructor.Invoke(typeExp);

                return null;
            }

            public void Add(M.DeclSymbolId declId, string typeParam, int index)
            {
                dict = dict.SetItem(typeParam, typeExp => InternalTypeVarTypeExpInfo.Make(declId, typeParam, index, typeExp));
            }
        }

        struct ExpVisitor
        {
            TypeEnv typeEnv;
            Context context;

            ExpVisitor(TypeEnv typeEnv, Context context)
            {
                this.typeEnv = typeEnv;
                this.context = context;
            }

            public static void Visit(S.Exp exp, TypeEnv typeEnv, Context context)
            {
                var visitor = new ExpVisitor(typeEnv, context);
                visitor.VisitExp(exp);
            }

            public static void Visit(ImmutableArray<S.StringExpElement> elems, TypeEnv typeEnv, Context context)
            {
                var visitor = new ExpVisitor(typeEnv, context);
                visitor.VisitStringExpElements(elems);
            }

            void VisitIdExp(S.IdentifierExp idExp)
            {
                TypeExpVisitor.Visit(idExp.TypeArgs, typeEnv, context);
            }

            void VisitNullLiteralExp(S.NullLiteralExp nullExp)
            {
            }

            void VisitBoolLiteralExp(S.BoolLiteralExp boolExp)
            {
            }

            void VisitIntLiteralExp(S.IntLiteralExp intExp)
            {
            }

            void VisitStringExp(S.StringExp stringExp)
            {
                VisitStringExpElements(stringExp.Elements);
            }

            void VisitUnaryOpExp(S.UnaryOpExp unaryOpExp)
            {
                VisitExp(unaryOpExp.Operand);
            }

            void VisitBinaryOpExp(S.BinaryOpExp binaryOpExp)
            {
                VisitExp(binaryOpExp.Operand0);
                VisitExp(binaryOpExp.Operand1);
            }

            void VisitCallExp(S.CallExp callExp)
            {
                VisitExp(callExp.Callable);

                foreach (var arg in callExp.Args)
                    VisitArgument(arg);
            }

            void VisitLambdaExp(S.LambdaExp lambdaExp)
            {   
                foreach (var param in lambdaExp.Params)
                {
                    if (param.Type != null)
                    {
                        TypeExpVisitor.Visit(param.Type, typeEnv, context);
                    }
                }

                StmtVisitor.Visit(lambdaExp.Body, typeEnv, context);
            }

            void VisitIndexerExp(S.IndexerExp exp)
            {
                VisitExp(exp.Object);
                VisitExp(exp.Index);
            }

            // T<int, short>.x
            void VisitMemberExp(S.MemberExp memberExp)
            {
                VisitExp(memberExp.Parent);
                TypeExpVisitor.Visit(memberExp.MemberTypeArgs, typeEnv, context);
            }

            void VisitListExp(S.ListExp listExp)
            {
                if (listExp.ElemType != null)
                    TypeExpVisitor.Visit(listExp.ElemType, typeEnv, context);

                foreach (var elem in listExp.Elems)
                    VisitExp(elem);
            }

            void VisitNewExp(S.NewExp newExp)
            {
                TypeExpVisitor.Visit(newExp.Type, typeEnv, context);

                foreach (var arg in newExp.Args)
                    VisitArgument(arg);
            }

            void VisitExp(S.Exp exp)
            {
                try
                {
                    switch (exp)
                    {
                        case S.IdentifierExp idExp: VisitIdExp(idExp); break;
                        case S.NullLiteralExp nullExp: VisitNullLiteralExp(nullExp); break;
                        case S.BoolLiteralExp boolExp: VisitBoolLiteralExp(boolExp); break;
                        case S.IntLiteralExp intExp: VisitIntLiteralExp(intExp); break;
                        case S.StringExp stringExp: VisitStringExp(stringExp); break;
                        case S.UnaryOpExp unaryOpExp: VisitUnaryOpExp(unaryOpExp); break;
                        case S.BinaryOpExp binaryOpExp: VisitBinaryOpExp(binaryOpExp); break;
                        case S.CallExp callExp: VisitCallExp(callExp); break;
                        case S.LambdaExp lambdaExp: VisitLambdaExp(lambdaExp); break;
                        case S.IndexerExp indexerExp: VisitIndexerExp(indexerExp); break;
                        case S.MemberExp memberExp: VisitMemberExp(memberExp); break;
                        case S.ListExp listExp: VisitListExp(listExp); break;
                        case S.NewExp newExp: VisitNewExp(newExp); break;
                        default: throw new UnreachableCodeException();
                    }
                }
                catch (FatalException)
                {

                }
            }

            void VisitStringExpElements(ImmutableArray<S.StringExpElement> elems)
            {
                foreach (var elem in elems)
                {
                    switch (elem)
                    {
                        case S.TextStringExpElement _: break;
                        case S.ExpStringExpElement expElem: VisitExp(expElem.Exp); break;
                        default: throw new UnreachableCodeException();
                    }
                }
            }

            void VisitArgument(S.Argument arg)
            {
                switch (arg)
                {
                    case S.Argument.Normal normalArg:
                        VisitExp(normalArg.Exp);
                        break;

                    case S.Argument.Params paramsArg:
                        VisitExp(paramsArg.Exp);
                        break;

                    case S.Argument.Ref refArg:
                        VisitExp(refArg.Exp);
                        break;
                }
            }
        }        
    }
}
