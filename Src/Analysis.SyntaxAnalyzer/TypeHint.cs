﻿using Pretune;

namespace Citron.Analysis
{
    abstract class TypeHint
    {
    }

    class NoneTypeHint : TypeHint 
    {
        public static readonly NoneTypeHint Instance = new NoneTypeHint();
        private NoneTypeHint() { }
    }

    class TypeSymbolTypeHint : TypeHint
    {
        public ITypeSymbol TypeSymbol { get; }
        public TypeSymbolTypeHint(ITypeSymbol typeValue) { TypeSymbol = typeValue; }
    }

    // Callable분석에서 쓰인다. E.F()에서 E.F만을 가리켜야 할 때
    [AutoConstructor]
    partial class EnumConstructorTypeHint : TypeHint
    {
        public EnumSymbol EnumTypeValue { get; }
    }
}