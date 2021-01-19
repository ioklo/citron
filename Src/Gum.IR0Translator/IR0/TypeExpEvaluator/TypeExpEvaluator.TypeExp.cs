using System;
using System.Collections.Generic;
using System.Text;
using S = Gum.Syntax;
using static Gum.IR0.AnalyzeErrorCode;
using Gum.CompileTime;
using Gum.Misc;

namespace Gum.IR0
{
    partial class TypeExpEvaluator
    {
        TypeExpInfo VisitIdTypeExp(S.IdTypeExp typeExp)
        {
            var typeArgs = VisitTypeArgExps(typeExp.TypeArgs);
            if (typeExp.Name == "var")
            {
                if (typeExp.TypeArgs.Length != 0)
                    Throw(T0102_IdTypeExp_VarTypeCantApplyTypeArgs, typeExp, "var는 타입 인자를 가질 수 없습니다");

                AddTypeValue(typeExp, TypeValue.Var.Instance);
                return new TypeExpInfo.NoMember(TypeValue.Var.Instance);
            }
            else if (typeExp.Name == "void")
            {
                if (typeExp.TypeArgs.Length != 0)
                {
                    Throw(T0101_IdTypeExp_TypeDoesntHaveTypeParams, typeExp, "void는 타입 인자를 가질 수 없습니다");
                }

                AddTypeValue(typeExp, TypeValue.Void.Instance);
                return new TypeExpInfo.NoMember(TypeValue.Void.Instance);
            }

            // built-in
            else if (typeExp.Name == "bool")
            {
                return HandleBuiltInType(typeExp, "bool", ItemIds.Bool);
            }
            else if (typeExp.Name == "int")
            {
                return HandleBuiltInType(typeExp, "int", ItemIds.Int);
            }
            else if (typeExp.Name == "string")
            {
                return HandleBuiltInType(typeExp, "string", ItemIds.String);
            }

            // 1. TypeVar에서 먼저 검색
            var typeVar = GetTypeVar(typeExp.Name);
            if (typeVar != null)
            {
                if (typeExp.TypeArgs.Length != 0)
                    Throw(T0105_IdTypeExp_TypeVarCantApplyTypeArgs, typeExp, "타입 변수는 타입 인자를 가질 수 없습니다");

                AddTypeValue(typeExp, typeVar);
                return new TypeExpInfo.NoMember(typeVar);
            }

            // TODO: 2. 현재 This Context에서 검색

            // 3. 전역에서 검색, 
            // TODO: 현재 namespace 상황에 따라서 Namespace.Root대신 인자를 집어넣어야 한다.

            var candidates = new List<TypeExpInfo>();
            var path = new AppliedItemPath(NamespacePath.Root, new AppliedItemPathEntry(typeExp.Name, string.Empty, typeArgs));

            foreach (var typeExpInfo in GetTypeExpInfos(path))
                candidates.Add(typeExpInfo);

            if (candidates.Count == 1)
            {
                AddTypeValue(typeExp, candidates[0].GetTypeValue());
                return candidates[0];
            }
            else if (1 < candidates.Count)
            {
                Throw(T0103_IdTypeExp_MultipleTypesOfSameName, typeExp, $"이름이 같은 {typeExp} 타입이 여러개 입니다");
            }
            else
            {
                Throw(T0104_IdTypeExp_TypeNotFound, typeExp, $"{typeExp}를 찾지 못했습니다");
            }

            throw new UnreachableCodeException();
        }

        // X<T>.Y<U, V>
        TypeExpInfo VisitMemberTypeExp(S.MemberTypeExp exp)
        {
            // X<T>
            var parentInfo = VisitTypeExp(exp.Parent);

            // U, V            
            var typeArgs = VisitTypeArgExps(exp.TypeArgs);            

            if (parentInfo is TypeExpInfo.NoMember)
                Throw(T0201_MemberTypeExp_TypeIsNotNormalType, exp.Parent, "멤버가 있는 타입이 아닙니다");

            var memberInfo = parentInfo.GetMemberInfo(exp.MemberName, typeArgs);
            if (memberInfo == null)
                Throw(T0202_MemberTypeExp_MemberTypeNotFound, exp, $"{parentInfo.GetTypeValue()}에서 {exp.MemberName}을 찾을 수 없습니다");

            AddTypeValue(exp, memberInfo.GetTypeValue());
            return memberInfo;
        }

        TypeExpInfo VisitTypeExp(S.TypeExp exp)
        {            
            if (exp is S.IdTypeExp idExp)
                return VisitIdTypeExp(idExp);

            else if (exp is S.MemberTypeExp memberExp)
                return VisitMemberTypeExp(memberExp);

            throw new UnreachableCodeException();
        }

        void VisitTypeExpNoThrow(S.TypeExp exp)
        {
            try
            {
                VisitTypeExp(exp);
            }
            catch(TypeExpEvaluatorFatalException)
            {
            }
        }
    }
}
