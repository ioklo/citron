using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gum.Core.IL
{
    public class GlobalDomain : Domain
    {
        public static GlobalDomain Instance { get; private set; }

        public static VoidType VoidType { get; private set; }
        public static Gum.Core.IL.ValueType BoolType { get; private set; }
        public static Gum.Core.IL.ValueType IntType { get; private set; }
        public static Gum.Core.IL.RefType StringType { get; private set; }
        public static Gum.Core.IL.RefType TypeType { get; private set; }
        public static Gum.Core.IL.RefType RefType { get; private set; } // 일반적인 RefType

        static GlobalDomain()
        {
            Instance = new GlobalDomain();
            VoidType = new VoidType();

            BoolType = new Gum.Core.IL.ValueType();
            IntType = new Gum.Core.IL.ValueType(); 
            StringType = new Gum.Core.IL.RefType("string", Enumerable.Empty<IType>());
            TypeType = new Gum.Core.IL.RefType("type", Enumerable.Empty<IType>());
            RefType = new Gum.Core.IL.RefType("$ref", Enumerable.Empty<IType>());

            Instance.AddValue("void", new TypeValue(VoidType));
            Instance.AddValue("bool", new TypeValue(BoolType));
            Instance.AddValue("int", new TypeValue(IntType));
            Instance.AddValue("string", new TypeValue(StringType));
            Instance.AddValue("Type", new TypeValue(TypeType));
        }

        public static FunctionType FuncType(IType retType, params IType[] argTypes)
        {
            return new FunctionType(retType, argTypes);
        }
    }
}
