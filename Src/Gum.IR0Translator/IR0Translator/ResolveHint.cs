using Pretune;
using System;

namespace Gum.IR0Translator
{
    [AutoConstructor]
    partial struct ResolveHint
    {
        public static readonly ResolveHint None = new ResolveHint(NoneTypeHint.Instance, NoneFuncParamHint.Instance);

        public TypeHint TypeHint { get; }
        public FuncParamHint FuncParamHint { get; }

        public static ResolveHint Make(TypeValue typeValue)
        {
            return new ResolveHint(new TypeValueTypeHint(typeValue), NoneFuncParamHint.Instance);
        }
    }
}