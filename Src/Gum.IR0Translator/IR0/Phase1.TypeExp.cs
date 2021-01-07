using System;
using System.Collections.Generic;
using System.Text;
using S = Gum.Syntax;

namespace Gum.IR0
{
    partial class Phase1
    {
        struct TypeExpResult
        {
            public TypeExpEvaluator.TypeExpInfo Info { get; }
            public TypeExpResult(TypeExpEvaluator.TypeExpInfo info) { Info = info; }
        }

        TypeExpResult VisitIdTypeExp(S.IdTypeExp typeExp)
        {
            var typeArgs = VisitTypeArgExps(typeExp.TypeArgs);

            var typeExpInfo = typeExpEvaluator.EvaluateIdTypeExp(typeExp, typeArgs);
            return new TypeExpResult(typeExpInfo);
        }

        // X<T>.Y<U, V>
        TypeExpResult VisitMemberTypeExp(S.MemberTypeExp exp)
        {
            // X<T>
            var parentResult = VisitTypeExp(exp.Parent);

            // U, V            
            var typeArgs = VisitTypeArgExps(exp.TypeArgs);

            var expInfo = typeExpEvaluator.EvaluateMemberTypeExp(exp, parentResult.Info, typeArgs);
            return new TypeExpResult(expInfo);
        }

        TypeExpResult VisitTypeExp(S.TypeExp exp)
        {            
            if (exp is S.IdTypeExp idExp)
                return VisitIdTypeExp(idExp);

            else if (exp is S.MemberTypeExp memberExp)
                return VisitMemberTypeExp(memberExp);

            else
                throw new InvalidOperationException();            
        }

        void VisitTypeExpNoResult(S.TypeExp exp)
        {
            try
            {
                VisitTypeExp(exp);
            }
            catch(TypeExpEvaluatorException ex)
            {
                errorCollector.Add(ex.Erro);
            }
        }

    }
}
