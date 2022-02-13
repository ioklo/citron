using Gum.Collections;
using System;
using System.Diagnostics;
using M = Gum.CompileTime;
using static Gum.CompileTime.SymbolIdExtensions;

namespace Citron.Analysis
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
            if (decl == null) return 0;

            return decl.GetTotalTypeParamCount();
        }

        public static bool CanAccess(this ISymbolNode user, ISymbolNode target)
        {
            var targetDecl = target.GetDeclSymbolNode();
            var userDecl = user.GetDeclSymbolNode();
            Debug.Assert(targetDecl != null && userDecl != null);

            return userDecl.CanAccess(targetDecl);
        }
        
        public static M.SymbolId GetSymbolId(this ISymbolNode symbol)
        {
            var outer = symbol.GetOuter();
            if (outer != null)
            {
                var outerId = outer.GetSymbolId();

                if (outerId is M.ModuleSymbolId outerModuleId)
                {
                    var decl = symbol.GetDeclSymbolNode();
                    Debug.Assert(decl != null); // ModuleSymbolId이기 때문에 무조건 있다

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
                            return new M.ModuleSymbolId(moduleDecl.GetNodeName().Name, null);
                        }

                    // case NullableSymbol: return new NullableSymbolId();

                    default:
                        throw new NotImplementedException();
                }

            }
        }
    }
}