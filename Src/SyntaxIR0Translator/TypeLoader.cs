using System;

using Citron.Infra;
using Citron.Symbol;

using Pretune;

namespace Citron.Analysis;

[AutoConstructor]
public partial struct TypeLoader
{
    SymbolLoader symbolLoader;

    public IType Load(TypeId id)
    {
        switch (id)
        {
            case SymbolId symbolId:
                return ((ITypeSymbol)symbolLoader.Load(symbolId)).MakeType();

            case VarTypeId:
                return new VarType();

            case VoidTypeId voidTypeId:
                return new VoidType();

            case NullableTypeId nullableId:
                throw new NotImplementedException(); // NullableSymbol

            // class C<T> { class D<U> { "여기를 분석할 때" } }
            // 분석중인 Decl환경에서 C<T>.D<U>
            // TypeVarSymbolId는 index만 갖고 있다 (1이면 U이다)
            // 그럼 지금 위치 (C<T>.D<U>)를 넘겨주던가
            // [T, U] 리스트를 넘겨주던가 해야한다
            case TypeVarTypeId typeVarId: // 3이러면 어떻게 아는가
                return new TypeVarType(typeVarId.Index, typeVarId.Name);

            default:
                throw new UnreachableCodeException();
        }
    }
}