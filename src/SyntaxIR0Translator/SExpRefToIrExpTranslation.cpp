#include "SExpRefToIrExpTranslation.h"

#include <expected>

#include "Infra/Ptr.h"
#include "Infra/Exceptions.h"
#include "Syntax/Syntax.h"
#include "Logging/Logger.h"
#include "Logging/Diag.h"

#include "IrExp.h"

#include "SExpToNExpTranslation.h"
#include "SExpToNLocTranslation.h"
#include "SExpRefToNExpTranslation.h"
#include "IrExpAndMemberNameToIrExpTranslation.h"
#include "ImExpToIrExpTranslation.h"

#include "TranslationContext.h"
#include "Misc.h"

using namespace std;

namespace Citron::SyntaxIR0Translator {

namespace {

// & exp syntax를 중간과정으로 번역해주는 역할
// SExp -> IrExp
struct SExpRefToIrExpTranslator : public SExpVisitor
{
    expected<IrExp*, DiagPtr>* result;
    TranslationContext& context;

public:
    SExpRefToIrExpTranslator(std::expected<IrExp*, DiagPtr>* result, TranslationContext& context)
        : result(result), context(context)
    {
    }

private:
    void Forward(expected<IrExp*, DiagPtr>&& r)
    {
        *result = move(r);
    }

    void Value(IrExp* v)
    {
        *result = move(v);
    }

    template<typename TValue, typename... TArgs> requires std::is_base_of_v<IrExp, TValue>
    void Value(TArgs&&... args)
    {
        *result = MakePtr<TValue>(forward<TArgs>(args)...);
    }

    template<typename TValue>
    void Error(expected<TValue, DiagPtr>&& e)
    {
        *result = unexpected{move(e).error()};
    }

    template<typename TDiag, typename... TArgs> requires std::is_base_of_v<Diag, TDiag>
    void Error(TArgs&&... args)
    {
        *result = unexpected{MakePtr<TDiag>(forward<TArgs>(args)...)};
    }

    void HandleValue(SExp& exp)
    {
        auto eExp = TranslateSExpToNExp(exp, /*hintType*/ nullptr, context);
        if (!eExp)
        {
            *result = unexpected{move(eExp).error()};
            return;
        }

        *result = MakePtr<IrExp_LocalValue>(move(*eExp));
    }

public:
    // identifier에 &가 붙으면 어떻게 처리할 것인가
    void Visit(SExp_Identifier& exp) override
    {   
        // identifier는 name<typeArgs>로 이뤄져 있다
        auto eTypeArgs = MakeTypeArgs(exp.typeArgs, context);
        if (!eTypeArgs) return Error(move(eTypeArgs));

        auto eImExp = context.ResolveIdentifier(RName_Normal{exp.value}, move(*eTypeArgs));

        if (!eImExp)
        {
            auto* error = eImExp.error().get();

            if (auto* mcError = dynamic_cast<ResolveIdentifierError_MultipleCandidates*>(error))
            {
                return Error<Error_ResolveIdentifier_MultipleCandidatesForIdentifier>();
            }
            else
            {
                return Error<Error_NotImplemented>();
            }
        }

        auto eIrExp = TranslateImExpToIrExp(*eImExp, context);
        if (!eIrExp)
            return Error(move(eIrExp));

        return Value(move(*eIrExp));
    }

    // string은 중간과정에서는 value로 평가하면 될 것 같다
    void Visit(SExp_String& exp) override
    {
        return HandleValue(exp);
    }

    void Visit(SExp_IntLiteral& exp) override
    {
        return HandleValue(exp);
    }

    void Visit(SExp_BoolLiteral& exp) override
    {
        return HandleValue(exp);
    }

    void Visit(SExp_NullLiteral& exp) override
    {
        return HandleValue(exp);
    }

    void Visit(SExp_BinaryOp& exp) override
    {
        // assign 제외
        return HandleValue(exp);
    }

    void Visit(SExp_UnaryOp& exp) override
    {
        if (exp.kind == SUnaryOpKind::Ref) // & &는 불가능
        {
            auto eExp = TranslateSExpRefToNExp(*exp.operand, context);
            if (!eExp) return Error(move(eExp));

            return Value<IrExp_LocalValue>(move(*eExp));
        }
        else if (exp.kind == SUnaryOpKind::Deref) // *pS
        {
            DesignatedDiagnostic<Error_ResolveIdentifier_ExpressionIsNotLocation> designatedDiag;

            auto eLoc = TranslateSExpToNLoc(exp, /*hintType*/ nullptr, /*bWrapExpAsLoc*/ true, &designatedDiag, context);
            if (!eLoc) return Error(move(eLoc));

            return Value<IrExp_DerefedBoxValue>(move(*eLoc));
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
        auto eIrParent = TranslateSExpRefToIrExp(*exp.parent, context);
        if (!eIrParent) return Error(move(eIrParent));

        auto eTypeArgsExceptOuter = MakeTypeArgs(exp.memberTypeArgs, context);
        
        return Forward(TranslateIrExpAndMemberNameToIrExp(*eIrParent, RName_Normal(exp.memberName), move(*eTypeArgsExceptOuter), context));
    }

    void Visit(SExp_IndirectMember& exp) override
    {
        throw NotImplementedException();
    }

    void Visit(SExp_List& exp) override
    {
        return HandleValue(exp);
    }

    void Visit(SExp_New& exp) override
    {
        return HandleValue(exp);
    }

    void Visit(SExp_Box& exp) override
    {
        return HandleValue(exp);
    }

    void Visit(SExp_Is& exp) override
    {
        return HandleValue(exp);
    }

    void Visit(SExp_As& exp) override
    {
        return HandleValue(exp);
    }
};

} // namespace 

expected<IrExp*, DiagPtr> TranslateSExpRefToIrExp(SExp& exp, TranslationContext& context)
{
    expected<IrExp*, DiagPtr> irExp;
    SExpRefToIrExpTranslator translator(&irExp, context);
    exp.Accept(translator);

    return irExp;
}

} // namespace Citron::SyntaxIR0Translator