#include "ReExpToNLocTranslation.h"

#include "Infra/Ptr.h"
#include "Logging/Logger.h"
#include "IR0/RClassVarDecl.h"
#include "IR0/RStructVarDecl.h"
#include "IR0/NLambdaVarDecl.h"
#include "IR0/NLoc.h"

#include "TranslationContext.h"
#include "ScopeContext.h"
#include "DesignatedDiagnostic.h"
#include "ReExp.h"

using namespace std;

namespace Citron::SyntaxIR0Translator {

expected<NLoc*, DiagPtr> TranslateReThisVarExpToNLoc(ReExp_ThisVar& reExp, TranslationContext& context) // nothrow
{
    return context.MakeThisLoc();
}

expected<NLoc*, DiagPtr> TranslateReClassVarExpToNLoc(ReExp_ClassVar& reExp, TranslationContext& context)
{
    if (reExp.hasExplicitInstance) // c.x, C.x 둘다 해당
    {
        NLoc* instance = nullptr;
        
        if (reExp.explicitInstance != nullptr)
        {   
            DesignatedDiagnostic<Error_ResolveIdentifier_ExpressionIsNotLocation> designatedDiag;
            auto eInstance = TranslateReExpToNLoc(*reExp.explicitInstance, /* bWrapExpAsLoc */ true, &designatedDiag, context);

            if (!eInstance)
                return unexpected{move(eInstance).error()};

            instance = *eInstance;
        }

        return MakePtr<NLoc_ClassVar>(move(instance), reExp.decl, reExp.typeArgs);
    }
    else // x, x (static) 둘다 해당
    {   
        NLoc* nInstanceLoc = reExp.decl->IsStatic()? nullptr : context.MakeThisLoc();
        return MakePtr<NLoc_ClassVar>(move(nInstanceLoc), reExp.decl, reExp.typeArgs);
    }
}

expected<NLoc*, DiagPtr> TranslateReLocalVarExpToNLoc(ReExp_LocalVar& reExp)
{
    return MakePtr<NLoc_LocalVar>(RName_Normal(reExp.name), reExp.type);
}

expected<NLoc*, DiagPtr> TranslateReLambdaVarExpToNLoc(ReExp_LambdaVar& reExp)
{
    return MakePtr<NLoc_LambdaVar>(reExp.decl, reExp.typeArgs);
}

expected<NLoc*, DiagPtr> TranslateReStructVarExpToNLoc(ReExp_StructVar& reExp, TranslationContext& context)
{
    if (reExp.hasExplicitInstance) // c.x, C.x 둘다 해당
    {
        NLoc* instance = nullptr;

        if (reExp.explicitInstance != nullptr)
        {
            DesignatedDiagnostic<Error_ResolveIdentifier_ExpressionIsNotLocation> designatedDiag;
            auto eInstance = TranslateReExpToNLoc(*reExp.explicitInstance, /*bWrapExpAsLoc*/ true, &designatedDiag, context);
            if (!eInstance) return unexpected{move(eInstance).error()};

            instance = *eInstance;
        }

        return MakePtr<NLoc_StructVar>(instance, reExp.decl, reExp.typeArgs);
    }
    else // x, x (static) 둘다 해당
    {   
        // TODO: [10] box 함수 내부이면, local ptr대신 box ptr로 변경해야 한다
        NLoc* nInstanceLoc = reExp.decl->IsStatic() ? nullptr : MakePtr<NLoc_LocalDeref>(context.MakeThisLoc());
        return MakePtr<NLoc_StructVar>(nInstanceLoc, reExp.decl, reExp.typeArgs);
    }
}

expected<NLoc*, DiagPtr> TranslateReEnumElemVarExpToNLoc(ReExp_EnumElemVar& reExp, TranslationContext& context)
{   
    DesignatedDiagnostic<Error_ResolveIdentifier_ExpressionIsNotLocation> designatedDiag;

    auto eInst = TranslateReExpToNLoc(*reExp.instance, /*bWrapExpAsLoc*/ true, &designatedDiag, context);
    if (!eInst) return unexpected{move(eInst).error()};

    return MakePtr<NLoc_EnumElemVar>(*eInst, reExp.decl, reExp.typeArgs);
}

expected<NLoc*, DiagPtr> TranslateReListIndexerExpToNLoc(ReExp_ListIndexer& reExp, TranslationContext& context)
{
    DesignatedDiagnostic<Error_ResolveIdentifier_ExpressionIsNotLocation> designatedDiag;

    auto eInst = TranslateReExpToNLoc(*reExp.instance, /*bWrapExpAsLoc*/ true, &designatedDiag, context);
    if (!eInst) return unexpected{move(eInst).error()};

    auto eIndex = TranslateReExpToNLoc(*reExp.index, /*bWrapExpAsLoc*/ true, &designatedDiag, context);
    if (!eIndex) return unexpected{move(eIndex).error()};

    return MakePtr<NLoc_ListIndexer>(move(*eInst), move(*eIndex), reExp.itemType);
}

expected<NLoc*, DiagPtr> TranslateReLocalDerefExpToNLoc(ReExp_LocalDeref& reExp, TranslationContext& context)
{
    // *x, *G()
    DesignatedDiagnostic<Error_ResolveIdentifier_ExpressionIsNotLocation> designatedDiag;

    auto eTarget = TranslateReExpToNLoc(*reExp.target, /*bWrapExpAsLoc*/ true, &designatedDiag, context);
    if (!eTarget) return unexpected{move(eTarget).error()};

    return MakePtr<NLoc_LocalDeref>(move(*eTarget));
}

expected<NLoc*, DiagPtr> TranslateReBoxDerefExpToNLoc(ReExp_BoxDeref& reExp, TranslationContext& context)
{
    // *x, *G()
    DesignatedDiagnostic<Error_ResolveIdentifier_ExpressionIsNotLocation> designatedDiag;

    auto eTarget = TranslateReExpToNLoc(*reExp.target, /*bWrapExpAsLoc*/ true, &designatedDiag, context);
    if (!eTarget) return unexpected{move(eTarget).error()};

    return MakePtr<NLoc_BoxDeref>(move(*eTarget));
}

namespace {

class ReExpToNLocTranslator : public ReExpVisitor
{
    expected<NLoc*, DiagPtr>* result;
    bool bWrapExpAsLoc;
    IDesignatedDiagnostic* notLocationDiag;
    

    TranslationContext& context;

public:
    ReExpToNLocTranslator(expected<NLoc*, DiagPtr>* result, bool bWrapExpAsLoc, IDesignatedDiagnostic* notLocationDiag, TranslationContext& context)
        : result(result), bWrapExpAsLoc(bWrapExpAsLoc), notLocationDiag(notLocationDiag), context(context)
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
            *result = unexpected{notLocationDiag->MakeDiag()};
        }
    }
};

}

expected<NLoc*, DiagPtr> TranslateReExpToNLoc(ReExp& reExp, bool bWrapExpAsLoc, IDesignatedDiagnostic* notLocationDiag, TranslationContext& context)
{
    expected<NLoc*, DiagPtr> nLoc;
    ReExpToNLocTranslator translator{&nLoc, bWrapExpAsLoc, notLocationDiag, context};
    reExp.Accept(translator);
    return nLoc;
}

}
