using System;
using System.Diagnostics;

using Citron.Infra;
using Citron.Collections;
using Citron.Symbol;

using Pretune;

using M = Citron.Module;

using static Citron.Symbol.FuncParamTypeId;

namespace Citron.Analysis
{
    [AutoConstructor]
    public partial class TypeLoader
    {
        SymbolLoader symbolLoader;

        public IType Load(TypeId id, TypeEnv typeEnv)
        {
            switch (id)
            {
                case SymbolId symbolId:
                    return ((ITypeSymbol)symbolLoader.Load(symbolId)).MakeType();

                case VarTypeId:
                    return new VarType();

                case VoidTypeId voidTypeId:
                    return new VoidType();

                case NullableTypeId nullableId:
                    throw new NotImplementedException(); // NullableSymbol

                // class C<T> { class D<U> { "여기를 분석할 때" } }
                // 분석중인 Decl환경에서 C<T>.D<U>
                // TypeVarSymbolId는 index만 갖고 있다 (1이면 U이다)
                // 그럼 지금 위치 (C<T>.D<U>)를 넘겨주던가
                // [T, U] 리스트를 넘겨주던가 해야한다
                case TypeVarTypeId typeVarId: // 3이러면 어떻게 아는가
                    return new TypeVarType(typeVarId.Index, typeVarId.Name);

                default:
                    throw new UnreachableCodeException();
            }
        }
    }

    // SymbolId => Symbol
    [AutoConstructor]
    public partial class SymbolLoader
    {
        SymbolFactory factory;
        ImmutableArray<ModuleDeclSymbol> moduleDecls;

        ISymbolNode LoadPath(ModuleDeclSymbol moduleDecl, SymbolPath? path)
        {
            if (path == null)
            {
                var instance = SymbolInstantiator.Instantiate(factory, null, moduleDecl, default);
                if (instance == null)
                    throw new NotImplementedException(); // 에러 처리

                return instance;
            }
            else
            {
                var outer = LoadPath(moduleDecl, path.Outer);
                var outerDecl = outer.GetDeclSymbolNode();
                if (outerDecl == null)
                    throw new NotImplementedException(); // 에러 처리

                var decl = outerDecl.GetMemberDeclNode(new DeclSymbolNodeName(path.Name, path.TypeArgs.Length, path.ParamIds));
                if (decl == null)
                    throw new NotImplementedException(); // 에러 처리

                var instance = SymbolInstantiator.Instantiate(factory, outer, decl, default);

                if (instance == null)
                    throw new NotImplementedException(); // 에러 처리

                return instance;
            }
        }

        // TypeVar때문에 환경을 알고 있어야 한다? => 재귀적이기 때문에 Id만 완전하면 환경을 몰라도 된다
        // class C<X> { class D<X> { X x; } } // 여기서 X x;의 X의 Symbol은 C<TypeVar(0)>.D<TypeVar(1)>.TypeVar(1)
        public ISymbolNode Load(SymbolId id)
        {
            switch (id)
            {
                case SymbolId symbolId:
                    foreach (var moduleDecl in moduleDecls)
                        if (moduleDecl.GetName().Equals(symbolId.ModuleName))
                            return LoadPath(moduleDecl, symbolId.Path);

                    throw new NotImplementedException(); // 에러 처리

                
                default:
                    throw new UnreachableCodeException();
            }
        }

        public SymbolQueryResult Query(SymbolPath? outerPath, M.Name name, int typeParamCount)
        {
            var candidates = new Candidates<SymbolQueryResult>();

            // 각 모듈 decl들에 대해서,
            foreach (var moduleDecl in moduleDecls)
            {
                var symbol = LoadPath(moduleDecl, outerPath); // 실패할 경우
                if (symbol is ITypeSymbol typeSymbol)
                {
                    var queryResult = typeSymbol.QueryMember(name, typeParamCount);
                    if (queryResult is SymbolQueryResult.Error)
                        return queryResult; // 즉시 종료

                    if (queryResult is SymbolQueryResult.Valid)
                    {
                        candidates.Add(queryResult);
                        continue;
                    }

                    Debug.Assert(queryResult is SymbolQueryResult.NotFound);
                }
            }

            var result = candidates.GetUniqueResult();

            if (result.IsFound(out var value))
                return value;
            else if (result.IsNotFound())
                return SymbolQueryResults.NotFound;
            else if (result.IsMultipleError())
                return SymbolQueryResults.Error.MultipleCandidates;

            throw new UnreachableCodeException();
        }
    }
 
}