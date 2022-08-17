using Citron.Collections;
using Citron.Symbol;
using System;

namespace Citron.Syntax
{
    // 직접 InternalModuleDeclSymbol을 만들지 않고 TypeExpInfo를 Syntax Node에 먼저 만들고 나서 하는 이유
    // ModuleDeclSymbol 없는 상태에서 GetMemberInfo를 하려면 정보가 필요하다
    
    // X<int>.Y<T>, closed
    public abstract class TypeExpInfo
    {
        public abstract TypeExpInfoKind GetKind();
        public abstract SymbolId GetSymbolId();
        // memberTypeExp: 리턴할 TypeExpInfo를 생성하는데 필요한 typeExp
        public abstract TypeExpInfo? GetMemberInfo(string name, ImmutableArray<SymbolId> typeArgs, TypeExp memberTypeExp);
        public abstract TypeExp GetTypeExp(); // back reference
    }
}