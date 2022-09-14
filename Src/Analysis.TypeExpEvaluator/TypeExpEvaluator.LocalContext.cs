using Citron.Collections;
using System;
using Citron.Symbol;
using Citron.Syntax;
using Citron.Module;
using System.Diagnostics;
using Citron.Infra;
using System.Collections.Generic;

namespace Citron.Analysis
{
    public partial class TypeExpEvaluator
    {
        class LocalContext
        {
            LocalContext? outer;
            Skeleton skeleton; // 현재 Skeleton 위치
            IHolder<ImmutableArray<FuncParamId>> funcParamIdsHolder;

            LocalContext(LocalContext? outer, Skeleton skeleton, IHolder<ImmutableArray<FuncParamId>> funcParamIdsHolder)
            {
                this.outer = outer;
                this.skeleton = skeleton;
                this.funcParamIdsHolder = funcParamIdsHolder;
            }

            public static LocalContext NewModuleContext(Skeleton moduleSkel)
            {
                Debug.Assert(moduleSkel.Kind == SkeletonKind.Module);
                return new LocalContext(null, moduleSkel, new Holder<ImmutableArray<FuncParamId>>(default));
            }

            public LocalContext NewLocalContextWithFuncDecl(IHolder<ImmutableArray<FuncParamId>> funcParamIdsHolder, ISyntaxNode node)
            {
                var funcSkel = skeleton.GetFuncDeclMember(node);
                Debug.Assert(funcSkel != null);

                return new LocalContext(this, funcSkel, funcParamIdsHolder);
            }

            public LocalContext NewLocalContext(Name.Normal name, int typeParamCount)
            {
                var memberSkel = skeleton.GetMember(name, typeParamCount, 0);
                Debug.Assert(memberSkel != null);

                return new LocalContext(this, memberSkel, new Holder<ImmutableArray<FuncParamId>>(default));
            }

            // 해당하는 이름의 
            public UniqueQueryResult<Skeleton> GetUniqueSkeletonMember(Name name, int typeParamCount)
            {
                return skeleton.GetUniqueMember(name, typeParamCount);
            }

            public IEnumerable<Skeleton> GetSkeletonMembers(Name name, int typeParamCount)
            {
                return skeleton.GetMembers(name, typeParamCount);
            }

            public LocalContext? GetOuter()
            {
                return outer;
            }
            // 현재 decl space에서 선언된 타입파라미터 총 개수
            int GetTotalTypeParamCount()
            {
                if (outer == null)
                    return skeleton.TypeParamCount;

                return outer.GetTotalTypeParamCount() + skeleton.TypeParamCount;
            }

            // GetThisSymbolId -> 현재 decl space의 This 타입을 나타낸다
            // class C<T> { class D<U> {
            //                             // 이 space의 This타입은 C<T>.D<U>
            //     D<int> d;               // C<T>.D<int>를 만들 수 있다
            // } }
            //
            // 이 함수를 계산으로 만드는 이유는, LocalContext 생성 시점엔 ModuleSymbolId를 알수가 없기 때문이다
            // T F<T>(T t) { ... } 를 Evaluation할 때,
            // (1)T F<T>((2)T t) { (3) ... }
            // 
            // (1)시점에 F<T>(T)에 대한 localContext를 생성하는데, F<T>(T)의 ModuleSymbolId는 (2)의 SymbolId를 필요로 한다
            // 인자 T는 F<>와 관련을 없애기 위해서, Index로만 계산한다
            public ModuleSymbolId GetThisSymbolId()
            {
                switch(skeleton.Kind)
                {
                    case SkeletonKind.Module:
                        Debug.Assert(outer == null);
                        return new ModuleSymbolId(skeleton.Name, null);

                    case SkeletonKind.Namespace:
                    case SkeletonKind.Class:
                    case SkeletonKind.Struct:
                    case SkeletonKind.Interface:
                    case SkeletonKind.Enum:
                    case SkeletonKind.EnumElem:
                    case SkeletonKind.GlobalFunc:
                    case SkeletonKind.ClassMemberFunc:
                    case SkeletonKind.StructMemberFunc:
                        Debug.Assert(outer != null);
                        var outerId = outer.GetThisSymbolId();
                        var outerTypeParamCount = outer.GetTotalTypeParamCount();

                        // typeParam을 만들어야 한다                        
                        var typeVarsBuilder = ImmutableArray.CreateBuilder<SymbolId>(skeleton.TypeParamCount);
                        for (int i = outerTypeParamCount, totalTypeParamCount = outerTypeParamCount + skeleton.TypeParamCount; i < totalTypeParamCount; i++)
                            typeVarsBuilder.Add(new TypeVarSymbolId(i));

                        return outerId.Child(skeleton.Name, typeVarsBuilder.MoveToImmutable(), funcParamIdsHolder.GetValue());

                    case SkeletonKind.TypeVar: // decl space에 TypeVar가 들어오는 경우는 없으므로 에러
                    default:
                        throw new UnreachableCodeException();
                }
            }

            // 지금 위치가 namespace인가
            public bool IsOnModuleOrNamespace()
            {
                return skeleton.Kind == SkeletonKind.Module || skeleton.Kind == SkeletonKind.Namespace;
            }

            public SymbolPath? GetNamespacePath()
            {
                Debug.Assert(skeleton.Kind == SkeletonKind.Module || skeleton.Kind == SkeletonKind.Namespace);
                Debug.Assert(skeleton.TypeParamCount == 0);

                if (skeleton.Kind == SkeletonKind.Module)
                    return null;

                Debug.Assert(outer != null);
                SymbolPath? outerPath = outer.GetNamespacePath();

                return new SymbolPath(outerPath, skeleton.Name);
            }
        }
    }
}
