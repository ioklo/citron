using Gum.Misc;
using System.Collections.Immutable;
using M = Gum.CompileTime;

namespace Gum.IR0
{
    abstract class ItemResult
    {
    }

    class NotFoundItemResult : ItemResult 
    {
        public static NotFoundItemResult Instance { get; } = new NotFoundItemResult();
        NotFoundItemResult() { }
    }

    class MultipleCandidatesItemResult : ItemResult
    {
        public static MultipleCandidatesItemResult Instance { get; } = new MultipleCandidatesItemResult();
        MultipleCandidatesItemResult() { }
    }

    class ValueItemResult : ItemResult
    {
        public ItemValue ItemValue { get; }
        public ValueItemResult(ItemValue itemValue) { ItemValue = itemValue; }
    }
}