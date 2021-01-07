using Gum.CompileTime;
using Gum.Infra;
using Gum.Syntax;
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
    class TypeExpEvaluatorException : Exception 
    {
        public AnalyzeError Error { get; }
        public TypeExpEvaluatorException(AnalyzeError error)
        {
            Error = error;
        }
    }

    // TypeExp를 TypeValue로 바꿔서 저장합니다.
    internal partial class TypeExpEvaluator
    {
        ItemInfoRepository itemInfoRepo;
        SkeletonRepository skelRepo;

        Dictionary<S.TypeExp, TypeValue> typeValuesByTypeExp;
        Dictionary<S.Exp, TypeValue> typeValuesByExp;
        ImmutableDictionary<string, TypeValue.TypeVar> typeEnv;

        public TypeExpEvaluator(ItemInfoRepository itemInfoRepo, SkeletonRepository skelRepo)
        {
            this.itemInfoRepo = itemInfoRepo;
            this.skelRepo = skelRepo;

            typeValuesByTypeExp = new Dictionary<S.TypeExp, TypeValue>();
            typeValuesByExp = new Dictionary<S.Exp, TypeValue>();
            typeEnv = ImmutableDictionary<string, TypeValue.TypeVar>.Empty;
        }

        [DoesNotReturn]
        public void Throw(AnalyzeErrorCode code, S.ISyntaxNode node, string msg)
        {
            throw new TypeExpEvaluatorException(new AnalyzeError(code, node, msg));
        }

        public void AddTypeValue(S.TypeExp exp, TypeValue typeValue)
        {
            typeValuesByTypeExp.Add(exp, typeValue);
        }        

        public void AddTypeValue(S.Exp exp, TypeValue typeValue)
        {
            typeValuesByExp.Add(exp, typeValue);
        }

        public bool GetTypeVar(string name, [NotNullWhen(true)] out TypeValue.TypeVar? typeValue)
        {
            return typeEnv.TryGetValue(name, out typeValue);
        }

        public void ExecInScope(ItemPath itemPath, IEnumerable<string> typeParams, Action action)
        {
            var prevTypeEnv = typeEnv;

            int i = 0;
            foreach (var typeParam in typeParams)
            {
                typeEnv = typeEnv.SetItem(typeParam, new TypeValue.TypeVar(itemPath.OuterEntries.Length, i, typeParam));
                i++;
            }

            try
            {
                action();
            }
            finally
            {
                typeEnv = prevTypeEnv;
            }
        }

        private TypeExpInfo HandleBuiltInType(S.IdTypeExp exp, string name, ItemId itemId)
        {
            if (exp.TypeArgs.Length != 0)
                Throw(T0101_IdTypeExp_TypeDoesntHaveTypeParams, exp, $"{name}은 타입 인자를 가질 수 없습니다");
            
            AddTypeValue(exp, new TypeValue.Normal(itemId)); // NOTICE: no type args
            return new TypeExpInfo.NoMember(new TypeValue.Normal(itemId));
        }
        
        private IEnumerable<TypeExpInfo> GetTypeExpInfos(AppliedItemPath appliedItemPath)
        {
            var itemPath = appliedItemPath.GetItemPath();
            var typeSkel = skelRepo.GetSkeleton(itemPath) as Skeleton.Type;

            if (typeSkel != null)
            {
                yield return new TypeExpInfo.Internal(typeSkel, new TypeValue.Normal(ModuleName.Internal, appliedItemPath));
            }

            // 3-2. Reference에서 검색, GlobalTypeSkeletons에 이름이 겹치지 않아야 한다.. ModuleInfo들 끼리도 이름이 겹칠 수 있다
            foreach (var typeInfo in itemInfoRepo.GetTypes(itemPath))
                yield return new TypeExpInfo.External(typeInfo, new TypeValue.Normal(typeInfo.GetId().ModuleName, appliedItemPath));
        }
        
        public TypeExpInfo EvaluateIdTypeExp(S.IdTypeExp exp, IEnumerable<TypeValue> typeArgs)
        {
            if (exp.Name == "var")
            {
                if (exp.TypeArgs.Length != 0)
                    Throw(T0102_IdTypeExp_VarTypeCantApplyTypeArgs, exp, "var는 타입 인자를 가질 수 없습니다");
                
                AddTypeValue(exp, TypeValue.Var.Instance);
                return new TypeExpInfo.NoMember(TypeValue.Var.Instance);
            }
            else if (exp.Name == "void")
            {
                if (exp.TypeArgs.Length != 0)
                {
                    Throw(T0101_IdTypeExp_TypeDoesntHaveTypeParams, exp, "void는 타입 인자를 가질 수 없습니다");
                }
                
                AddTypeValue(exp, TypeValue.Void.Instance);
                return new TypeExpInfo.NoMember(TypeValue.Void.Instance);
            }

            // built-in
            else if (exp.Name == "bool")
            {
                return HandleBuiltInType(exp, "bool", ItemIds.Bool);
            }
            else if (exp.Name == "int")
            {
                return HandleBuiltInType(exp, "int", ItemIds.Int);
            }
            else if (exp.Name == "string")
            {
                return HandleBuiltInType(exp, "string", ItemIds.String);
            }

            // 1. TypeVar에서 먼저 검색
            if (GetTypeVar(exp.Name, out var typeVar))
            {
                if (exp.TypeArgs.Length != 0)
                    Throw(T0105_IdTypeExp_TypeVarCantApplyTypeArgs, exp, "타입 변수는 타입 인자를 가질 수 없습니다");                    

                AddTypeValue(exp, typeVar);
                return new TypeExpInfo.NoMember(typeVar);
            }

            // TODO: 2. 현재 This Context에서 검색
            
            // 3. 전역에서 검색, 
            // TODO: 현재 namespace 상황에 따라서 Namespace.Root대신 인자를 집어넣어야 한다.

            var candidates = new List<TypeExpInfo>();
            var path = new AppliedItemPath(NamespacePath.Root, new AppliedItemPathEntry(exp.Name, string.Empty, typeArgs));

            foreach (var typeExpInfo in GetTypeExpInfos(path))
                candidates.Add(typeExpInfo);

            if (candidates.Count == 1)
            {
                AddTypeValue(exp, candidates[0].GetTypeValue());
                return candidates[0];
            }
            else if (1 < candidates.Count)
            {
                Throw(T0103_IdTypeExp_MultipleTypesOfSameName, exp, $"이름이 같은 {exp} 타입이 여러개 입니다");
            }
            else
            {
                Throw(T0104_IdTypeExp_TypeNotFound, exp, $"{exp}를 찾지 못했습니다");
            }
        }

        // X<T>.Y<U, V>
        public TypeExpInfo EvaluateMemberTypeExp(
            S.MemberTypeExp exp,
            TypeExpInfo parentInfo,
            IEnumerable<TypeValue> typeArgs)
        {
            if (parentInfo is TypeExpInfo.NoMember)
                Throw(T0201_MemberTypeExp_TypeIsNotNormalType, exp.Parent, "멤버가 있는 타입이 아닙니다");

            var memberInfo = parentInfo.GetMemberInfo(exp.MemberName, typeArgs);
            if (memberInfo == null)
                Throw(T0202_MemberTypeExp_MemberTypeNotFound, exp, $"{parentInfo.GetTypeValue()}에서 {exp.MemberName}을 찾을 수 없습니다");
            
            AddTypeValue(exp, memberInfo.GetTypeValue());
            return memberInfo;
        }        

        // 타입을 찾을수 없다면 null, 
        // 타입이 하나 있다면 그것을,
        // 타입이 여러개 있다면 exception
        public TypeExpInfo? EvaluateMemberExpIdExpParent(S.IdentifierExp idExp, IEnumerable<TypeValue> typeArgs)
        {
            // TODO: NamespacePath.Root 부분은 네임 스페이스 선언 상황에 따라 달라질 수 있다
            var infos = GetTypeExpInfos(
                new AppliedItemPath(
                    NamespacePath.Root,
                    new AppliedItemPathEntry(idExp.Value, string.Empty, typeArgs))
            ).ToList();

            if (infos.Count == 0) return null;
            if (infos.Count == 1) return infos[0];

            Throw(T0103_IdTypeExp_MultipleTypesOfSameName, idExp, $"이름이 같은 {idExp} 타입이 여러개 입니다");
        }

        public TypeExpInfo? EvaluateMemberExpMemberExpParent(S.MemberExp exp, TypeExpInfo? parentInfo, IEnumerable<TypeValue> typeArgs)
        {
            if (parentInfo == null) return null;
            
            var memberInfo = parentInfo.GetMemberInfo(exp.MemberName, typeArgs);
            if (memberInfo != null)
            {
                // 타입이었다면 상위에 바로 알려준다
                return memberInfo;
            }
            else
            {
                // 부모는 타입인데, 나는 타입이 아니라면, 부모 타입이 최외각이므로 매핑을 추가한다
                AddTypeValue(exp.Parent, parentInfo.GetTypeValue());
                return null;
            }
        }
        
        public void EvaluateMemberExp(S.MemberExp memberExp, TypeExpInfo? parentInfo, IEnumerable<TypeValue> typeArgs)
        {
            if (parentInfo == null) return;

            // NOTICE: EvaluateMemberExpParent의 memberExp 처리 부분이랑 거의 같다. 수정할때 같이 수정해줘야 한다
            var memberInfo = parentInfo.GetMemberInfo(memberExp.MemberName, typeArgs);

            // 최상위 부분이므로, 타입이었다면 에러 
            if (memberInfo != null)
                Throw(T0203_MemberTypeExp_ExpShouldNotBeType, memberExp, "식이 들어갈 부분이 타입으로 계산되었습니다");

            AddTypeValue(memberExp.Parent, parentInfo.GetTypeValue());
        }

        public TypeExpTypeValueService MakeTypeExpTypeValueService()
        {
            return new TypeExpTypeValueService(typeValuesByTypeExp.ToImmutableDictionary(), typeValuesByExp.ToImmutableDictionary());
        }
    }
}
