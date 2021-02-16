using Gum.Misc;
using System.Collections.Immutable;
using M = Gum.CompileTime;

namespace Gum.IR0Translator
{
    // Error/NotFound/Value
    abstract class ItemResult
    {
    }

    abstract class ErrorItemResult : ItemResult { }

    class VarWithTypeArgErrorItemResult : ErrorItemResult
    {
        public static VarWithTypeArgErrorItemResult Instance { get; } = new VarWithTypeArgErrorItemResult();
        VarWithTypeArgErrorItemResult() { }
    }

    class MultipleCandidatesErrorItemResult : ErrorItemResult
    {
        public static MultipleCandidatesErrorItemResult Instance { get; } = new MultipleCandidatesErrorItemResult();
        MultipleCandidatesErrorItemResult() { }
    }

    class NotFoundItemResult : ItemResult 
    {
        public static NotFoundItemResult Instance { get; } = new NotFoundItemResult();
        NotFoundItemResult() { }
    }
    
    class ValueItemResult : ItemResult
    {
        public ItemValue ItemValue { get; }
        public ValueItemResult(ItemValue itemValue) { ItemValue = itemValue; }
    }
}