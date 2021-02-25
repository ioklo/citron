using Gum.Infra;
using System;
using System.Collections.Generic;
using System.Text;
using S = Gum.Syntax;

namespace Gum.IR0Translator
{
    partial class TypeExpEvaluator
    {   
        void VisitIdExp(S.IdentifierExp idExp)
        {
            VisitTypeArgExpsOuterMost(idExp.TypeArgs);
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
                VisitExp(arg);
        }

        void VisitLambdaExp(S.LambdaExp lambdaExp)
        {
            foreach (var param in lambdaExp.Params)
                if (param.Type != null)
                    VisitTypeExpOuterMost(param.Type);

            VisitStmt(lambdaExp.Body);
        }

        void VisitIndexerExp(S.IndexerExp exp)
        {
            VisitExp(exp.Object);
            VisitExp(exp.Index);
        }
        
        void VisitMemberCallExp(S.MemberCallExp memberCallExp)
        {
            VisitExp(memberCallExp.Parent);
            VisitTypeArgExpsOuterMost(memberCallExp.MemberTypeArgs);

            foreach (var arg in memberCallExp.Args)
                VisitExp(arg);
        }

        // T<int, short>.x
        void VisitMemberExp(S.MemberExp memberExp)
        {
            VisitExp(memberExp.Parent);
            VisitTypeArgExpsOuterMost(memberExp.MemberTypeArgs);
        }

        void VisitListExp(S.ListExp listExp)
        {
            if (listExp.ElemType != null)
                VisitTypeExpOuterMost(listExp.ElemType);

            foreach (var elem in listExp.Elems)
                VisitExp(elem);
        }

        void VisitExp(S.Exp exp)
        {
            try
            {
                switch (exp)
                {
                    case S.IdentifierExp idExp: VisitIdExp(idExp); break;
                    case S.BoolLiteralExp boolExp: VisitBoolLiteralExp(boolExp); break;
                    case S.IntLiteralExp intExp: VisitIntLiteralExp(intExp); break;
                    case S.StringExp stringExp: VisitStringExp(stringExp); break;
                    case S.UnaryOpExp unaryOpExp: VisitUnaryOpExp(unaryOpExp); break;
                    case S.BinaryOpExp binaryOpExp: VisitBinaryOpExp(binaryOpExp); break;
                    case S.CallExp callExp: VisitCallExp(callExp); break;
                    case S.LambdaExp lambdaExp: VisitLambdaExp(lambdaExp); break;
                    case S.IndexerExp indexerExp: VisitIndexerExp(indexerExp); break;
                    case S.MemberCallExp memberCallExp: VisitMemberCallExp(memberCallExp); break;
                    case S.MemberExp memberExp: VisitMemberExp(memberExp); break;
                    case S.ListExp listExp: VisitListExp(listExp); break;
                    default: throw new UnreachableCodeException();
                }
            }
            catch (TypeExpEvaluatorFatalException)
            {

            }
        }
    }
}
