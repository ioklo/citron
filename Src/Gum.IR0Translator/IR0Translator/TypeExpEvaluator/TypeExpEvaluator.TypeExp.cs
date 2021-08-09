using System;
using System.Collections.Generic;
using System.Text;
using Gum.Infra;

using static Gum.IR0Translator.AnalyzeErrorCode;
using S = Gum.Syntax;
using M = Gum.CompileTime;
using System.Linq;
using Gum.Collections;

namespace Gum.IR0Translator
{
    partial class TypeExpEvaluator
    {
        TypeExpResult HandleBuiltInType(S.IdTypeExp exp, M.Type mtype, TypeExpInfoKind kind)
        {
            if (exp.TypeArgs.Length != 0)
                throw new NotImplementedException();
            
            var typeExpInfo = new MTypeTypeExpInfo(mtype, kind);
            return new NoMemberTypeExpResult(typeExpInfo); // TODO: 일단 NoMember로 놔두는데, 멤버가 있으니
        }

        TypeExpResult VisitIdTypeExp(S.IdTypeExp typeExp)
        {
            if (typeExp.Name == "var")
            {
                if (typeExp.TypeArgs.Length != 0)
                    Throw(T0102_IdTypeExp_VarTypeCantApplyTypeArgs, typeExp, "var는 타입 인자를 가질 수 없습니다");

                return NoMemberTypeExpResult.Var;
            }
            else if (typeExp.Name == "void")
            {
                if (typeExp.TypeArgs.Length != 0)
                    Throw(T0101_IdTypeExp_TypeDoesntHaveTypeParams, typeExp, "void는 타입 인자를 가질 수 없습니다");

                return NoMemberTypeExpResult.Void;
            }

            // built-in
            else if (typeExp.Name == "bool")
            {
                return HandleBuiltInType(typeExp, MTypes.Bool, TypeExpInfoKind.Struct);
            }
            else if (typeExp.Name == "int")
            {
                return HandleBuiltInType(typeExp, MTypes.Int, TypeExpInfoKind.Struct);
            }
            else if (typeExp.Name == "string")
            {
                return HandleBuiltInType(typeExp, MTypes.String, TypeExpInfoKind.Class);
            }

            // 1. TypeVar에서 먼저 검색
            var typeVar = GetTypeVar(typeExp.Name);
            if (typeVar != null)
            {
                if (typeExp.TypeArgs.Length != 0)
                    Throw(T0105_IdTypeExp_TypeVarCantApplyTypeArgs, typeExp, "타입 변수는 타입 인자를 가질 수 없습니다");

                return NoMemberTypeExpResult.TypeVar(typeVar);
            }

            // TODO: 2. 현재 This Context에서 검색

            // 3. 전역에서 검색, 
            // TODO: 현재 namespace 상황에 따라서 Namespace.Root대신 인자를 집어넣어야 한다.
            var typeArgs = VisitTypeArgExps(typeExp.TypeArgs);
            var candidates = GetTypeExpInfos(M.NamespacePath.Root, typeExp.Name, typeArgs).ToList();

            if (candidates.Count == 1)
            {
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
        TypeExpResult VisitMemberTypeExp(S.MemberTypeExp exp)
        {
            // X<T>
            var parentResult = VisitTypeExp(exp.Parent);

            // U, V            
            var typeArgs = VisitTypeArgExps(exp.TypeArgs);            

            if (parentResult is NoMemberTypeExpResult)
                Throw(T0201_MemberTypeExp_TypeIsNotNormalType, exp.Parent, "멤버가 있는 타입이 아닙니다");

            var memberResult = parentResult.GetMemberInfo(exp.MemberName, typeArgs);
            if (memberResult == null)
                Throw(T0202_MemberTypeExp_MemberTypeNotFound, exp, $"{parentResult.TypeExpInfo}에서 {exp.MemberName}을 찾을 수 없습니다");

            return memberResult;
        }

        TypeExpResult VisitTypeExp(S.TypeExp exp)
        {
            switch (exp)
            {
                case S.IdTypeExp idExp: return VisitIdTypeExp(idExp);
                case S.MemberTypeExp memberExp: return VisitMemberTypeExp(memberExp);
                default:
                    throw new UnreachableCodeException();
            }
        }

        // VisitTypeExp와 다른 점은 실행 후 (TypeExp => TypeExpInfo) 정보를 추가
        void VisitTypeExpOuterMost(S.TypeExp exp)
        {
            try
            {
                var result = VisitTypeExp(exp);
                AddInfo(exp, result.TypeExpInfo);
            }
            catch(TypeExpEvaluatorFatalException)
            {
            }
        }
    }
}
