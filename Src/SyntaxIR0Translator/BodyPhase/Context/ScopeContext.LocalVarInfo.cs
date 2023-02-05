using Citron.Symbol;

namespace Citron.Analysis;

partial class ScopeContext
{
    public struct LocalVarInfo
    {
        public bool IsRef { get; }
        public IType Type { get; }
        public Name Name { get; }

        public LocalVarInfo(bool bRef, IType type, Name name)
        {
            IsRef = bRef;
            Name = name;
            Type = type;
        }

        public LocalVarInfo UpdateTypeValue(IType newType)
        {
            return new LocalVarInfo(IsRef, newType, Name);
        }
    }
}