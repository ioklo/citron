using System;

using Citron.Infra;
using Citron.Module;

namespace Citron.Symbol
{
    // TypeVar
    // 두개의 안중 2안을 선택한다
    // void Func<T>(T t); 의 시그니처
    // 1. Func<T>.T Func<T>(Func<T>.T t)
    // 2. Func<>.T Func<T>(Func<>.T t)
    public class TypeVarSymbol : ITypeSymbol
    {
        int index;

        ISymbolNode ISymbolNode.Apply(TypeEnv typeEnv)
            => Apply(typeEnv);

        ITypeSymbol ITypeSymbol.Apply(TypeEnv typeEnv)
            => Apply(typeEnv);

        internal TypeVarSymbol(int index)
        {
            this.index = index;
        }
        
        public ITypeSymbol Apply(TypeEnv typeEnv)
        {
            // 정확히 뭘 하는지 생각하고 작성한다
            throw new NotImplementedException();

            // var appliedOuter = outer.Apply(typeEnv);
            // return factory.MakeTypeVar(appliedOuter, decl, appliedTypeArgs);

            // return typeEnv.GetValue(index)            
        }

        void ITypeSymbol.Apply(ITypeSymbolVisitor visitor)
        {
            visitor.VisitTypeVar(this);
        }

        IDeclSymbolNode? ISymbolNode.GetDeclSymbolNode()
            => null;


        ITypeDeclSymbol? ITypeSymbol.GetDeclSymbolNode()
            => null;

        // 이 심볼의 outer는 누구인가, 최상위면 null일 것이고, 정의되지 않았으면 exception
        ISymbolNode? ISymbolNode.GetOuter()
        {
            throw new NotImplementedException();
        }

        ITypeSymbol ISymbolNode.GetTypeArg(int i)
        {
            throw new RuntimeFatalException();
        }

        TypeEnv ISymbolNode.GetTypeEnv()
        {
            throw new NotImplementedException();
        }

        SymbolQueryResult ITypeSymbol.QueryMember(Name memberName, int typeParamCount)
        {
            return SymbolQueryResults.NotFound;
        }
    }
}
