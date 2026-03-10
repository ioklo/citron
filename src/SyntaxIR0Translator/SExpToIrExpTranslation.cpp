#include "SExpToIrExpTranslation.h"

#include "Infra/Expected.h"
#include "Syntax/Syntax.h"
#include "RSymbol/RTypes.h"
#include "RSymbol/RTypeArguments.h"
#include "MIR/MLoc.h"

#include "IrExp.h"
#include "SRTFactory.h"
#include "Misc.h"
#include "TranslationContexts.h"
#include "SExpToMLocTranslation.h"
#include "ImExpToIrExpTranslation.h"
#include "DesignatedDiagnostic.h"

using namespace std;

namespace Citron {

// SExp_Member의 base부분에 대한 translator
struct SExpToIrExpTranslator
{
    using ResultType = expected<IrExp*, DiagPtr>;
    TranslationContexts& contexts;

    // 기본
    ResultType Visit(SExp* exp)
    {
        return Error<Error_SharedTranslation_CantMakeSharedFromBase>();
    }

    ResultType Visit(SExp_Identifier* exp)
    {
        // identifier는 name<typeArgs>로 이뤄져 있다
        auto e_memberTypeArgs = MakeRTypeArgs(exp->typeArgs, contexts);
        RETURN_ON_ERROR(e_memberTypeArgs);

        auto* memberTypeArgs = *e_memberTypeArgs;
        auto e_bodyRes = ResolveIdentifier(RName_Normal{exp->value}, memberTypeArgs->GetCount(), contexts);
        RETURN_ON_ERROR(e_bodyRes);

        return TranslateBodyResAndMemberTypeArgsToIrExp(*e_bodyRes, memberTypeArgs, contexts);
    }
    
    // ResultType Visit(SExp_String* exp); // &"abc".id
    // ResultType Visit(SExp_IntLiteral* exp); // &1.id
    // ResultType Visit(SExp_BoolLiteral* exp); // &true.id
    // ResultType Visit(SExp_NullLiteral* exp); // &null.id
    // ResultType Visit(SExp_BinaryOp* exp); // &(e0 + e1).id, &(e0 = e1).id

    // &(*pS).id
    ResultType Visit(SExp_UnaryOp* exp) 
    {
        if (exp->kind == SUnaryOpKind::Deref)
        {
            // 두가지 경우가 accept될 수 있는데
            // 1. &(*pS).id : pS가 shared<S>일때 
            // 2. &(*ptrC).id : ptrC가 C*일때 

            DesignatedDiagnostic<Error_ResolveIdentifier_ExpressionIsNotLocation> designatedDiag;

            auto e_loc = TranslateSExpToMLoc(exp->operand, /*hintType*/nullptr, /*bMaterializeExp*/true, &designatedDiag, contexts);
            RETURN_ON_ERROR(e_loc);

            auto* locSharedType = dynamic_cast<RType_Shared*>((*e_loc)->GetType());
            if (!locSharedType)
                return Error<Error_SharedTranslation_MemberBaseShouldBeShared>();

            return contexts.srtFactory->MakeIrExp<IrExp_SharedDeref>(*e_loc);
        }
        else
        {
            return Error<Error_SharedTranslation_CantMakeSharedFromBase>();
        }
    }

    ResultType Visit(SExp_Call* exp);
    ResultType Visit(SExp_Lambda* exp);
    ResultType Visit(SExp_Indexer* exp);
    ResultType Visit(SExp_Member* exp);
    ResultType Visit(SExp_IndirectMember* exp);
    ResultType Visit(SExp_List* exp);
    ResultType Visit(SExp_New* exp);
    ResultType Visit(SExp_Shared* exp);
    ResultType Visit(SExp_Box* exp);
    ResultType Visit(SExp_Is* exp);
    ResultType Visit(SExp_As* exp);
};

expected<IrExp*, DiagPtr> TranslateSExpToIrExp(SExp* sExp, TranslationContexts& contexts)
{   
    return Accept(SExpToIrExpTranslator{contexts}, sExp);
}

} // namespace Citron