﻿namespace Citron.Symbol
{
    public interface IFuncDeclSymbol : IDeclSymbolNode
    {
        int GetParameterCount();
        FuncParameter GetParameter(int index);
    }
}