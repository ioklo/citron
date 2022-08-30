using System;

using Citron.Infra;

namespace Citron.Symbol
{
    public class TypeVarSymbol : ISymbolNode
    {
        SymbolFactory factory;
        ISymbolNode outer;
        TypeVarDeclSymbol decl;

        internal TypeVarSymbol(SymbolFactory factory, ISymbolNode outer, TypeVarDeclSymbol decl)
        {
            this.factory = factory;
            this.outer = outer;
            this.decl = decl;
        }

        // 이 함수는 void F<T>() { C<T> c; }
        ISymbolNode ISymbolNode.Apply(TypeEnv typeEnv)
        {
            // 정확히 뭘 하는지 생각하고 작성한다
            throw new NotImplementedException();

            // var appliedOuter = outer.Apply(typeEnv);
            // return factory.MakeTypeVar(appliedOuter, decl, appliedTypeArgs);

            // return typeEnv.GetValue(index)            
        }

        IDeclSymbolNode? ISymbolNode.GetDeclSymbolNode()
        {
            return decl;
        }

        ISymbolNode? ISymbolNode.GetOuter()
        {
            return outer;
        }

        ITypeSymbol ISymbolNode.GetTypeArg(int i)
        {
            throw new RuntimeFatalException();
        }

        TypeEnv ISymbolNode.GetTypeEnv()
        {
            return outer.GetTypeEnv();
        }
    }
}
