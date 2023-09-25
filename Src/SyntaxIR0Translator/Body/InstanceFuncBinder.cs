using System;
using Citron.Symbol;
using Citron.Collections;
using Citron.IR0;
using Pretune;

namespace Citron.Analysis;

using Result = TranslationResult<IR0ExpResult>;
using ITV = ITypeVisitor<TranslationResult<IR0ExpResult>>;
using ISQRV = ISymbolQueryResultVisitor<TranslationResult<IR0ExpResult>>;
using SQR = SymbolQueryResult;

struct InstanceFuncSymbolBinder : IFuncSymbolVisitor<Exp>
{
    Loc instance;
    ImmutableArray<Argument> args;

    public static Exp Bind(IFuncSymbol funcS, Loc instance, ImmutableArray<Argument> args)
    {
        var binder = new InstanceFuncSymbolBinder { instance = instance, args = args };
        return funcS.Accept<InstanceFuncSymbolBinder, Exp>(ref binder);
    }

    Exp IFuncSymbolVisitor<Exp>.VisitClassConstructor(ClassConstructorSymbol symbol)
    {
        throw new NotImplementedException();
    }

    Exp IFuncSymbolVisitor<Exp>.VisitClassMemberFunc(ClassMemberFuncSymbol symbol)
    {
        return new CallClassMemberFuncExp(symbol, instance, args);
    }

    Exp IFuncSymbolVisitor<Exp>.VisitGlobalFunc(GlobalFuncSymbol symbol)
    {
        throw new NotImplementedException();
    }

    Exp IFuncSymbolVisitor<Exp>.VisitLambda(LambdaSymbol symbol)
    {
        throw new NotImplementedException();
    }

    Exp IFuncSymbolVisitor<Exp>.VisitStructConstructor(StructConstructorSymbol symbol)
    {
        throw new NotImplementedException();
    }

    Exp IFuncSymbolVisitor<Exp>.VisitStructMemberFunc(StructMemberFuncSymbol symbol)
    {
        return new CallStructMemberFuncExp(symbol, instance, args);
    }
}
