using System.Collections.Generic;
using R = Gum.IR0;

namespace Gum.IR0Evaluator
{
    public partial class Evaluator
    {
        class ItemContainer
        {
            Dictionary<(R.Name, R.ParamHash), ItemContainer> containers;
            Dictionary<(R.Name, R.ParamHash), RuntimeItem> runtimeItems;            
            
            Dictionary<R.Name, R.EnumElement> enumElems;

            public ItemContainer()
            {
                containers = new Dictionary<(R.Name, R.ParamHash), ItemContainer>();
                runtimeItems = new Dictionary<(R.Name, R.ParamHash), RuntimeItem>();               
                
                enumElems = new Dictionary<R.Name, R.EnumElement>();
            }

            public ItemContainer GetContainer(R.Name name, R.ParamHash paramHash)
            {
                return containers[(name, paramHash)];
            }

            public TRuntimeItem GetRuntimeItem<TRuntimeItem>(R.Name name, R.ParamHash paramHash)
                where TRuntimeItem : RuntimeItem
            {
                return (TRuntimeItem)runtimeItems[(name, paramHash)];
            }
            
            public void AddItemContainer(R.Name name, R.ParamHash paramHash, ItemContainer itemContainer)
            {
                containers.Add((name, paramHash), itemContainer);
            }
            
            public void AddRuntimeItem(RuntimeItem runtimeItem)
            {
                runtimeItems.Add((runtimeItem.Name, runtimeItem.ParamHash), runtimeItem);
            }
            
            public void AddEnumElem(R.EnumElement enumElem)
            {
                enumElems.Add(enumElem.Name, enumElem);
            }

            public R.EnumElement GetEnumElem(R.Name name)
            {
                return enumElems[name];
            }
        }
    }    
}
