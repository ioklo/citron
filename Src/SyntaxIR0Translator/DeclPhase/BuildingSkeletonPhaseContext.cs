using System;
using System.Collections.Generic;
using Citron.Collections;
using Citron.Symbol;
using Citron.Syntax;

namespace Citron.Analysis
{
    // 1. BuildingSkeletonPhase
    class BuildingSkeletonPhaseContext
    {
        List<Action<BuildingMemberDeclPhaseContext>> buildingMemberDeclPhaseTasks;        

        public BuildingSkeletonPhaseContext()
        {
            buildingMemberDeclPhaseTasks = new List<Action<BuildingMemberDeclPhaseContext>>();
        }

        public void AddBuildingMemberDeclPhaseTask(Action<BuildingMemberDeclPhaseContext> task)
        {
            buildingMemberDeclPhaseTasks.Add(task);
        }
        
        public void BuildMemberDecl(BuildingMemberDeclPhaseContext context)
        {
            foreach (var task in buildingMemberDeclPhaseTasks)
            {
                task.Invoke(context);
            }
        }
    }
}