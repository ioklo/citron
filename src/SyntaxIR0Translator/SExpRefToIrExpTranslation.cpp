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

#include "TranslationContext.h"
#include "Misc.h"

using namespace std;

namespace Citron {

namespace {

// & exp syntax를 중간과정으로 번역해주는 역할
// SExp -> IrExp
struct SExpRefToIrExpTranslator
{
    using ResultType = expected<IrExp*, DiagPtr>;
    TranslationContext& context;

public:
    SExpRefToIrExpTranslator(TranslationContext& context)
        : context{context}
    {
    }

private:
    template<typename TValue, typename... TArgs> requires std::derived_from<TValue, IrExp>
    ResultType Value(TArgs&&... args)
    {
        return context.MakeIrExp<TValue>(forward<TArgs>(args)...);
    }

    template<typename TDiag, typename... TArgs> requires std::derived_from<TDiag, Diag>
    ResultType Error(TArgs&&... args)
    {
        return unexpected{MakePtr<TDiag>(forward<TArgs>(args)...)};
    }

    ResultType HandleValue(SExp* exp)
    {
        auto eExp = TranslateSExpToMExp(exp, /*hintType*/ nullptr, context);
        RETURN_ON_ERROR(eExp);
        
        return context.MakeIrExp<IrExp_LocalValue>(*eExp);
    }

public:
    // identifier에 &가 붙으면 어떻게 처리할 것인가
    ResultType Visit(SExp_Identifier* exp)
    {   
        // identifier는 name<typeArgs>로 이뤄져 있다
        auto eRTypeArgs = MakeRTypeArgs(exp->typeArgs, context);
        RETURN_ON_ERROR(eRTypeArgs);

        auto eImExp = context.ResolveIdentifier(RName_Normal{exp->value}, *eRTypeArgs);
        RETURN_ON_ERROR(eImExp);

        auto eIrExp = TranslateImExpToIrExp(*eImExp, context);
        RETURN_ON_ERROR(eIrExp);

        return *eIrExp;
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
            auto eExp = TranslateSExpRefToMExp(exp->operand, context);
            RETURN_ON_ERROR(eExp);

            return Value<IrExp_LocalValue>(*eExp);
        }
        else if (exp->kind == SUnaryOpKind::Deref) // *pS
        {
            DesignatedDiagnostic<Error_ResolveIdentifier_ExpressionIsNotLocation> designatedDiag;

            auto eLoc = TranslateSExpToMLoc(exp, /*hintType*/ nullptr, /*bWrapExpAsLoc*/ true, &designatedDiag, context);
            RETURN_ON_ERROR(eLoc);

            return Value<IrExp_DerefedBoxValue>(*eLoc);
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
        auto eIrParent = TranslateSExpRefToIrExp(exp->parent, context);
        RETURN_ON_ERROR(eIrParent);

        auto eRTypeArgsExceptOuter = MakeRTypeArgs(exp->memberTypeArgs, context);
        
        return TranslateIrExpAndMemberNameToIrExp(*eIrParent, RName_Normal(exp->memberName), *eRTypeArgsExceptOuter, context);
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

expected<IrExp*, DiagPtr> TranslateSExpRefToIrExp(SExp* exp, TranslationContext& context)
{
    SExpRefToIrExpTranslator translator{context};
    return Accept(translator, exp);
}

} // namespace Citron