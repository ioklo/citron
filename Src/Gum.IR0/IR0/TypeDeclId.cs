using Pretune;
using System.Diagnostics;

namespace Gum.IR0
{
    [ImplementIEquatable]
    public partial struct TypeDeclId
    {
        public int Value { get; }

        public enum PredefinedValue : int
        {
            Void = -1,
            Bool = -2,
            Int = -3,
            String = -4,
            Enumerable = -5,
            Lambda = -6,
            List = -7,
        }

        // Predefined Ids
        public static TypeDeclId Void = new TypeDeclId(PredefinedValue.Void);
        public static TypeDeclId Bool = new TypeDeclId(PredefinedValue.Bool);
        public static TypeDeclId Int = new TypeDeclId(PredefinedValue.Int);
        public static TypeDeclId String = new TypeDeclId(PredefinedValue.String);
        public static TypeDeclId Enumerable = new TypeDeclId(PredefinedValue.Enumerable);
        public static TypeDeclId Lambda = new TypeDeclId(PredefinedValue.Lambda);
        public static TypeDeclId List = new TypeDeclId(PredefinedValue.List);

        private TypeDeclId(PredefinedValue pv)
        {
            Value = (int)pv;
        }

        public TypeDeclId(int value)
        {
            Debug.Assert(0 <= value);
            Value = value;
        }
    }
}