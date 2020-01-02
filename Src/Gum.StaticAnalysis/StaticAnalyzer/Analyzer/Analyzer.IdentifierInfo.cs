using Gum.CompileTime;

namespace Gum.StaticAnalysis
{
    public partial class Analyzer
    {
        public class IdentifierInfo
        {
            public class Var : IdentifierInfo
            {
                public StorageInfo StorageInfo { get; }
                public TypeValue TypeValue { get; }

                public Var(StorageInfo storageInfo, TypeValue typeValue)
                {
                    StorageInfo = storageInfo;
                    TypeValue = typeValue;
                }
            }

            public class Func : IdentifierInfo
            {
                public FuncValue FuncValue { get; }
                public Func(FuncValue funcValue)
                {
                    FuncValue = funcValue;
                }
            }

            public class Type : IdentifierInfo
            {
                public TypeValue.Normal TypeValue { get; }
                public Type(TypeValue.Normal typeValue)
                {
                    TypeValue = typeValue;
                }
            }

            public class EnumElem : IdentifierInfo
            {
                public TypeValue.Normal EnumTypeValue { get; }
                public EnumElemInfo ElemInfo { get; }

                public EnumElem(TypeValue.Normal enumTypeValue, EnumElemInfo elemInfo)
                {
                    EnumTypeValue = enumTypeValue;
                    ElemInfo = elemInfo;
                }
            }

            public static Var MakeVar(StorageInfo storageInfo, TypeValue typeValue) => new Var(storageInfo, typeValue);

            public static Func MakeFunc(FuncValue funcValue) => new Func(funcValue);

            public static Type MakeType(TypeValue.Normal typeValue) => new Type(typeValue);

            public static EnumElem MakeEnumElem(TypeValue.Normal enumTypeValue, EnumElemInfo elemInfo) => new EnumElem(enumTypeValue, elemInfo);
        }
    }
}
