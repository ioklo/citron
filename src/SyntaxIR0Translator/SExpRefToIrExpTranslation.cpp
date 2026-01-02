#include "SExpRefToIrExpTranslation.h"

#include <expected>

#include "Infra/Ptr.h"
#include "Infra/Exceptions.h"
#include "Infra/Expected.h"
#include "Syntax/Syntax.h"
#include "Logging/Logger.h"
#include "Logging/Diag.h"

#include "IrExp.h"

#include "SExpToMExpTranslation.h"
#include "SExpToMLocTranslation.h"
#include "SExpRefToMExpTranslation.h"
#include "IrExpAndMemberNameToIrExpTranslation.h"
#include "ImExpToIrExpTranslation.h"
#include "TranslationContexts.h"
#include "Misc.h"
#include "SRTFactory.h"
#include "DesignatedDiagnostic.h"

using namespace std;

namespace Citron {

namespace {

// & exp syntax를 중간과정으로 번역해주는 역할
// SExp -> IrExp
struct SExpRefToIrExpTranslator
{
    using ResultType = expected<IrExp*, DiagPtr>;
    TranslationContexts& contexts;

public:
    SExpRefToIrExpTranslator(TranslationContexts& contexts)
        : contexts{contexts}
    {
    }

private:
    template<typename TValue, typename... TArgs> requires std::derived_from<TValue, IrExp>
    ResultType Value(TArgs&&... args)
    {
        return contexts.srtFactory->MakeIrExp<TValue>(forward<TArgs>(args)...);
    }

    template<typename TDiag, typename... TArgs> requires std::derived_from<TDiag, Diag>
    ResultType Error(TArgs&&... args)
    {
        return unexpected{MakePtr<TDiag>(forward<TArgs>(args)...)};
    }

    ResultType HandleValue(SExp* exp)
    {
        auto e_exp = TranslateSExpToMExp(exp, /*hintType*/ nullptr, contexts);
        RETURN_ON_ERROR(e_exp);
        
        return contexts.srtFactory->MakeIrExp<IrExp_LocalValue>(*e_exp);
    }

public:
    // identifier에 &가 붙으면 어떻게 처리할 것인가
    ResultType Visit(SExp_Identifier* exp)
    {   
        // identifier는 name<typeArgs>로 이뤄져 있다
        auto e_rTypeArgs = MakeRTypeArgs(exp->typeArgs, contexts);
        RETURN_ON_ERROR(e_rTypeArgs);

        auto e_imExp = ResolveIdentifier(RName_Normal{exp->value}, *e_rTypeArgs, contexts);
        RETURN_ON_ERROR(e_imExp);

        auto e_irExp = TranslateImExpToIrExp(*e_imExp, contexts);
        RETURN_ON_ERROR(e_irExp);

        return *e_irExp;
    }

    // string은 중간과정에서는 value로 평가하면 될 것 같다
    ResultType Visit(SExp_String* exp)
    {
        return HandleValue(exp);
    }

    ResultType Visit(SExp_IntLiteral* exp)
    {
        return HandleValue(exp);
    }

    ResultType Visit(SExp_BoolLiteral* exp)
    {
        return HandleValue(exp);
    }

    ResultType Visit(SExp_NullLiteral* exp)
    {
        return HandleValue(exp);
    }

    ResultType Visit(SExp_BinaryOp* exp)
    {
        // assign 제외
        return HandleValue(exp);
    }

    ResultType Visit(SExp_UnaryOp* exp)
    {
        if (exp->kind == SUnaryOpKind::Ref) // & &는 불가능
        {
            auto e_exp = TranslateSExpRefToMExp(exp->operand, contexts);
            RETURN_ON_ERROR(e_exp);

            return Value<IrExp_LocalValue>(*e_exp);
        }
        else if (exp->kind == SUnaryOpKind::Deref) // *pS
        {
            DesignatedDiagnostic<Error_ResolveIdentifier_ExpressionIsNotLocation> designatedDiag;

            auto e_loc = TranslateSExpToMLoc(exp, /*hintType*/ nullptr, /*bWrapExpAsLoc*/ true, &designatedDiag, contexts);
            RETURN_ON_ERROR(e_loc);

            return Value<IrExp_DerefedBoxValue>(*e_loc);
        }
        else
        {
            return HandleValue(exp);
        }
    }

    ResultType Visit(SExp_Call* exp)
    {
        return HandleValue(exp);
    }

    ResultType Visit(SExp_Lambda* exp)
    {
        return HandleValue(exp);
    }

    // e[e] 꼴
    ResultType Visit(SExp_Indexer* exp)
    {
        // location으로 쓰지 않고 value로 쓴다
        return HandleValue(exp);
    }

    ResultType Visit(SExp_Member* exp)
    {
        auto e_irParent = TranslateSExpRefToIrExp(exp->parent, contexts);
        RETURN_ON_ERROR(e_irParent);

        auto e_rTypeArgsExceptOuter = MakeRTypeArgs(exp->memberTypeArgs, contexts);
        
        return TranslateIrExpAndMemberNameToIrExp(*e_irParent, RName_Normal(exp->memberName), *e_rTypeArgsExceptOuter, contexts);
    }

    ResultType Visit(SExp_IndirectMember* exp)
    {
        throw NotImplementedException{};
    }

    ResultType Visit(SExp_List* exp)
    {
        return HandleValue(exp);
    }

    ResultType Visit(SExp_New* exp)
    {
        return HandleValue(exp);
    }

    ResultType Visit(SExp_Box* exp)
    {
        return HandleValue(exp);
    }

    ResultType Visit(SExp_Is* exp)
    {
        return HandleValue(exp);
    }

    ResultType Visit(SExp_As* exp)
    {
        return HandleValue(exp);
    }
};

} // namespace 

expected<IrExp*, DiagPtr> TranslateSExpRefToIrExp(SExp* exp, TranslationContexts& contexts)
{
    SExpRefToIrExpTranslator translator{contexts};
    return Accept(translator, exp);
}

} // namespace Citron