using Citron.Collections;
using Citron.Infra;
using Pretune;

namespace Citron.Symbol
{    
    public record class SymbolPath : ISerializable
    {
        public SymbolPath? Outer { get; set; } // 오픈한 이유는
        public Name Name { get; }
        public ImmutableArray<TypeId> TypeArgs { get; }
        public ImmutableArray<FuncParamId> ParamIds { get; } 

        public SymbolPath(SymbolPath? outer, Name name, ImmutableArray<TypeId> typeArgs = default, ImmutableArray<FuncParamId> paramIds = default)
        {
            Outer = outer;
            Name = name;
            TypeArgs = typeArgs;
            ParamIds = paramIds;
        }

        void ISerializable.DoSerialize(ref SerializeContext context)
        {
            context.SerializeRef(nameof(Outer), Outer);
            context.SerializeRef(nameof(Name), Name);
            context.SerializeRefArray(nameof(TypeArgs), TypeArgs);
            context.SerializeValueArray(nameof(ParamIds), ParamIds);
        }
    }

    public static class SymbolPathExtensions
    {
        public static SymbolPath Child(this SymbolPath? outer, Name name, ImmutableArray<TypeId> typeArgs = default, ImmutableArray<FuncParamId> paramIds = default)
        {
            return new SymbolPath(outer, name, typeArgs, paramIds);
        }
    }
}