using System;
using System.Diagnostics;

using Citron.Infra;
using Citron.Collections;
using Pretune;

using M = Citron.CompileTime;

namespace Citron.Analysis
{
    // SymbolId => Symbol
    [AutoConstructor]
    public partial class SymbolLoader
    {
        SymbolFactory factory;
        ImmutableArray<ModuleDeclSymbol> moduleDecls;

        ISymbolNode LoadPath(ModuleDeclSymbol moduleDecl, M.SymbolPath? path)
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
            
        public ISymbolNode Load(M.SymbolId id)
        {
            switch(id)
            {
                case M.ModuleSymbolId moduleId:
                    foreach (var moduleDecl in moduleDecls)
                        if (moduleDecl.Equals(moduleId.ModuleName))
                            return LoadPath(moduleDecl, moduleId.Path);

                    throw new NotImplementedException(); // 에러 처리

                case M.VarSymbolId:
                    return factory.MakeVar();

                case M.TypeVarSymbolId typeVarId:
                    throw new NotImplementedException(); // TypeVarSymbol

                case M.NullableSymbolId nullableId:                        
                    throw new NotImplementedException(); // NullableSymbol

                case M.VoidSymbolId voidId:
                    throw new NotImplementedException(); // VoidSymbol

                default:
                    throw new UnreachableCodeException();
            }
        }

        public SymbolQueryResult Query(M.SymbolPath? outerPath, M.Name name, int typeParamCount)
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
                return SymbolQueryResult.NotFound.Instance;

            if (candidates.HasMultiple)
                return SymbolQueryResult.Error.MultipleCandidates.Instance;

            throw new UnreachableCodeException();
        }
    }
 
}