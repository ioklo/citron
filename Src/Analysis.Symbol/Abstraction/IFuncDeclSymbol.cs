﻿namespace Citron.Analysis
{
    public interface IFuncDeclSymbol : IDeclSymbolNode
    {
        int GetParameterCount();
        FuncParameter GetParameter(int index);

        void AddLambda(LambdaDeclSymbol symbol);
    }
}