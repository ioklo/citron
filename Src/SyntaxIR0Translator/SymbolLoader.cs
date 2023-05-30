﻿using System;
using System.Diagnostics;

using Citron.Infra;
using Citron.Collections;
using Citron.Symbol;

using Pretune;

namespace Citron.Analysis;

// SymbolId => Symbol
[AutoConstructor]
public partial struct SymbolLoader
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

            var typeLoader = new TypeLoader(this); // 복사해서 쓴다
            var typeArgsBuilder = ImmutableArray.CreateBuilder<IType>(path.TypeArgs.Length);
            foreach (var typeArgId in path.TypeArgs)
            {
                var typeArg = typeLoader.Load(typeArgId);
                typeArgsBuilder.Add(typeArg);
            }

            var instance = SymbolInstantiator.Instantiate(factory, outer, decl, typeArgsBuilder.MoveToImmutable());

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
                throw new UnreachableException();
        }
    }

    public SymbolQueryResult? Query(SymbolPath? outerPath, Name name, int typeParamCount)
    {
        var candidates = new Candidates<SymbolQueryResult>();

        // 각 모듈 decl들에 대해서,
        foreach (var moduleDecl in moduleDecls)
        {
            var symbol = LoadPath(moduleDecl, outerPath); // 실패할 경우
            
            var queryResult = symbol.QueryMember(name, typeParamCount);
            if (queryResult == null) continue;

            if (queryResult is SymbolQueryResult.MultipleCandidatesError)
                return queryResult; // 즉시 종료
            
            candidates.Add(queryResult);
        }

        int count = candidates.GetCount();
        if (count == 0) return null;
        else if (count == 1) return candidates.GetAt(0);
        else
        {
            Debug.Assert(1 < count);
            var builder = ImmutableArray.CreateBuilder<SymbolQueryResult>(count);
            for (int i = 0; i < count; i++)
                builder.Add(candidates.GetAt(i));

            return new SymbolQueryResult.MultipleCandidatesError(builder.MoveToImmutable());
        }
    }
}
