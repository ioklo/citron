﻿using System.Collections.Generic;
using R = Gum.IR0;

namespace Gum.IR0Evaluator
{
    public partial class Evaluator
    {
        class ItemContainer
        {
            Dictionary<(R.Name, R.ParamHash), ItemContainer> containers;
            Dictionary<(R.Name, R.ParamHash), RuntimeItem> runtimeItems;

            public ItemContainer()
            {
                containers = new Dictionary<(R.Name, R.ParamHash), ItemContainer>();
                runtimeItems = new Dictionary<(R.Name, R.ParamHash), RuntimeItem>();               
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
        }
    }    
}