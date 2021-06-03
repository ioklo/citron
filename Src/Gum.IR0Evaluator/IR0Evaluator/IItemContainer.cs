using R = Gum.IR0;

namespace Gum.IR0Evaluator
{
    public interface IItemContainer
    {
        IItemContainer GetContainer(R.Name name, R.ParamHash paramHash);
        TRuntimeItem GetRuntimeItem<TRuntimeItem>(R.Name name, R.ParamHash paramHash)
            where TRuntimeItem : RuntimeItem;
    }
}