namespace Gum.IR0Translator
{
    abstract class TypeHint
    {
    }

    class NontTypeHint : TypeHint 
    {
        public static readonly NontTypeHint Instance = new NontTypeHint();
        private NontTypeHint() { }
    }

    class TypeValueTypeHint : TypeHint
    {
        public TypeValue TypeValue { get; }
        public TypeValueTypeHint(TypeValue typeValue) { TypeValue = typeValue; }
    }
}