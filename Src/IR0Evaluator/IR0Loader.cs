using Citron.Collections;
using Citron.Infra;
using Citron.IR0;
using Citron.Symbol;
using System;
using System.Diagnostics;

namespace Citron
{
    // SymbolPath => Symbol, SymbolPath => Stmt
    public class IR0Loader
    {
        SymbolFactory factory;
        ModuleDeclSymbol moduleDecl;        
        ImmutableDictionary<DeclSymbolPath, ImmutableArray<Stmt>> bodies;

        public static IR0Loader Make(SymbolFactory factory, Script script)
        {
            var bodiesBuilder = ImmutableDictionary.CreateBuilder<DeclSymbolPath, ImmutableArray<Stmt>>();
            foreach (var stmtBody in script.StmtBodies)
            {
                var path = stmtBody.DSymbol.GetDeclSymbolId().Path;
                Debug.Assert(path != null);

                bodiesBuilder.Add(path, stmtBody.Stmts);
            }

            var bodies = bodiesBuilder.ToImmutable();

            return new IR0Loader(factory, script.ModuleDeclSymbol, bodies);
        }

        IR0Loader(SymbolFactory factory, ModuleDeclSymbol moduleDecl, ImmutableDictionary<DeclSymbolPath, ImmutableArray<Stmt>> bodies)
        {
            this.factory = factory;
            this.moduleDecl = moduleDecl;
            this.bodies = bodies;
        }

        // source from SymbolLoader
        ISymbolNode LoadSymbolCore(SymbolPath? path)
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
                var outer = LoadSymbolCore(path.Outer);
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

        public TSymbol LoadSymbol<TSymbol>(SymbolPath path)
            where TSymbol : class, ISymbolNode
        {
            var symbol = LoadSymbolCore(path) as TSymbol;
            if (symbol == null)
                throw new RuntimeFatalException();

            return symbol;
        }

        public ImmutableArray<Stmt> GetBody(DeclSymbolPath declPath)
        {
            if (bodies.TryGetValue(declPath, out var body))
                return body;

            throw new RuntimeFatalException();
        }
    }
}