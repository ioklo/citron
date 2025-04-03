module Citron.SyntaxIR0Translator:ReExpToNLocTranslation;

import Citron.Ptr;
import Citron.Logger;
import Citron.RDecls;
import Citron.NDecls;

import :TranslationContext;
import :ScopeContext;
import :DesignatedErrorLogger;
import :ReExp;

namespace Citron::SyntaxIR0Translator {

NLocPtr TranslateReThisVarExpToNLoc(ReExp_ThisVar& reExp, TranslationContext& context) // nothrow
{
    return context.MakeThisLoc();
}

NLocPtr TranslateReClassVarExpToNLoc(ReExp_ClassVar& reExp, TranslationContext& context)
{
    if (reExp.hasExplicitInstance) // c.x, C.x 둘다 해당
    {
        NLocPtr instance = nullptr;
        
        if (reExp.explicitInstance != nullptr)
        {   
            auto designatedErrorLogger = context.MakeDesignatedErrorLogger(&Logger::Fatal_ResolveIdentifier_ExpressionIsNotLocation);

            instance = TranslateReExpToNLoc(*reExp.explicitInstance, /* bWrapExpAsLoc */ true, &designatedErrorLogger, context);
            if (!instance) return nullptr;
        }

        return MakePtr<NLoc_ClassVar>(std::move(instance), reExp.decl, reExp.typeArgs);
    }
    else // x, x (static) 둘다 해당
    {   
        NLocPtr nInstanceLoc = reExp.decl->IsStatic()? nullptr : context.MakeThisLoc();
        return MakePtr<NLoc_ClassVar>(std::move(nInstanceLoc), reExp.decl, reExp.typeArgs);
    }
}

NLocPtr TranslateReLocalVarExpToNLoc(ReExp_LocalVar& reExp)
{
    return MakePtr<NLoc_LocalVar>(RName_Normal(reExp.name), reExp.type);
}

NLocPtr TranslateReLambdaVarExpToNLoc(ReExp_LambdaVar& reExp)
{
    return MakePtr<NLoc_LambdaVar>(reExp.decl, reExp.typeArgs);
}

NLocPtr TranslateReStructVarExpToNLoc(ReExp_StructVar& reExp, TranslationContext& context)
{
    if (reExp.hasExplicitInstance) // c.x, C.x 둘다 해당
    {
        NLocPtr instance = nullptr;

        if (reExp.explicitInstance != nullptr)
        {
            auto designatedErrorLogger = context.MakeDesignatedErrorLogger(&Logger::Fatal_ResolveIdentifier_ExpressionIsNotLocation);

            instance = TranslateReExpToNLoc(*reExp.explicitInstance, /*bWrapExpAsLoc*/ true, &designatedErrorLogger, context);
            if (!instance)
                return nullptr;
        }

        return MakePtr<NLoc_StructVar>(instance, reExp.decl, reExp.typeArgs);
    }
    else // x, x (static) 둘다 해당
    {   
        // TODO: [10] box 함수 내부이면, local ptr대신 box ptr로 변경해야 한다
        NLocPtr nInstanceLoc = reExp.decl->IsStatic() ? nullptr : MakePtr<NLoc_LocalDeref>(context.MakeThisLoc());
        return MakePtr<NLoc_StructVar>(nInstanceLoc, reExp.decl, reExp.typeArgs);
    }
}

NLocPtr TranslateReEnumElemVarExpToNLoc(ReExp_EnumElemVar& reExp, TranslationContext& context)
{   
    auto designatedErrorLogger = context.MakeDesignatedErrorLogger(&Logger::Fatal_ResolveIdentifier_ExpressionIsNotLocation);

    auto nInstLoc = TranslateReExpToNLoc(*reExp.instance, /*bWrapExpAsLoc*/ true, &designatedErrorLogger, context);
    if (!nInstLoc) return nullptr;

    return MakePtr<NLoc_EnumElemVar>(nInstLoc, reExp.decl, reExp.typeArgs);
}

NLocPtr TranslateReListIndexerExpToNLoc(ReExp_ListIndexer& reExp, TranslationContext& context)
{
    auto designatedErrorLogger = context.MakeDesignatedErrorLogger(&Logger::Fatal_ResolveIdentifier_ExpressionIsNotLocation);

    auto nInstLoc = TranslateReExpToNLoc(*reExp.instance, /*bWrapExpAsLoc*/ true, &designatedErrorLogger, context);
    if (!nInstLoc) return nullptr;

    return MakePtr<NLoc_ListIndexer>(std::move(nInstLoc), reExp.index, reExp.itemType);
}

NLocPtr TranslateReLocalDerefExpToNLoc(ReExp_LocalDeref& reExp, TranslationContext& context)
{
    // *x, *G()
    auto designatedErrorLogger = context.MakeDesignatedErrorLogger(&Logger::Fatal_ResolveIdentifier_ExpressionIsNotLocation);

    auto nTargetLoc = TranslateReExpToNLoc(*reExp.target, /*bWrapExpAsLoc*/ true, &designatedErrorLogger, context);
    if (!nTargetLoc) return nullptr;

    return MakePtr<NLoc_LocalDeref>(std::move(nTargetLoc));
}

NLocPtr TranslateReBoxDerefExpToNLoc(ReExp_BoxDeref& reExp, TranslationContext& context)
{
    // *x, *G()
    auto designatedErrorLogger = context.MakeDesignatedErrorLogger(&Logger::Fatal_ResolveIdentifier_ExpressionIsNotLocation);

    auto nTargetLoc = TranslateReExpToNLoc(*reExp.target, /*bWrapExpAsLoc*/ true, &designatedErrorLogger, context);
    if (!nTargetLoc) return nullptr;

    return MakePtr<NLoc_BoxDeref>(std::move(nTargetLoc));
}

namespace {

class ReExpToNLocTranslator : public ReExpVisitor
{
    bool bWrapExpAsLoc;
    IDesignatedErrorLogger* notLocationErrorLogger;
    NLocPtr* result;

    TranslationContext& context;

public:
    ReExpToNLocTranslator(bool bWrapExpAsLoc, IDesignatedErrorLogger* notLocationErrorLogger, NLocPtr* result, TranslationContext& context)
        : bWrapExpAsLoc(bWrapExpAsLoc), notLocationErrorLogger(notLocationErrorLogger), result(result), context(context)
    {
    }

    void Visit(ReExp_ThisVar& exp) override
    {
        *result = TranslateReThisVarExpToNLoc(exp, context);
    }

    void Visit(ReExp_LocalVar& exp) override
    {
        *result = TranslateReLocalVarExpToNLoc(exp);
    }

    void Visit(ReExp_LambdaVar& exp) override
    {
        *result = TranslateReLambdaVarExpToNLoc(exp);
    }

    void Visit(ReExp_ClassVar& exp) override
    {
        *result = TranslateReClassVarExpToNLoc(exp, context);
    }

    void Visit(ReExp_StructVar& exp) override
    {
        *result = TranslateReStructVarExpToNLoc(exp, context);
    }

    void Visit(ReExp_EnumElemVar& exp) override
    {
        *result = TranslateReEnumElemVarExpToNLoc(exp, context);
    }

    void Visit(ReExp_LocalDeref& exp) override
    {
        *result = TranslateReLocalDerefExpToNLoc(exp, context);
    }

    void Visit(ReExp_BoxDeref& exp) override
    {
        *result = TranslateReBoxDerefExpToNLoc(exp, context);
    }

    void Visit(ReExp_ListIndexer& exp) override
    {
        *result = TranslateReListIndexerExpToNLoc(exp, context);
    }

    void Visit(ReExp_Else& exp) override
    {
        if (bWrapExpAsLoc)
        {
            *result = MakePtr<NLoc_Temp>(exp.nExp);
        }
        else
        {
            notLocationErrorLogger->Log();
            *result = nullptr;
        }
    }
};

}

NLocPtr TranslateReExpToNLoc(ReExp& reExp, bool bWrapExpAsLoc, IDesignatedErrorLogger* notLocationErrorLogger, TranslationContext& context)
{
    NLocPtr nLoc;
    ReExpToNLocTranslator translator(bWrapExpAsLoc, notLocationErrorLogger, &nLoc, context);
    reExp.Accept(translator);
    return nLoc;
}

}
