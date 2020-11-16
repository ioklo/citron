using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;

namespace Gum.CompileTime
{
    public struct AppliedItemPathEntry
    {
        public Name Name { get ; }
        public string ParamHash { get ; }   // 함수에서 파라미터에 따라 달라지는 값        
        public ImmutableArray<TypeValue> TypeArgs { get; }

        public AppliedItemPathEntry(Name name, string paramHash = "")
        {
            Name = name;
            ParamHash = paramHash;
            TypeArgs = ImmutableArray<TypeValue>.Empty;
        }
        
        public AppliedItemPathEntry(Name name, string paramHash, IEnumerable<TypeValue> typeArgs)
        {            
            Name = name;
            TypeArgs = typeArgs.ToImmutableArray();
            ParamHash = paramHash;            
        }

        public ItemPathEntry GetItemPathEntry()
        {
            return new ItemPathEntry(Name, TypeArgs.Length, ParamHash);
        }
    }
}
