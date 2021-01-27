using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gum.Misc
{
    public static class HashExtensions
    {
        public static void AddSequence<TElem>(ref this HashCode hashCode, ImmutableArray<TElem> array)
        {
            foreach (var elem in array)
                hashCode.Add(elem);
        }
    }
}
