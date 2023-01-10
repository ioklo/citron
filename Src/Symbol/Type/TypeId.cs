using Citron.Collections;
using Citron.Infra;

using Name = Citron.Module.Name;

namespace Citron.Symbol
{
    public abstract record class TypeId;


    // MyModule.MyClass<X, Y>.MyStruct<T, U, X>.T => 2 (Index는 누적)
    // declId를 참조하게 만들지 않는 이유, FuncParamId 등을 만들기가 어렵다 (순환참조가 발생하기 쉽다)    
    // public record class TypeVarSymbolId(int Index) : SymbolId;    
    // => TypeVarSymbolId도 ModuleSymbolId의 일부분으로 통합한다. 사용할 때 resolution이 필요할거 같지만 큰 문제는 아닌 것 같다
    // 'MyModule.MyClass<X, Y>.MyStruct<T, U, X>.X'
    // => 순환참조때문에 누적 Index를 사용하는 TypeVarSymbolId로 다시 롤백한다
    // 'MyModule.MyClass<X, Y>.MyStruct<T, U, X>.Func<T>(T, int).T' path에 Func<T>와 T가 순환 참조된다
    // => TypeVarSymbolId(5)로 참조하게 한다
    public record class TypeVarTypeId(int Index, Name Name) : TypeId;

    public record class NullableTypeId(TypeId InnerTypeId) : TypeId;

    public record class VoidTypeId : TypeId;

    public record class TupleTypeId(ImmutableArray<(TypeId TypeId, Name Name)> MemberVarIds) : TypeId;

    public record class VarTypeId : TypeId;

    public record class LambdaTypeId : TypeId;

    public static class TypeIds
    {
        public readonly static VoidTypeId Void = new VoidTypeId();
        public readonly static TypeId Bool = new SymbolId(new Name.Normal("System.Runtime"), null).Child(new Name.Normal("System")).Child(new Name.Normal("Boolean"));
        public readonly static TypeId Int = new SymbolId(new Name.Normal("System.Runtime"), null).Child(new Name.Normal("System")).Child(new Name.Normal("Int32"));
        public readonly static TypeId String = new SymbolId(new Name.Normal("System.Runtime"), null).Child(new Name.Normal("System")).Child(new Name.Normal("String"));
    }
}
