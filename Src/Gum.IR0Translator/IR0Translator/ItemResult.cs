using Gum.Infra;
using Gum.Collections;
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
        public static readonly VarWithTypeArgErrorItemResult Instance = new VarWithTypeArgErrorItemResult();
        VarWithTypeArgErrorItemResult() { }
    }

    class MultipleCandidatesErrorItemResult : ErrorItemResult
    {
        public static readonly MultipleCandidatesErrorItemResult Instance = new MultipleCandidatesErrorItemResult();
        MultipleCandidatesErrorItemResult() { }
    }

    class NotFoundItemResult : ItemResult 
    {
        public static readonly NotFoundItemResult Instance = new NotFoundItemResult();
        NotFoundItemResult() { }
    }
    
    class ValueItemResult : ItemResult
    {
        public ItemValue ItemValue { get; }
        public ValueItemResult(ItemValue itemValue) { ItemValue = itemValue; }
    }
}