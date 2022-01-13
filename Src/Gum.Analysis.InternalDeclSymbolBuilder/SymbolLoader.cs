using Gum.Infra;
using System;
using Gum.Collections;
using Pretune;

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
    }
}