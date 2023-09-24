using System;
using Citron.Symbol;
using Citron.Collections;
using R = Citron.IR0;

namespace Citron.Analysis;

struct InstanceFuncBinder : IFuncSymbolVisitor<R.Exp>
{
    R.Loc instance;
    ImmutableArray<R.Argument> args;

    public static R.Exp Bind(IFuncSymbol funcS, R.Loc instance, ImmutableArray<R.Argument> args)
    {
        var binder = new InstanceFuncBinder { instance = instance, args = args };
        return funcS.Accept<InstanceFuncBinder, R.Exp>(ref binder);
    }

    R.Exp IFuncSymbolVisitor<R.Exp>.VisitClassConstructor(ClassConstructorSymbol symbol)
    {
        throw new NotImplementedException();
    }

    R.Exp IFuncSymbolVisitor<R.Exp>.VisitClassMemberFunc(ClassMemberFuncSymbol symbol)
    {
        return new R.CallClassMemberFuncExp(symbol, instance, args);
    }

    R.Exp IFuncSymbolVisitor<R.Exp>.VisitGlobalFunc(GlobalFuncSymbol symbol)
    {
        throw new NotImplementedException();
    }

    R.Exp IFuncSymbolVisitor<R.Exp>.VisitLambda(LambdaSymbol symbol)
    {
        throw new NotImplementedException();
    }

    R.Exp IFuncSymbolVisitor<R.Exp>.VisitStructConstructor(StructConstructorSymbol symbol)
    {
        throw new NotImplementedException();
    }

    R.Exp IFuncSymbolVisitor<R.Exp>.VisitStructMemberFunc(StructMemberFuncSymbol symbol)
    {
        return new R.CallStructMemberFuncExp(symbol, instance, args);
    }
}

