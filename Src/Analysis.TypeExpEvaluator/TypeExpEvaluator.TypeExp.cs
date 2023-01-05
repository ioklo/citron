using System.Collections.Generic;
using System.Text;
using System.Linq;

using Citron.Infra;
using Citron.Syntax;
using Citron.Module;
using Citron.Symbol;
using Citron.Collections;

using static Citron.Analysis.TypeExpErrorCode;
using System;
using System.Diagnostics;

namespace Citron.Analysis
{
    public partial class TypeExpEvaluator
    {
        struct TypeExpVisitor
        {
            LocalContext localContext;
            GlobalContext globalContext;

            public TypeExpVisitor(LocalContext localContext, GlobalContext globalContext)
            {
                this.localContext = localContext;
                this.globalContext = globalContext;
            }

            public static TypeExpInfo Visit(TypeExp exp, LocalContext localContext, GlobalContext globalContext)
            {
                var visitor = new TypeExpVisitor(localContext, globalContext);
                return visitor.VisitTypeExpOuterMost(exp);
            }

            public static ImmutableArray<TypeExpInfo> Visit(ImmutableArray<TypeExp> typeArgExps, LocalContext localContext, GlobalContext globalContext)
            {
                var visitor = new TypeExpVisitor(localContext, globalContext);

                var builder = ImmutableArray.CreateBuilder<TypeExpInfo>(typeArgExps.Length);
                foreach (var typeArgExp in typeArgExps)
                {
                    var info = visitor.VisitTypeExpOuterMost(typeArgExp);
                    builder.Add(info);
                }

                return builder.MoveToImmutable();
            }

            // 입구 함수
            UniqueQueryResult<TypeExpInfo> QueryOnSkeleton(LocalContext? curContext, Name name, ImmutableArray<SymbolId> typeArgs, TypeExp typeExp)
            {
                // curContext가 없는 것은 모듈의 부모일때 밖에 없다. 검색 불가능                
                if (curContext == null) 
                    return UniqueQueryResults<TypeExpInfo>.NotFound;

                // 컨텍스트가 네임스페이스라면 네임 스페이스 전용 쿼리로 넘어간다 (skeleton과 모듈에서 동시 검색)
                if (curContext.IsOnModuleOrNamespace())
                {
                    var curPath = curContext.GetNamespacePath();
                    return QueryOnSkeletonAndRefModules(curContext, curPath, name, typeArgs, typeExp);
                }
                else
                {
                    return QueryOnSkeletonOnly(curContext, name, typeArgs, typeExp);
                }
            }

            public TypeExpInfo MakeInternalTypeExpInfo(LocalContext curContext, Skeleton skel, ImmutableArray<SymbolId> typeArgs, TypeExp typeExp)
            {
                switch (skel.Kind)
                {
                    // 불가능 에러
                    case SkeletonKind.Module:
                        throw new UnreachableCodeException();

                    // 일반 케이스
                    case SkeletonKind.Namespace:
                    case SkeletonKind.Class:
                    case SkeletonKind.Struct:
                    case SkeletonKind.Interface:
                    case SkeletonKind.Enum:
                    case SkeletonKind.EnumElem:
                        {
                            var symbolId = curContext.GetThisSymbolId();
                            var memberSymbolId = symbolId.Child(skel.Name, typeArgs);
                            return new InternalTypeExpInfo(memberSymbolId, skel, typeExp);
                        }

                    // TypeVar는 따로 처리
                    case SkeletonKind.TypeVar:
                        {
                            Debug.Assert(typeArgs.Length == 0);
                            Debug.Assert(skel.TypeParamIndex != null);

                            var memberSymbolId = new TypeVarSymbolId(skel.TypeParamIndex.Value);
                            return new InternalTypeExpInfo(memberSymbolId, skel, typeExp);
                        }

                    // 함수 참조 에러
                    case SkeletonKind.GlobalFunc:
                    case SkeletonKind.ClassMemberFunc:
                    case SkeletonKind.StructMemberFunc:
                        // TODO: 함수 참조 에러 
                        throw new NotImplementedException();

                    default:
                        throw new UnreachableCodeException();
                }
            }

            // C<>.F<>.T는 있지만 symbol decl space에 존재, 
            // C<int>.F<short>.T는 없다.. id space에서는 없다?
            // typealias로 존재해야 하는거 아니냐? T = short
            UniqueQueryResult<TypeExpInfo> QueryOnSkeletonOnly(LocalContext curContext, Name name, ImmutableArray<SymbolId> typeArgs, TypeExp typeExp)
            {
                var result = curContext.GetUniqueSkeletonMember(name, typeArgs.Length);

                switch (result)
                {
                    case UniqueQueryResult<Skeleton>.Found foundResult:
                        {
                            var typeExpInfo = MakeInternalTypeExpInfo(curContext, foundResult.Value, typeArgs, typeExp);
                            return UniqueQueryResults<TypeExpInfo>.Found(typeExpInfo);
                        }

                    case UniqueQueryResult<Skeleton>.MultipleError:
                        return UniqueQueryResults<TypeExpInfo>.MultipleError;

                    // 발견을 못했을 경우
                    case UniqueQueryResult<Skeleton>.NotFound:
                        var outerContext = curContext.GetOuter();
                        return QueryOnSkeleton(outerContext, name, typeArgs, typeExp);
                        
                    default:
                        throw new UnreachableCodeException();
                }
            }

            UniqueQueryResult<TypeExpInfo> QueryOnSkeletonAndRefModules(LocalContext curContext, SymbolPath? curPath, Name name, ImmutableArray<SymbolId> typeArgs, TypeExp typeExp)
            {
                var candidates = new Candidates<TypeExpInfo>();

                foreach (var member in localContext.GetSkeletonMembers(name, typeArgs.Length))
                {
                    candidates.Add(MakeInternalTypeExpInfo(curContext, member, typeArgs, typeExp));
                }
                
                // type만 쿼리하므로 FuncParamId는 쓰지 않는다
                var path = curPath.Child(name, typeArgs);
                foreach(var (typeId, declSymbol) in globalContext.QuerySymbolsOnReference(path))
                {
                    candidates.Add(new ModuleSymbolTypeExpInfo(typeId, declSymbol, typeExp));
                }

                var result = candidates.GetUniqueResult();
                switch(result)
                {
                    case UniqueQueryResult<TypeExpInfo>.Found:
                    case UniqueQueryResult<TypeExpInfo>.MultipleError:
                        return result;

                    case UniqueQueryResult<TypeExpInfo>.NotFound:
                        var outerContext = curContext.GetOuter();
                        if (outerContext != null)
                        {
                            Debug.Assert(curPath != null);
                            return QueryOnSkeletonAndRefModules(outerContext, curPath.Outer, name, typeArgs, typeExp);
                        }
                        else
                        {
                            return UniqueQueryResults<TypeExpInfo>.NotFound;
                        }

                    default: 
                        throw new UnreachableCodeException();
                }
            }

            TypeExpInfo VisitIdTypeExp(IdTypeExp typeExp)
            {
                if (typeExp.Name == "var")
                {
                    if (typeExp.TypeArgs.Length != 0)
                        globalContext.Throw(T0102_IdTypeExp_VarTypeCantApplyTypeArgs, typeExp, "var는 타입 인자를 가질 수 없습니다");

                    return SpecialTypeExpInfo.Var(typeExp);
                }
                else if (typeExp.Name == "void")
                {
                    if (typeExp.TypeArgs.Length != 0)
                        globalContext.Throw(T0101_IdTypeExp_TypeDoesntHaveTypeParams, typeExp, "void는 타입 인자를 가질 수 없습니다");

                    return SpecialTypeExpInfo.Void(typeExp);
                }

                // built-in
                else if (typeExp.Name == "bool")
                {
                    return BuiltinTypeExpInfo.Bool(typeExp);
                }
                else if (typeExp.Name == "int")
                {
                    return BuiltinTypeExpInfo.Int(typeExp);
                }
                else if (typeExp.Name == "string")
                {
                    return BuiltinTypeExpInfo.String(typeExp);
                }

                // 1. 먼저 typeArgs를 다 TypeExpInfo로 만든다
                var typeArgs = VisitTypeArgExps(typeExp.TypeArgs);

                // 2. TypeExp가 현재 속한 skeleton을 시작으로, skeleton과 레퍼런스에서 해당 이름을 검색한다
                var result = QueryOnSkeleton(localContext, new Name.Normal(typeExp.Name), typeArgs, typeExp);

                switch(result)
                {
                    case UniqueQueryResult<TypeExpInfo>.Found foundResult:
                        return foundResult.Value;
                        
                    case UniqueQueryResult<TypeExpInfo>.MultipleError:
                        globalContext.Throw(T0103_IdTypeExp_MultipleTypesOfSameName, typeExp, $"이름이 같은 {typeExp} 타입이 여러개 입니다");
                        throw new UnreachableCodeException();

                    case UniqueQueryResult<TypeExpInfo>.NotFound:
                        globalContext.Throw(T0104_IdTypeExp_TypeNotFound, typeExp, $"{typeExp}를 찾지 못했습니다");
                        throw new UnreachableCodeException();

                    default: 
                        throw new UnreachableCodeException();
                }
            }

            // X<T>.Y<U, V>
            TypeExpInfo VisitMemberTypeExp(MemberTypeExp exp)
            {
                // X<T>
                var parentResult = VisitTypeExp(exp.Parent);

                // U, V            
                var typeArgs = VisitTypeArgExps(exp.TypeArgs);

                var memberResult = parentResult.MakeMemberInfo(exp.MemberName, typeArgs, exp);
                if (memberResult == null)
                    globalContext.Throw(T0202_MemberTypeExp_MemberTypeNotFound, exp, $"{parentResult}에서 {exp.MemberName}을 찾을 수 없습니다");

                return memberResult;
            }

            // int?
            TypeExpInfo VisitNullableTypeExp(NullableTypeExp exp)
            {
                // int
                var innerTypeResult = VisitTypeExp(exp.InnerTypeExp);
                var typeId = innerTypeResult.GetSymbolId();                

                return SpecialTypeExpInfo.Nullable(typeId, exp);
            }

            TypeExpInfo VisitTypeExp(TypeExp exp)
            {
                switch (exp)
                {
                    case IdTypeExp idExp: return VisitIdTypeExp(idExp);
                    case MemberTypeExp memberExp: return VisitMemberTypeExp(memberExp);
                    case NullableTypeExp nullableExp: return VisitNullableTypeExp(nullableExp);

                    default: throw new UnreachableCodeException();
                }
            }

            // VisitTypeExp와 다른 점은 실행 후 (TypeExp => TypeExpInfo) 정보를 추가
            TypeExpInfo VisitTypeExpOuterMost(TypeExp exp)
            {
                try
                {
                    var result = VisitTypeExp(exp);
                    exp.SetTypeExpInfo(result);
                    return result;
                }
                catch (FatalException)
                {
                    // 에러 처리를 해야 한다
                    throw new NotImplementedException();
                }                
            }

            ImmutableArray<SymbolId> VisitTypeArgExps(ImmutableArray<TypeExp> typeArgExps)
            {
                var builder = ImmutableArray.CreateBuilder<SymbolId>(typeArgExps.Length);
                foreach (var typeArgExp in typeArgExps)
                {
                    var typeArgResult = VisitTypeExp(typeArgExp);
                    var typeArg = typeArgResult.GetSymbolId();

                    builder.Add(typeArg);
                }

                return builder.MoveToImmutable();
            }
        }
    }
}
