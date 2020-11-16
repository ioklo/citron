using Gum.CompileTime;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gum.IR0
{
    public static class TypeValues
    {
        public static TypeValue Bool { get; } = new TypeValue.Normal(ItemIds.Bool);
        public static TypeValue Int { get; } = new TypeValue.Normal(ItemIds.Int);
        public static TypeValue String { get; } = new TypeValue.Normal(ItemIds.String);
    }
}
