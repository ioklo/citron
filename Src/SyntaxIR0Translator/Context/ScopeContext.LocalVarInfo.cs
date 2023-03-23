using Citron.Symbol;

namespace Citron.Analysis;

partial class ScopeContext
{
    public struct LocalVarInfo
    {
        public IType Type { get; }
        public Name Name { get; }

        public LocalVarInfo(IType type, Name name)
        {
            Name = name;
            Type = type;
        }

        public LocalVarInfo UpdateTypeValue(IType newType)
        {
            return new LocalVarInfo(newType, Name);
        }
    }
}