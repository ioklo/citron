using System;
using System.Collections.Generic;
using System.Text;
using S = Gum.Syntax;

namespace Gum.IR0
{
    partial class Phase1
    {
        struct ExpResult
        {
            // NOTICE: Info 가 null인 것은 Invalid함이 아니다
            public TypeExpEvaluator.TypeExpInfo? Info { get; }
            public ExpResult(TypeExpEvaluator.TypeExpInfo? info) { Info = info; }
        }

        void VisitIdExp(S.IdentifierExp idExp)
        {
            VisitTypeArgExpsNoResult(idExp.TypeArgs);
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
            VisitTypeArgExpsNoResult(callExp.TypeArgs);

            foreach (var arg in callExp.Args)
                VisitExp(arg);
        }

        void VisitLambdaExp(S.LambdaExp lambdaExp)
        {
            foreach (var param in lambdaExp.Params)
                if (param.Type != null)
                    VisitTypeExp(param.Type);

            VisitStmt(lambdaExp.Body);
        }

        void VisitIndexerExp(S.IndexerExp exp)
        {
            VisitExp(exp.Object);
            VisitExp(exp.Index);
        }

        void VisitMemberExpParentNoResult(S.Exp exp)
        {
            try
            {
                VisitMemberExpParent(exp);
            }
            catch(TypeExpEvaluatorException ex)
            {
                errorCollector.Add(ex.Error);
            }
        }
        
        // 타입이었다면 TypeExpInfo를 리턴한다
        ExpResult VisitMemberExpParent(S.Exp exp)
        {
            // IdentifierExp, MemberExp일 경우만 따로 처리, 나머지
            if (exp is S.IdentifierExp idExp)
            {
                var typeArgs = VisitTypeArgExps(idExp.TypeArgs);

                // 지금은, Evalute-함수가 null을 리턴해도 해당 Type이 없다는 의미이지, identifier가 잘못되었단 뜻이 아니므로 값을 담고 리턴해야 한다
                // 하지만 조금있다가 제작할 idResolver에서는 identifier의 종류를 리턴하게 되므로 null을 리턴할 수 없게 된다
                var info = typeExpEvaluator.EvaluateMemberExpIdExpParent(idExp, typeArgs);
                return new ExpResult(info);
            }
            else if (exp is S.MemberExp memberExp)
            {
                // NOTICE: VisitMemberExp랑 return memberInfo하는 부분 빼고 같다. 수정할때 같이 수정해줘야 한다
                var parentResult = VisitMemberExpParent(memberExp.Parent);
                var typeArgs = VisitTypeArgExps(memberExp.MemberTypeArgs);

                var info = typeExpEvaluator.EvaluateMemberExpMemberExpParent(memberExp, parentResult.Info, typeArgs);
                return new ExpResult(info);
            }
            else
            {
                VisitExp(exp);
                return new ExpResult(null);
            }
        }

        void VisitMemberCallExp(S.MemberCallExp memberCallExp)
        {
            VisitMemberExpParentNoResult(memberCallExp.Object);
            VisitTypeArgExpsNoResult(memberCallExp.MemberTypeArgs);

            foreach (var arg in memberCallExp.Args)
                VisitExp(arg);
        }

        void VisitMemberExp(S.MemberExp memberExp)
        {
            // NOTICE: VisitMemberExpParent의 memberExp 처리 부분이랑 거의 같다. 수정할때 같이 수정해줘야 한다
            var parentResult = VisitMemberExpParent(memberExp.Parent);
            var typeArgs = VisitTypeArgExps(memberExp.MemberTypeArgs);

            typeExpEvaluator.EvaluateMemberExp(memberExp, parentResult.Info, typeArgs);
        }

        void VisitListExp(S.ListExp listExp)
        {
            if (listExp.ElemType != null)
                VisitTypeExp(listExp.ElemType);

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
                    default: throw new InvalidOperationException();
                }
            }
            catch(TypeExpEvaluatorException ex)
            {
                errorCollector.Add(ex.Error);
            }
        }
    }
}
