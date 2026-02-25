#include "SExpRefToIrExpTranslation.h"

#include <expected>

#include "Infra/Ptr.h"
#include "Infra/Exceptions.h"
#include "Infra/Expected.h"
#include "Syntax/Syntax.h"
#include "Logging/Logger.h"
#include "Logging/Diag.h"
#include "RSymbol/RTypes.h"
#include "MIR/MLoc.h"

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

// & exp syntax중에서 SExp_Member의 parent 부분을 번역해주는 역할
// SExp_Member(SExp parent, name) -> IrExp(parent), name
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
        auto e_exp = TranslateSExpToMExp(exp, /*hintType*/nullptr, contexts);
        RETURN_ON_ERROR(e_exp);
        
        return contexts.srtFactory->MakeIrExp<IrExp_LocalValue>(*e_exp);
    }

public:

    // 기본 동작
    ResultType Visit(SExp* exp)
    {
        return HandleValue(exp);
    }

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

    ResultType Visit(SExp_UnaryOp* exp)
    {
        if (exp->kind == SUnaryOpKind::Deref) // *pS
        {
            DesignatedDiagnostic<Error_ResolveIdentifier_ExpressionIsNotLocation> designatedDiag;

            auto e_loc = TranslateSExpToMLoc(exp, /*hintType*/nullptr, /*bWrapExpAsLoc*/true, &designatedDiag, contexts);
            RETURN_ON_ERROR(e_loc);

            auto* sharedLocType = dynamic_cast<RType_Shared*>((*e_loc)->GetType());
            if (!sharedLocType)
                return Error<Error_SharedRefTranslation_MemberParentShouldBeShared>();

            return Value<IrExp_SharedDeref>(*e_loc); // shared를 deref한것은 따로 표시를 해준다
        }
        else
        {
            return HandleValue(exp);
        }
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
};

} // namespace 

expected<IrExp*, DiagPtr> TranslateSExpRefToIrExp(SExp* exp, TranslationContexts& contexts)
{
    SExpRefToIrExpTranslator translator{contexts};
    return Accept(translator, exp);
}

} // namespace Citron