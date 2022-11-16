using System;
using System.Diagnostics;

using Citron.Infra;
using Citron.Collections;
using Pretune;

using Citron.Module;
using Citron.Symbol;

namespace Citron.Analysis
{
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

        // TypeVar때문에 환경을 알고 있어야 한다
        public ISymbolNode Load(TypeEnv typeEnv, SymbolId id)
        {
            switch (id)
            {
                case ModuleSymbolId moduleId:
                    foreach (var moduleDecl in moduleDecls)
                        if (moduleDecl.GetName().Equals(moduleId.ModuleName))
                            return LoadPath(moduleDecl, moduleId.Path);

                    throw new NotImplementedException(); // 에러 처리

                case VarSymbolId:
                    return factory.MakeVar();

                case NullableSymbolId nullableId:
                    throw new NotImplementedException(); // NullableSymbol

                case VoidSymbolId:
                    return factory.MakeVoid();

                // class C<T> { class D<U> { "여기를 분석할 때" } }
                // 분석중인 Decl환경에서 C<T>.D<U>
                // TypeVarSymbolId는 index만 갖고 있다 (1이면 U이다)
                // 그럼 지금 위치 (C<T>.D<U>)를 넘겨주던가
                // [T, U] 리스트를 넘겨주던가 해야한다
                case TypeVarSymbolId typeVarId: // 3이러면 어떻게 아는가
                    factory.MakeTypeVar()
                    typeId = env.GetTypeId(typeVarId.Index);
                    break;

                default:
                    throw new UnreachableCodeException();
            }
        }

        public SymbolQueryResult Query(SymbolPath? outerPath, Name name, int typeParamCount)
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

            var result = candidates.GetSingle();
            if (result != null)
                return result;

            if (candidates.IsEmpty)
                return SymbolQueryResults.NotFound;

            if (candidates.HasMultiple)
                return SymbolQueryResults.Error.MultipleCandidates;

            throw new UnreachableCodeException();
        }
    }
 
}