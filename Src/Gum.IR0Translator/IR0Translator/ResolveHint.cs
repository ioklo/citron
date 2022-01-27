using Gum.Analysis;
using Pretune;
using System;

namespace Gum.IR0Translator
{
    [AutoConstructor]
    partial struct ResolveHint
    {
        public static readonly ResolveHint None = new ResolveHint(NoneTypeHint.Instance);

        public TypeHint TypeHint { get; }        

        public static ResolveHint Make(ITypeSymbol typeValue)
        {
            return new ResolveHint(new TypeSymbolTypeHint(typeValue));
        }
    }
}