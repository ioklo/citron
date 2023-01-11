﻿using Citron.Infra;

namespace Citron.Symbol
{
    // module, namespace
    public interface ITopLevelSymbolNode : ISymbolNode, ICyclicEqualityComparableClass<ITopLevelSymbolNode>
    {
        new ITopLevelSymbolNode Apply(TypeEnv typeEnv);
        // (Name Module, NamespacePath? NamespacePath) GetRootPath();

        SymbolQueryResult QueryMember(Name memberName, int typeParamCount);
    }
}