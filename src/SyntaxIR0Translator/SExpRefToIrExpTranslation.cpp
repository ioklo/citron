#include "pch.h"
#include "SExpRefToIrExpTranslation.h"

#include <Infra/Ptr.h>
#include <Syntax/Syntax.h>
#include <Logging/Logger.h>

#include "IrExp.h"

#include "SExpToNExpTranslation.h"
#include "SExpToNLocTranslation.h"
#include "SExpRefToNExpTranslation.h"
#include "IrExpAndMemberNameToIrExpTranslation.h"

#include "TranslationContext.h"

#include "DesignatedErrorLogger.h"
#include "Misc.h"

namespace Citron::SyntaxIR0Translator {

namespace {

// & exp syntax를 중간과정으로 번역해주는 역할
// SExp -> IrExp
struct SExpRefToIrExpTranslator : public SExpVisitor
{
    IrExpPtr* result;
    TranslationContext& context;

public:
    SExpRefToIrExpTranslator(IrExpPtr* result, TranslationContext& context)
        : result(result), context(context)
    {
    }

    void HandleValue(SExp& exp)
    {
        auto nExp = TranslateSExpToNExp(exp, /*hintType*/ nullptr, context);
        if (!nExp)
        {
            *result = nullptr;
            return;
        }

        *result = MakePtr<IrExp_LocalValue>(std::move(nExp));
    }

    void Visit(SExp_Identifier& exp) override
    {
        static_assert(false);
        /*try
        {
            var typeArgs = BodyMisc.MakeTypeArgs(exp.TypeArgs, context);
            var imExp = context.ResolveIdentifier(new Name.Normal(exp.Value), typeArgs);
            if (imExp == null)
            {
                return Fatal(A2007_ResolveIdentifier_NotFound, exp);
            }

            var imRefExp = TranslateImExpToIrExp(imExp, factory);
            if (imRefExp == null)
            {
                return Fatal(A3001_Reference_CantMakeReference, exp);
            }

            return Valid(imRefExp);
        }
        catch (IdentifierResolverMultipleCandidatesException)
        {
            return Fatal(A2001_ResolveIdentifier_MultipleCandidatesForIdentifier, exp);
        }*/
    }

    // string은 중간과정에서는 value로 평가하면 될 것 같다
    void Visit(SExp_String& exp) override
    {
        HandleValue(exp);
    }

    void Visit(SExp_IntLiteral& exp) override
    {
        HandleValue(exp);
    }

    void Visit(SExp_BoolLiteral& exp) override
    {
        HandleValue(exp);
    }

    void Visit(SExp_NullLiteral& exp) override
    {
        HandleValue(exp);
    }

    void Visit(SExp_BinaryOp& exp) override
    {
        // assign 제외
        HandleValue(exp);
    }

    void Visit(SExp_UnaryOp& exp) override
    {
        if (exp.kind == SUnaryOpKind::Ref) // & &는 불가능
        {
            auto nExp = TranslateSExpRefToNExp(*exp.operand, context);
            if (!nExp)
            {
                *result = nullptr;
                return;
            }

            *result = MakePtr<IrExp_LocalValue>(std::move(nExp));
            return;
        }
        else if (exp.kind == SUnaryOpKind::Deref) // *pS
        {
            auto designatedErrorLogger = context.MakeDesignatedErrorLogger(&Logger::Fatal_ResolveIdentifier_ExpressionIsNotLocation);

            auto nOperandLoc = TranslateSExpToNLoc(exp, /*hintType*/ nullptr, /*bWrapExpAsLoc*/ true, &designatedErrorLogger, context);
            if (!nOperandLoc)
            {
                *result = nullptr;
                return;
            }

            *result = MakePtr<IrExp_DerefedBoxValue>(std::move(nOperandLoc));
            return;
        }
        else
        {
            return HandleValue(exp);
        }
    }

    void Visit(SExp_Call& exp) override
    {
        HandleValue(exp);
    }

    void Visit(SExp_Lambda& exp) override
    {
        HandleValue(exp);
    }

    // e[e] 꼴
    void Visit(SExp_Indexer& exp) override
    {
        // location으로 쓰지 않고 value로 쓴다
        HandleValue(exp);
    }

    void Visit(SExp_Member& exp) override
    {
        auto parent = TranslateSExpRefToIrExp(*exp.parent, context);
        if (!parent)
        {
            *result = nullptr;
            return;
        }

        auto typeArgsExceptOuter = MakeTypeArgs(exp.memberTypeArgs, context);

        context.SetSyntax(exp.parent);
        *result = TranslateIrExpAndMemberNameToIrExp(parent, RName_Normal(exp.memberName), std::move(typeArgsExceptOuter), context);
    }

    void Visit(SExp_IndirectMember& exp) override
    {
        static_assert(false);
    }

    void Visit(SExp_List& exp) override
    {
        HandleValue(exp);
    }

    void Visit(SExp_New& exp) override
    {
        HandleValue(exp);
    }

    void Visit(SExp_Box& exp) override
    {
        HandleValue(exp);
    }

    void Visit(SExp_Is& exp) override
    {
        HandleValue(exp);
    }

    void Visit(SExp_As& exp) override
    {
        HandleValue(exp);
    }
};

} // namespace 

IrExpPtr TranslateSExpRefToIrExp(SExp& exp, TranslationContext& context)
{
    IrExpPtr irExp;
    SExpRefToIrExpTranslator translator(&irExp, context);
    exp.Accept(translator);

    return irExp;
}

} // namespace Citron::SyntaxIR0Translator