using Gum.Collections;
using System;
using M = Gum.CompileTime;


namespace Gum.Analysis
{
    // 자식으로 가는 줄기는 없다
    public interface ISymbolNode
    {
        // 순회
        ISymbolNode? GetOuter();
        IDeclSymbolNode? GetDeclSymbolNode();
        
        TypeEnv GetTypeEnv();
        ISymbolNode Apply(TypeEnv typeEnv);        
        ImmutableArray<ITypeSymbol> GetTypeArgs();
    }

    public static class SymbolNodeExtensions
    {
        public static int GetTotalTypeParamCount(this ISymbolNode symbol)
        {
            var decl = symbol.GetDeclSymbolNode();
            return decl.GetTotalTypeParamCount();
        }
        
        public static SymbolId GetSymbolId(this ISymbolNode symbol)
        {
            var outer = symbol.GetOuter();
            if (outer != null)
            {
                var outerId = outer.GetSymbolId();

                if (outerId is ModuleSymbolId outerModuleId)
                {
                    var decl = symbol.GetDeclSymbolNode();
                    var declName = decl.GetNodeName();
                    var typeArgIds = ImmutableArray.CreateRange(symbol.GetTypeArgs(), typeArg => typeArg.GetSymbolId());                    

                    return outerModuleId.Child(declName.Name, typeArgIds, declName.ParamIds);
                }
                else
                {
                    throw new NotImplementedException(); // 에러 처리
                }
            }
            else
            {
                // 루트 노드는 몇 안된다
                switch(symbol)
                {   
                    case ModuleSymbol module:
                        var moduleDecl = module.GetDeclSymbolNode();
                        {
                            return new ModuleSymbolId(moduleDecl.GetNodeName().Name, null);
                        }

                    // case NullableSymbol: return new NullableSymbolId();

                    default:
                        throw new NotImplementedException();
                }

            }
        }
    }
}