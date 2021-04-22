using Pretune;

namespace Gum.IR0Translator
{
    abstract class TypeHint
    {
    }

    class NoneTypeHint : TypeHint 
    {
        public static readonly NoneTypeHint Instance = new NoneTypeHint();
        private NoneTypeHint() { }
    }

    class TypeValueTypeHint : TypeHint
    {
        public TypeValue TypeValue { get; }
        public TypeValueTypeHint(TypeValue typeValue) { TypeValue = typeValue; }
    }

    [AutoConstructor]
    partial class EnumConstructorTypeHint : TypeHint
    {
        public EnumTypeValue EnumTypeValue { get; }
    }
}