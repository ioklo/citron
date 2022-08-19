using Citron.Collections;
using Citron.Infra;
using System.Collections.Generic;
using System.Text;
using Citron.Syntax;

namespace Citron.Analysis
{
    public partial class TypeExpEvaluator
    {   
        struct ExpVisitor
        {
            LocalContext localContext;
            GlobalContext globalContext;

            ExpVisitor(LocalContext typeEnv, GlobalContext globalContext)
            {
                this.localContext = typeEnv;
                this.globalContext = globalContext;
            }

            public static void Visit(Exp exp, LocalContext typeEnv, GlobalContext context)
            {
                var visitor = new ExpVisitor(typeEnv, context);
                visitor.VisitExp(exp);
            }

            public static void Visit(ImmutableArray<StringExpElement> elems, LocalContext typeEnv, GlobalContext context)
            {
                var visitor = new ExpVisitor(typeEnv, context);
                visitor.VisitStringExpElements(elems);
            }

            void VisitIdExp(IdentifierExp idExp)
            {
                TypeExpVisitor.Visit(idExp.TypeArgs, localContext, globalContext);
            }

            void VisitNullLiteralExp(NullLiteralExp nullExp)
            {
            }

            void VisitBoolLiteralExp(BoolLiteralExp boolExp)
            {
            }

            void VisitIntLiteralExp(IntLiteralExp intExp)
            {
            }

            void VisitStringExp(StringExp stringExp)
            {
                VisitStringExpElements(stringExp.Elements);
            }

            void VisitUnaryOpExp(UnaryOpExp unaryOpExp)
            {
                VisitExp(unaryOpExp.Operand);
            }

            void VisitBinaryOpExp(BinaryOpExp binaryOpExp)
            {
                VisitExp(binaryOpExp.Operand0);
                VisitExp(binaryOpExp.Operand1);
            }

            void VisitCallExp(CallExp callExp)
            {
                VisitExp(callExp.Callable);

                foreach (var arg in callExp.Args)
                    VisitArgument(arg);
            }

            void VisitLambdaExp(LambdaExp lambdaExp)
            {   
                foreach (var param in lambdaExp.Params)
                {
                    if (param.Type != null)
                    {
                        TypeExpVisitor.Visit(param.Type, localContext, globalContext);
                    }
                }

                StmtVisitor.Visit(lambdaExp.Body, localContext, globalContext);
            }

            void VisitIndexerExp(IndexerExp exp)
            {
                VisitExp(exp.Object);
                VisitExp(exp.Index);
            }

            // T<int, short>.x
            void VisitMemberExp(MemberExp memberExp)
            {
                VisitExp(memberExp.Parent);
                TypeExpVisitor.Visit(memberExp.MemberTypeArgs, localContext, globalContext);
            }

            void VisitListExp(ListExp listExp)
            {
                if (listExp.ElemType != null)
                    TypeExpVisitor.Visit(listExp.ElemType, localContext, globalContext);

                foreach (var elem in listExp.Elems)
                    VisitExp(elem);
            }

            void VisitNewExp(NewExp newExp)
            {
                TypeExpVisitor.Visit(newExp.Type, localContext, globalContext);

                foreach (var arg in newExp.Args)
                    VisitArgument(arg);
            }

            void VisitExp(Exp exp)
            {
                try
                {
                    switch (exp)
                    {
                        case IdentifierExp idExp: VisitIdExp(idExp); break;
                        case NullLiteralExp nullExp: VisitNullLiteralExp(nullExp); break;
                        case BoolLiteralExp boolExp: VisitBoolLiteralExp(boolExp); break;
                        case IntLiteralExp intExp: VisitIntLiteralExp(intExp); break;
                        case StringExp stringExp: VisitStringExp(stringExp); break;
                        case UnaryOpExp unaryOpExp: VisitUnaryOpExp(unaryOpExp); break;
                        case BinaryOpExp binaryOpExp: VisitBinaryOpExp(binaryOpExp); break;
                        case CallExp callExp: VisitCallExp(callExp); break;
                        case LambdaExp lambdaExp: VisitLambdaExp(lambdaExp); break;
                        case IndexerExp indexerExp: VisitIndexerExp(indexerExp); break;
                        case MemberExp memberExp: VisitMemberExp(memberExp); break;
                        case ListExp listExp: VisitListExp(listExp); break;
                        case NewExp newExp: VisitNewExp(newExp); break;
                        default: throw new UnreachableCodeException();
                    }
                }
                catch (FatalException)
                {

                }
            }

            void VisitStringExpElements(ImmutableArray<StringExpElement> elems)
            {
                foreach (var elem in elems)
                {
                    switch (elem)
                    {
                        case TextStringExpElement _: break;
                        case ExpStringExpElement expElem: VisitExp(expElem.Exp); break;
                        default: throw new UnreachableCodeException();
                    }
                }
            }

            void VisitArgument(Argument arg)
            {
                switch (arg)
                {
                    case Argument.Normal normalArg:
                        VisitExp(normalArg.Exp);
                        break;

                    case Argument.Params paramsArg:
                        VisitExp(paramsArg.Exp);
                        break;

                    case Argument.Ref refArg:
                        VisitExp(refArg.Exp);
                        break;
                }
            }
        }        
    }
}
