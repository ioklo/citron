using System.Collections.Generic;
using Gum.Collections;
using System.Text;
using Gum.Infra;
using Pretune;

namespace Gum.IR0
{
    [AutoConstructor, ImplementIEquatable]
    public partial class StructDecl : Decl
    {
        public AccessModifier AccessModifier { get; }
        public string Name { get; }
        public ImmutableArray<string> TypeParams { get; }
        public ImmutableArray<Path> BaseTypes { get; }

        public override void EnsurePure()
        {
            Misc.EnsurePure(TypeParams);
            Misc.EnsurePure(BaseTypes);
        }
        // public ImmutableArray<Element> Elems { get; }
    }
}
