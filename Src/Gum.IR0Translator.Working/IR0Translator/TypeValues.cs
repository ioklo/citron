using M = Gum.CompileTime;
using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.Immutable;

namespace Gum.IR0Translator
{
    static class TypeValues
    {
        public static TypeValue Bool { get; } 
        public static TypeValue Int { get; }
        public static TypeValue String { get; }

        public static TypeValue List(params TypeValue[] typeArgs)
            => throw new NotImplementedException(); // TODO: TypeValueFactory가 있어서 Info를 제대로 받아야 한다

        static TypeValues()
        {
            // TODO: 임시
            M.TypeInfo MakeEmptyStructInfo(M.Name name)
                => new M.StructInfo(name, ImmutableArray<string>.Empty, ImmutableArray<M.Type>.Empty,
                ImmutableArray<M.TypeInfo>.Empty, ImmutableArray<M.FuncInfo>.Empty, ImmutableArray<M.MemberVarInfo>.Empty);

            Bool = NormalTypeValue.MakeExternalGlobal("System.Runtime", new M.NamespacePath("System"), MakeEmptyStructInfo("Bool"), ImmutableArray<TypeValue>.Empty);
            Int = NormalTypeValue.MakeExternalGlobal("System.Runtime", new M.NamespacePath("System"), MakeEmptyStructInfo("Int32"), ImmutableArray<TypeValue>.Empty);
            String = NormalTypeValue.MakeExternalGlobal("System.Runtime", new M.NamespacePath("System"), MakeEmptyStructInfo("String"), ImmutableArray<TypeValue>.Empty);
        }
    }
}
