using System.Diagnostics;

namespace Gum.IR0
{
    public struct TypeId
    {
        public enum PredefinedValue
        {
            Void = -1,
            Bool = -2,
            Int = -3,
            String = -4,
            Enumerable = -5,
        }

        // Predefined Ids
        public static TypeId Void { get; } = new TypeId(PredefinedValue.Void);
        public static TypeId Bool { get; } = new TypeId(PredefinedValue.Bool);
        public static TypeId Int { get; } = new TypeId(PredefinedValue.Int);
        public static TypeId String { get; } = new TypeId(PredefinedValue.String);
        public static TypeId Enumerable { get; } = new TypeId(PredefinedValue.Enumerable);

        public int Value { get; }
        

        private TypeId(PredefinedValue pv)
        {
            Value = (int)pv;
        }

        public TypeId(int value)
        {
            Debug.Assert(0 <= value);
            Value = value;
        }
    }
}