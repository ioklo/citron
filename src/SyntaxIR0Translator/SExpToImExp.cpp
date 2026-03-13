#include "SExpToImExp.h"
#include "Infra/Expected.h"
#include "Syntax/Syntax.h"
#include "MIR/MExp.h"
#include "MIR/MLoc.h"
#include "MIR/MInitExp.h"
#include "ImExp.h"
#include "ReExp.h"
#include "SRTFactory.h"
#include "TranslationContexts.h"
#include "SExp_IdentifierToImExp.h"
#include "SExp_CallToImExp.h"
#include "SExp_MemberToImExp.h"
#include "SExpTranslations.h"

using namespace std;

namespace Citron {

struct SExpToImExpTranslator
{
    using ResultType = expected<ImExp*, DiagPtr>;
    RType* hintType;
    TranslationContexts& contexts;

    ImExp* MakeImExp_ReExp(ReExp&& reExp)
    {
        return contexts.srtFactory->MakeImExp<ImExp_ReExp>(move(reExp));
    }

    ImExp* MakeImExp_ReExp_InitExp(MInitExp* mInitExp)
    {
        return contexts.srtFactory->MakeImExp<ImExp_ReExp>(ReExp_InitExp{mInitExp});
    }

    ImExp* MakeImExp_ReExp_Exp(MExp* mExp)
    {
        return contexts.srtFactory->MakeImExp<ImExp_ReExp>(ReExp_Exp{mExp});
    }

    ImExp* MakeImExp_ReExp_Loc(MLoc* mLoc)
    {
        return contexts.srtFactory->MakeImExp<ImExp_ReExp>(ReExp_Loc{mLoc});
    }

    ResultType Visit(SExp_Identifier* sExp) 
    {
        return TranslateSExp_IdentifierToImExp(sExp, contexts);
    }

    ResultType Visit(SExp_String* sExp) 
    {
        auto e_mInitExp = TranslateSExp_StringToMInitExp_String(sExp, contexts);
        RETURN_ON_ERROR(e_mInitExp);

        return MakeImExp_ReExp_InitExp(*e_mInitExp);
    }

    ResultType Visit(SExp_IntLiteral* sExp) 
    {
        auto e_mExp = TranslateSExp_IntLiteralToMExp_IntLiteral(sExp, contexts);
        RETURN_ON_ERROR(e_mExp);

        return MakeImExp_ReExp_Exp(*e_mExp);
    }

    ResultType Visit(SExp_BoolLiteral* sExp)
    {
        auto e_mExp = TranslateSExp_BoolLiteralToMExp_BoolLiteral(sExp, contexts);
        RETURN_ON_ERROR(e_mExp);

        return MakeImExp_ReExp_Exp(*e_mExp);
    }
    
    ResultType Visit(SExp_NullLiteral* sExp) 
    {   
        auto e_reExp = TranslateSExp_NullLiteralToReExp(sExp, hintType, contexts);
        RETURN_ON_ERROR(e_reExp);

        return MakeImExp_ReExp(move(*e_reExp));
    }

    ResultType Visit(SExp_BinaryOp* sExp)
    {
        auto e_reExp = TranslateSExp_BinaryOpToReExp(sExp, contexts);
        RETURN_ON_ERROR(e_reExp);

        return MakeImExp_ReExp(move(*e_reExp));
    }

    ResultType Visit(SExp_UnaryOp* sExp) 
    {
        auto e_reExp = TranslateSExp_UnaryOpToReExp(sExp, hintType, contexts);
        RETURN_ON_ERROR(e_reExp);

        return MakeImExp_ReExp(move(*e_reExp));
    }

    ResultType Visit(SExp_Call* sExp) 
    {
        return TranslateSExp_CallToImExp(sExp, contexts);
    }

    ResultType Visit(SExp_Lambda* sExp) 
    {
        auto e_reExp = TranslateSExp_LambdaToReExp(sExp, contexts);
        RETURN_ON_ERROR(e_reExp);

        return MakeImExp_ReExp(move(*e_reExp));
    }

    ResultType Visit(SExp_Indexer* sExp) 
    {
        auto e_mLoc = TranslateSExp_IndexerToMLoc_ListIndexer(sExp, contexts);
        RETURN_ON_ERROR(e_mLoc);

        return MakeImExp_ReExp_Loc(*e_mLoc);
    }

    ResultType Visit(SExp_Member* sExp) 
    {
        return TranslateSExp_MemberToImExp(sExp, contexts);
    }

    ResultType Visit(SExp_List* sExp) 
    {
        auto e_mInitExp = TranslateSExp_ListToMInitExp(sExp, hintType, contexts);
        RETURN_ON_ERROR(e_mInitExp);

        return MakeImExp_ReExp_InitExp(*e_mInitExp);
    }

    ResultType Visit(SExp_New* sExp) 
    {
        auto e_mInitExp = TranslateSExp_NewToMInitExp_NewClass(sExp, contexts);
        RETURN_ON_ERROR(e_mInitExp);

        return MakeImExp_ReExp_InitExp(*e_mInitExp);
    }
    
    ResultType Visit(SExp_Shared* sExp) 
    {
        auto e_mInitExp = TranslateSExp_SharedToMInitExp_Shared(sExp, hintType, contexts);
        RETURN_ON_ERROR(e_mInitExp);

        return MakeImExp_ReExp_InitExp(*e_mInitExp);
    }

    ResultType Visit(SExp_Is* sExp) 
    {
        auto e_mExp = TranslateSExp_IsToMExp_Is(sExp, contexts);
        RETURN_ON_ERROR(e_mExp);

        return MakeImExp_ReExp_Exp(*e_mExp);
    }

    ResultType Visit(SExp_As* sExp) 
    {
        auto e_mInitExp = TranslateSExp_AsToMInitExp_As(sExp, contexts);
        RETURN_ON_ERROR(e_mInitExp);

        return MakeImExp_ReExp_InitExp(*e_mInitExp);
    }
};

expected<ImExp*, DiagPtr> TranslateSExpToImExp(SExp* sExp, RType* hintType, TranslationContexts& contexts)
{
    return Accept(SExpToImExpTranslator{hintType, contexts}, sExp);
}

} // namespace Citron