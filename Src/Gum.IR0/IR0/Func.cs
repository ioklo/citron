using Gum.Infra;
using Pretune;
using System;
using System.Collections.Generic;
using Gum.Collections;
using System.Linq;

namespace Gum.IR0
{   
    public abstract record FuncOuter;
    public record RootFuncOuter(ModuleName ModuleName, NamespacePath NamespacePath) : FuncOuter;
    public record TypeFuncOuter(Type Type) : FuncOuter;

    [AutoConstructor, ImplementIEquatable]
    public partial struct Func
    {
        public FuncOuter Outer { get; }
        public Name Name { get; }
        public ImmutableArray<Type> TypeArgs { get; }
    }
}