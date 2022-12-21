using System;
using System.Collections.Generic;

namespace Citron.Analysis
{
    // 1. BuildingSkeletonPhase
    class BuildingSkeletonPhaseContext
    {
        List<Action<BuildingFuncDeclPhaseContext>> tasks;

        public BuildingSkeletonPhaseContext()
        {
            tasks = new List<Action<BuildingFuncDeclPhaseContext>>();
        }

        public void RegisterTaskAfterBuildingAllTypeDeclSymbols(Action<BuildingFuncDeclPhaseContext> task)
        {
            tasks.Add(task);
        }

        public void DoRegisteredTasks(BuildingFuncDeclPhaseContext context)
        {
            foreach(var task in tasks)
            {
                task.Invoke(context);
            }
        }
    }
}