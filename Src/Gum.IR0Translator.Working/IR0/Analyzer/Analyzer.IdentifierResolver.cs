using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using static Gum.IR0.Analyzer.Misc;

using S = Gum.Syntax;
using M = Gum.CompileTime;
using Gum.Misc;

namespace Gum.IR0
{
    partial class Analyzer
    {
        IdentifierResult ResolveIdentifierIdExp(S.IdentifierExp idExp, TypeValue? hintTypeValue)
        {
            var typeArgs = GetTypeValues(idExp.TypeArgs, context);
            var resolver = new IdentifierResolver(idExp.Value, typeArgs, hintTypeValue, context);
            return resolver.Resolve();
        }

        IdentifierResult ResolveIdentifierMemberExpExpParent(Exp parent, TypeValue parentType, string memberName, ImmutableArray<TypeValue> typeArgs, TypeValue? hintType)
        {
            // Assert.Debug(TypeValue.ConstraintMemberNameMapsEitherVarOrFuncs);

            // memberName
            if (typeArgs.Length == 0)
            {
                // e.y
                ClassValue cv = parentType.GetClassValue();
                
                if (cv == null)
                {
                    cv.GetMemberVar(memberName);
                }

                var memberVar = parentType.GetMemberVar(memberName);
                {
                    new ExpIdentifierResult(new ClassMemberExp(parent, memberName), memberType);
                }

                // e.i
                EnumElemTypeValue ee;
            }

            // e.F
            foreach(var memberFunc in parentType.GetMemberFuncs(memberName))
            {

            }

            // TODO: hint 매칭
        }

        IdentifierResult ResolveIdentifierMemberExp(S.MemberExp memberExp, TypeValue? hintType)
        {
            var typeArgs = GetTypeValues(memberExp.MemberTypeArgs, context);

            // 힌트가 없다. EnumElemIdentifierResult가 나올 수 없다
            var parentResult = ResolveIdentifier(memberExp.Parent, null);

            switch (parentResult)
            {
                case NotFoundErrorIdentifierResult _:
                case MultipleCandiatesErrorIdentifierResult _:
                    return parentResult;

                case ExpIdentifierResult expResult:
                    return ResolveIdentifierMemberExpExpParent(expResult.Exp, expResult.TypeValue, memberExp.MemberName, typeArgs, hintType);

                case TypeIdentifierResult typeResult:
                    return ResolveIdentifierMemberExpTypeParent(typeResult.TypeValue);

                case FuncIdentifierResult funcResult:
                    // 함수는 멤버변수를 가질 수 없습니다
                    return FuncCantHaveMemberErrorIdentifierResult.Instance;

                case EnumElemIdentifierResult enumElemResult:
                    // 힌트 없이 EnumElem이 나올 수가 없다
                    throw new UnreachableCodeException();
            }
        }

        IdentifierResult ResolveIdentifier(S.Exp exp, TypeValue? hintTypeValue)
        {
            if (exp is S.IdentifierExp idExp)
            {
                return ResolveIdentifier(idExp, hintTypeValue);
            }
            else if (exp is S.MemberExp memberExp)
            {
                return ResolveIdentifier(memberExp, hintTypeValue);
            }
            else
            {
                var expResult = AnalyzeExp(exp, hintTypeValue);
                return new ExpIdentifierResult(expResult.Exp, expResult.TypeValue);
            }
        }

        struct IdentifierResolver
        {
            string idName;
            ImmutableArray<TypeValue> typeArgs;
            TypeValue? hintTypeValue;
            Context context;

            public IdentifierResolver(string idName, ImmutableArray<TypeValue> typeArgs, TypeValue? hintTypeValue, Context context)
            {
                this.idName = idName;
                this.typeArgs = typeArgs;
                this.hintTypeValue = hintTypeValue;
                this.context = context;
            }

            ExpIdentifierResult? GetLocalVarOutsideLambdaInfo()
            {
                // 지역 스코프에는 변수만 있고, 함수, 타입은 없으므로 이름이 겹치는 것이 있는지 검사하지 않아도 된다
                if (typeArgs.Length != 0) return null;

                var varInfo = context.GetLocalVarOutsideLambda(idName);
                if (varInfo == null) return null;

                return new ExpIdentifierResult(new LocalVarExp(varInfo.Value.Name), varInfo.Value.TypeValue);
            }

            ExpIdentifierResult? GetLocalVarInfo()
            {
                // 지역 스코프에는 변수만 있고, 함수, 타입은 없으므로 이름이 겹치는 것이 있는지 검사하지 않아도 된다
                if (typeArgs.Length != 0) return null;

                var varInfo = context.GetLocalVar(idName);
                if (varInfo == null) return null;

                return new ExpIdentifierResult(new LocalVarExp(varInfo.Value.Name), varInfo.Value.TypeValue);
            }

            ExpIdentifierResult? GetThisMemberInfo()
            {
                // TODO: implementation
                return null;
            }

            ExpIdentifierResult? GetInternalGlobalVarInfo()
            {
                if (typeArgs.Length != 0) return null;

                var varInfo = context.GetInternalGlobalVarInfo(idName);
                if (varInfo == null) return null;

                return new ExpIdentifierResult(new PrivateGlobalVarExp(varInfo.Name.ToString()), varInfo.TypeValue);
            }


            TypeIdentifierResult? GetInternalGlobalTypeInfo(M.NamespacePath namespacePath)
            {
                var typeValue = context.GetInternalGlobalType(namespacePath, idName, typeArgs);
                if (typeValue == null) return null;

                return new TypeIdentifierResult(typeValue);
            }

            IEnumerable<FuncIdentifierResult> GetInternalGlobalFuncInfos(M.NamespacePath namespacePath)
            {
                return context.GetInternalGlobalFuncInfos(namespacePath, idName, typeArgs).Select(fv => new FuncIdentifierResult(fv));
            }

            IEnumerable<TypeIdentifierResult> GetExternalGlobalTypeInfos(M.NamespacePath namespacePath)
            {
                throw new NotImplementedException();
            }

            IEnumerable<FuncIdentifierResult> GetExternalGlobalFuncInfos(M.NamespacePath namespacePath)
            {
                throw new NotImplementedException();
            }

            IdentifierResult? GetGlobalInfo(string idName, ImmutableArray<TypeValue> typeArgs, TypeValue? hintTypeValue)
            {
                var candidates = new List<IdentifierResult>();

                var curNamespacePath = M.NamespacePath.Root;

                var internalGlobalTypeInfo = GetInternalGlobalTypeInfo(curNamespacePath);
                if (internalGlobalTypeInfo != null)
                    candidates.Add(internalGlobalTypeInfo);

                var internalGlobalFuncInfos = GetInternalGlobalFuncInfos(curNamespacePath);
                candidates.AddRange(internalGlobalFuncInfos);

                var externalGlobalTypeInfos = GetExternalGlobalTypeInfos(curNamespacePath);
                candidates.AddRange(externalGlobalTypeInfos);

                var externalGlobalFuncInfos = GetExternalGlobalFuncInfos(curNamespacePath);
                candidates.AddRange(externalGlobalFuncInfos);

                // 힌트가 E고, First가 써져 있으면 E.First를 검색한다
                // enum 힌트 사용, typeArgs가 있으면 지나간다
                if (hintTypeValue is NormalTypeValue hintNTV)
                {
                    // First<T> 같은건 없기 때문에 없을때만 검색한다
                    if (typeArgs.Length == 0)
                    {
                        var enumInfo = hintNTV.GetEnumInfo();
                        if (enumInfo != null)
                        {
                            if (enumInfo.GetElemInfo(idName, out var elemInfo))
                            {
                                var idInfo = new EnumElemIdentifierResult(hintNTV, elemInfo.Value);
                                candidates.Add(idInfo);
                            }
                        }
                    }
                }

                if (candidates.Count == 1)
                    return candidates[0];

                if (2 <= candidates.Count)
                    return MultipleCandiatesErrorIdentifierResult.Instance;

                return null;
            }
            public IdentifierResult Resolve()
            {
                // 0. local 변수, local 변수에서는 힌트를 쓸 일이 없다
                var localVarInfo = GetLocalVarInfo();
                if (localVarInfo != null) return localVarInfo;

                // 1. 람다 바깥의 local 변수
                var localOutsideInfo = GetLocalVarOutsideLambdaInfo();
                if (localOutsideInfo != null) return localOutsideInfo;

                // 2. thisType의 {{instance, static} * {변수, 함수}}, 타입. 아직 지원 안함
                // 힌트는 오버로딩 함수 선택에 쓰일수도 있고,
                // 힌트가 thisType안의 enum인 경우 elem을 선택할 수도 있다
                var thisMemberInfo = GetThisMemberInfo();
                if (thisMemberInfo != null) return thisMemberInfo;

                // 3. internal global 'variable', 변수이므로 힌트를 쓸 일이 없다
                var internalGlobalVarInfo = GetInternalGlobalVarInfo();
                if (internalGlobalVarInfo != null) return internalGlobalVarInfo;

                // 4. 네임스페이스 -> 바깥 네임스페이스 -> module global, 함수, 타입, 
                // 오버로딩 함수 선택, hint가 global enum인 경우, elem선택
                var externalGlobalInfo = GetGlobalInfo(idName, typeArgs, hintTypeValue);
                if (externalGlobalInfo != null) return externalGlobalInfo;

                return NotFoundErrorIdentifierResult.Instance;
            }
        }
    }
}
