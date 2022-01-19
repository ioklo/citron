﻿using Gum.Infra;
using System;
using Gum.Collections;
using Pretune;
using M = Gum.CompileTime;
using System.Diagnostics;

namespace Gum.Analysis
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

                var decl = outerDecl.GetMemberDeclNode(path.Name, path.TypeArgs.Length, path.ParamIds);
                if (decl == null)
                    throw new NotImplementedException(); // 에러 처리

                var instance = SymbolInstantiator.Instantiate(factory, outer, decl, default);

                if (instance == null)
                    throw new NotImplementedException(); // 에러 처리

                return instance;
            }
        }
            
        public ISymbolNode Load(SymbolId id)
        {
            switch(id)
            {
                case ModuleSymbolId moduleId:
                    foreach (var moduleDecl in moduleDecls)
                        if (moduleDecl.Equals(moduleId.ModuleName))
                            return LoadPath(moduleDecl, moduleId.Path);

                    throw new NotImplementedException(); // 에러 처리

                case TypeVarSymbolId typeVarId:                        
                    throw new NotImplementedException(); // TypeVarSymbol

                case NullableSymbolId nullableId:                        
                    throw new NotImplementedException(); // NullableSymbol

                case VoidSymbolId voidId:
                    throw new NotImplementedException(); // VoidSymbol

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