using System;
using System.Collections.Generic;
using Gum.Collections;
using System.Linq;

using S = Gum.Syntax;
using M = Gum.CompileTime;
using R = Gum.IR0;
using Gum.Infra;
using System.Diagnostics;

namespace Gum.IR0Translator
{
    partial class Analyzer
    {
        IdentifierResult ResolveIdentifierIdExp(S.IdentifierExp idExp, ResolveHint resolveHint)
        {
            var typeArgs = GetTypeValues(idExp.TypeArgs, context);
            var resolver = new IdExpIdentifierResolver(idExp.Value, typeArgs, resolveHint, context);
            return resolver.Resolve();
        }        

        R.Loc BuildMemberLoc(R.Loc parent, TypeValue parentType, string memberName)
        {
            switch(parentType)
            {
                case ClassTypeValue _: return new R.ClassMemberLoc(parent, memberName);
                case StructTypeValue _: return new R.StructMemberLoc(parent, memberName);
                case EnumElemTypeValue _: return new R.EnumMemberLoc(parent, memberName);
            }

            throw new UnreachableCodeException();
        }

        static ErrorIdentifierResult ToErrorIdentifierResult(ErrorItemResult errorResult)
        {
            switch(errorResult)
            {
                case MultipleCandidatesErrorItemResult:
                    return MultipleCandiatesErrorIdentifierResult.Instance;

                case VarWithTypeArgErrorItemResult:
                    return VarWithTypeArgErrorIdentifierResult.Instance;
            }

            throw new UnreachableCodeException();
        }
        
        // e.x 꼴
        IdentifierResult ResolveIdentifierMemberExpLocParent(
            R.Loc parentLoc,
            TypeValue parentType,
            LambdaCapture parentLambdaCapture,
            string memberName, ImmutableArray<TypeValue> typeArgs, 
            ResolveHint hint)
        {
            // 해당 이름의 타입, 변수, 함수
            var memberResult = parentType.GetMember(memberName, typeArgs, hint);

            switch(memberResult)
            {
                case ErrorItemResult errorResult:
                    return ToErrorIdentifierResult(errorResult);

                case NotFoundItemResult:
                    return NotFoundIdentifierResult.Instance;

                case ValueItemResult itemResult:
                    switch(itemResult.ItemValue)
                    {
                        case TypeValue:
                            return CantGetTypeMemberThroughInstanceIdentifierResult.Instance;

                        case FuncValue funcValue:
                            if (funcValue.IsStatic)
                                return CantGetStaticMemberThroughInstanceIdentifierResult.Instance;

                            return new InstanceFuncIdentifierResult(parentLoc, parentType, funcValue, parentLambdaCapture);

                        case MemberVarValue memberVarValue:
                            if (memberVarValue.IsStatic)
                                return CantGetStaticMemberThroughInstanceIdentifierResult.Instance;

                            var loc = BuildMemberLoc(parentLoc, parentType, memberName);
                            return new LocIdentifierResult(loc, memberVarValue.GetTypeValue(), parentLambdaCapture);

                        default:
                            throw new UnreachableCodeException();
                    }
            }

            throw new UnreachableCodeException();
        }

        // T.x 꼴
        IdentifierResult ResolveIdentifierMemberExpTypeParent(TypeValue parentType, string memberName, ImmutableArray<TypeValue> typeArgs, ResolveHint hint)
        {
            var member = parentType.GetMember(memberName, typeArgs, hint);

            switch (member)
            {
                case NotFoundItemResult:
                    return NotFoundIdentifierResult.Instance;

                case ErrorItemResult errorResult:
                    return ToErrorIdentifierResult(errorResult);

                case ValueItemResult itemResult:
                    switch(itemResult.ItemValue)
                    {
                        case TypeValue typeValue: return new TypeIdentifierResult(typeValue);
                        case FuncValue funcValue:

                            if (!funcValue.IsStatic)
                                return CantGetInstanceMemberThroughTypeIdentifierResult.Instance;

                            return new StaticFuncIdentifierResult(funcValue);

                        case MemberVarValue memberVarValue:

                            if (!memberVarValue.IsStatic)
                                return CantGetInstanceMemberThroughTypeIdentifierResult.Instance;

                            var rparentType = parentType.GetRType();
                            var loc = new R.StaticMemberLoc(rparentType, memberName);
                            return new LocIdentifierResult(loc, memberVarValue.GetTypeValue(), NoneLambdaCapture.Instance);

                        default:
                            throw new UnreachableCodeException();
                    }

                default:
                    throw new UnreachableCodeException();
            }
        }

        IdentifierResult ResolveIdentifierMemberExp(S.MemberExp memberExp, ResolveHint hint)
        {
            var typeArgs = GetTypeValues(memberExp.MemberTypeArgs, context);

            // 힌트가 없다. EnumElemIdentifierResult가 나올 수 없다
            var parentResult = ResolveIdentifier(memberExp.Parent, ResolveHint.None);

            switch (parentResult)
            {
                case ErrorIdentifierResult _:
                    return parentResult;                

                case LocIdentifierResult locResult:
                    return ResolveIdentifierMemberExpLocParent(locResult.Loc, locResult.TypeValue, locResult.LambdaCapture, memberExp.MemberName, typeArgs, hint);

                case TypeIdentifierResult typeResult:
                    return ResolveIdentifierMemberExpTypeParent(typeResult.TypeValue, memberExp.MemberName, typeArgs, hint);

                case InstanceFuncIdentifierResult _:
                    // 함수는 멤버변수를 가질 수 없습니다
                    return FuncCantHaveMemberErrorIdentifierResult.Instance;

                case StaticFuncIdentifierResult _:
                    // 함수는 멤버변수를 가질 수 없습니다
                    return FuncCantHaveMemberErrorIdentifierResult.Instance;

                case EnumElemIdentifierResult enumElemResult:
                    // 힌트 없이 EnumElem이 나올 수가 없다
                    throw new UnreachableCodeException();
            }

            throw new UnreachableCodeException();
        }

        IdentifierResult ResolveIdentifier(S.Exp exp, ResolveHint hint)
        {
            if (exp is S.IdentifierExp idExp)
            {
                return ResolveIdentifierIdExp(idExp, hint);
            }
            else if (exp is S.MemberExp memberExp)
            {
                return ResolveIdentifierMemberExp(memberExp, hint);
            }
            else
            {
                var expResult = AnalyzeExpExceptIdAndMember(exp, hint);
                return new LocIdentifierResult(new R.TempLoc(expResult.Exp, expResult.TypeValue.GetRType()), expResult.TypeValue, NoneLambdaCapture.Instance);
            }
        }

        struct IdExpIdentifierResolver
        {
            string idName;
            ImmutableArray<TypeValue> typeArgs;
            ResolveHint hint;
            Context context;

            public IdExpIdentifierResolver(string idName, ImmutableArray<TypeValue> typeArgs, ResolveHint hint, Context context)
            {
                this.idName = idName;
                this.typeArgs = typeArgs;
                this.hint = hint;
                this.context = context;
            }

            IdentifierResult GetLocalVarOutsideLambdaInfo()
            {
                // 지역 스코프에는 변수만 있고, 함수, 타입은 없으므로 이름이 겹치는 것이 있는지 검사하지 않아도 된다
                if (typeArgs.Length != 0) return NotFoundIdentifierResult.Instance;

                var varInfo = context.GetLocalVarOutsideLambda(idName);
                if (varInfo == null) return NotFoundIdentifierResult.Instance;

                var localCapture = new LocalLambdaCapture(varInfo.Value.Name, varInfo.Value.TypeValue);
                return new LocIdentifierResult(new R.LocalVarLoc(varInfo.Value.Name), varInfo.Value.TypeValue, localCapture);
            }

            IdentifierResult GetLocalVarInfo()
            {
                // 지역 스코프에는 변수만 있고, 함수, 타입은 없으므로 이름이 겹치는 것이 있는지 검사하지 않아도 된다
                if (typeArgs.Length != 0) return NotFoundIdentifierResult.Instance;

                var varInfo = context.GetLocalVar(idName);
                if (varInfo == null) return NotFoundIdentifierResult.Instance;

                return new LocIdentifierResult(new R.LocalVarLoc(varInfo.Value.Name), varInfo.Value.TypeValue, NoneLambdaCapture.Instance);
            }

            IdentifierResult GetThisMemberInfo()
            {
                // TODO: implementation
                return NotFoundIdentifierResult.Instance;
            }

            IdentifierResult GetInternalGlobalVarInfo()
            {
                if (typeArgs.Length != 0) return NotFoundIdentifierResult.Instance;

                var varInfo = context.GetInternalGlobalVarInfo(idName);
                if (varInfo == null) return NotFoundIdentifierResult.Instance;

                return new LocIdentifierResult(new R.GlobalVarLoc(varInfo.Name.ToString()), varInfo.TypeValue, NoneLambdaCapture.Instance);
            }
            
            IdentifierResult GetGlobalInfo()
            {
                // TODO: outer namespace까지 다 돌아야 한다
                var curNamespacePath = M.NamespacePath.Root; 
                var globalResult = context.GetGlobalItem(curNamespacePath, idName, typeArgs, hint);

                switch (globalResult)
                {
                    case NotFoundItemResult: return NotFoundIdentifierResult.Instance;
                    case ErrorItemResult errorResult: return ToErrorIdentifierResult(errorResult);
                    case ValueItemResult itemResult:
                        switch (itemResult.ItemValue)
                        {
                            case TypeValue typeValue: return new TypeIdentifierResult(typeValue);
                            case FuncValue funcValue: return new StaticFuncIdentifierResult(funcValue);
                            default: throw new UnreachableCodeException();
                        }
                }

                throw new UnreachableCodeException();
            }

            IdentifierResult ResolveScope()
            {
                // 0. local 변수, local 변수에서는 힌트를 쓸 일이 없다
                var localVarInfo = GetLocalVarInfo();
                if (localVarInfo != NotFoundIdentifierResult.Instance) return localVarInfo;

                // 1. 람다 바깥의 local 변수
                var localOutsideInfo = GetLocalVarOutsideLambdaInfo();
                if (localOutsideInfo != NotFoundIdentifierResult.Instance) return localOutsideInfo;

                // 2. thisType의 {{instance, static} * {변수, 함수}}, 타입. 아직 지원 안함
                // 힌트는 오버로딩 함수 선택에 쓰일수도 있고,
                // 힌트가 thisType안의 enum인 경우 elem을 선택할 수도 있다
                var thisMemberInfo = GetThisMemberInfo();
                if (thisMemberInfo != NotFoundIdentifierResult.Instance) return thisMemberInfo;

                // 3. internal global 'variable', 변수이므로 힌트를 쓸 일이 없다
                var internalGlobalVarInfo = GetInternalGlobalVarInfo();
                if (internalGlobalVarInfo != NotFoundIdentifierResult.Instance) return internalGlobalVarInfo;

                // 4. 네임스페이스 -> 바깥 네임스페이스 -> module global, 함수, 타입, 
                // 오버로딩 함수 선택, hint가 global enum인 경우, elem선택
                var externalGlobalInfo = GetGlobalInfo();
                if (externalGlobalInfo != NotFoundIdentifierResult.Instance) return externalGlobalInfo;

                return NotFoundIdentifierResult.Instance;
            }

            IdentifierResult ResolveEnumHint()
            {
                // 힌트가 E고, First가 써져 있으면 E.First를 검색한다
                // enum 힌트 사용, typeArgs가 있으면 지나간다
                if (hint.TypeHint is TypeValueTypeHint typeValueHintType && typeValueHintType.TypeValue is EnumTypeValue enumTypeValue)
                {
                    // First<T> 같은건 없기 때문에 없을때만 검색한다
                    if (typeArgs.Length == 0)
                    {
                        throw new NotImplementedException();
                        //if (enumHintType.GetEnumElem(idName, out var elemInfo))
                        //{
                        //    return new EnumElemIdentifierResult(hintNTV, elemInfo.Value);
                        //}
                    }
                }
                
                return NotFoundIdentifierResult.Instance;
            }

            public IdentifierResult Resolve()
            {
                // (로컬/글로벌/멤버)와 enum 힌트는 동급이다
                // E First;
                // E e = First; // 누구 말을 들을것인가, 에러.. local First를 지칭할 방법은? local.First, E.First

                var candidates = new Candidates<IdentifierResult>();

                // 로컬 -> ... -> 글로벌
                var scopeResult = ResolveScope();
                if (scopeResult is ValidIdentifierResult) candidates.Add(scopeResult);
                if (scopeResult != NotFoundIdentifierResult.Instance) return scopeResult;

                // enum 힌트
                var enumResult = ResolveEnumHint();
                if (enumResult is ValidIdentifierResult) candidates.Add(scopeResult);
                if (enumResult != NotFoundIdentifierResult.Instance) return enumResult;

                var result = candidates.GetSingle();
                if (result != null) return result;
                if (candidates.IsEmpty) return NotFoundIdentifierResult.Instance;
                if (candidates.HasMultiple) return MultipleCandiatesErrorIdentifierResult.Instance;
                throw new UnreachableCodeException();
            }
        }
    }
}
