//#include "SExpToMLocTranslation.h"
//
//#include <expected>
//
//#include "Infra/Ptr.h"
//#include "Infra/Exceptions.h"
//#include "Infra/Expected.h"
//#include "Logging/Logger.h"
//#include "Syntax/Syntax.h"
//#include "MIR/MLoc.h"
//#include "MIR/MExp.h"
//#include "MIR/MFactory.h"
//
//#include "DesignatedDiagnostic.h"
//#include "ReExpToMLocTranslation.h"
//#include "SExpToReExpTranslation.h"
//#include "SExpToMExpTranslation.h"
//
//#include "TranslationContexts.h"
//
//using namespace std;
//
//namespace Citron {
//
//namespace {
//
//class SExpToMLocTranslator
//{
//public:
//    using ResultType = expected<MLoc*, DiagPtr>;
//
//private:
//    RType* hintType;
//    bool bMaterializeExp;
//
//    IDesignatedDiagnostic* notLocationDiag;
//    TranslationContexts& contexts;
//
//public:
//    SExpToMLocTranslator(
//        RType* hintType,
//        bool bMaterializeExp,
//        IDesignatedDiagnostic* notLocationDiag,
//        TranslationContexts& contexts)
//        : hintType{hintType}, bMaterializeExp{bMaterializeExp}, notLocationDiag{notLocationDiag}, contexts{contexts}
//    {
//    }
//
//private:
//    ResultType HandleDefault(SExp* sExp)
//    {
//        auto e_reExp = TranslateSExpToReExp(sExp, hintType, contexts);
//        RETURN_ON_ERROR(e_reExp);
//        
//        DesignatedDiagnostic<Error_ResolveIdentifier_ExpressionIsNotLocation> designatedDiag;
//        return TranslateReExpToMLoc(*e_reExp, bMaterializeExp, &designatedDiag, contexts);
//    }
//
//    // fast track
//    ResultType HandleExp(expected<MExp*, DiagPtr>&& e_nExp)
//    {
//        if (!e_nExp)
//        {
//            return unexpected{move(e_nExp).error()};
//        }
//        else if (bMaterializeExp)
//        {
//            return contexts.mFactory->MakeMLoc<MLoc_Temp>(*e_nExp);
//        }
//        else
//        {
//            return unexpected{notLocationDiag->MakeDiag()};
//        }
//    }
//    
//public:
//    ResultType Visit(SExp_Identifier* exp)
//    {
//        return HandleDefault(exp);
//    }
//
//    ResultType Visit(SExp_String* exp)
//    {
//        auto e_nExp = TranslateSStringExpToMStringExp(exp, contexts);
//        RETURN_ON_ERROR(e_nExp);
//
//        return HandleExp(move(*e_nExp));
//    }
//
//    ResultType Visit(SExp_IntLiteral* exp)
//    {
//        auto e_nExp = TranslateSIntLiteralExpToMExp(exp, contexts);
//        RETURN_ON_ERROR(e_nExp);
//
//        return HandleExp(move(*e_nExp));
//    }
//
//    ResultType Visit(SExp_BoolLiteral* exp)
//    {
//        auto e_nExp = TranslateSBoolLiteralExpToMExp(exp, contexts);
//        RETURN_ON_ERROR(e_nExp);
//
//        return HandleExp(move(*e_nExp));
//    }
//
//    ResultType Visit(SExp_NullLiteral* exp)
//    {
//        auto e_nExp = TranslateSNullLiteralExpToMExp(exp, hintType, contexts);
//        RETURN_ON_ERROR(e_nExp);
//
//        return HandleExp(move(*e_nExp));
//    }
//
//    ResultType Visit(SExp_BinaryOp* exp)
//    {
//        auto e_nExp = TranslateSBinaryOpExpToMExp(exp, contexts);
//        RETURN_ON_ERROR(e_nExp);
//
//        return HandleExp(move(*e_nExp));
//    }
//
//    ResultType Visit(SExp_UnaryOp* exp)
//    {
//        // Deref는 loc으로 변경되어야 한다
//        if (exp->kind == SUnaryOpKind::Deref)
//        {
//            return HandleDefault(exp);
//        }
//        else
//        {
//            auto e_nExp = TranslateSUnaryOpExpToMExpExceptDeref(exp, contexts);
//            RETURN_ON_ERROR(e_nExp);
//
//            return HandleExp(move(*e_nExp));
//        }
//    }
//
//    ResultType Visit(SExp_Call* exp)
//    {
//        auto e_nExp = TranslateSCallExpToMExp(exp, hintType, contexts);
//        RETURN_ON_ERROR(e_nExp);
//
//        return HandleExp(move(*e_nExp));
//    }
//
//    ResultType Visit(SExp_Lambda* exp)
//    {
//        auto e_nExp = TranslateSLambdaExpToMExp(exp, contexts);
//        RETURN_ON_ERROR(e_nExp);
//
//        return HandleExp(move(*e_nExp));
//    }
//
//    ResultType Visit(SExp_Indexer* exp)
//    {
//        return HandleDefault(exp);
//    }
//
//    ResultType Visit(SExp_Member* exp)
//    {
//        return HandleDefault(exp);
//    }
//
//    // s->x
//    ResultType Visit(SExp_IndirectMember* exp) 
//    { 
//        throw NotImplementedException{};
//    }
//
//    ResultType Visit(SExp_List* exp)
//    {
//        auto e_nExp = TranslateSListExpToMExp(exp, contexts);
//        RETURN_ON_ERROR(e_nExp);
//
//        return HandleExp(move(*e_nExp));
//    }
//
//    ResultType Visit(SExp_New* exp)
//    {
//        auto e_nExp = TranslateSNewExpToMExp(exp, contexts);
//        RETURN_ON_ERROR(e_nExp);
//
//        return HandleExp(move(*e_nExp));
//    }
//
//    ResultType Visit(SExp_Box* exp)
//    {
//        auto e_nExp = TranslateSBoxExpToMExp(exp, hintType, contexts);
//        RETURN_ON_ERROR(e_nExp);
//
//        return HandleExp(move(*e_nExp));
//    }
//
//    ResultType Visit(SExp_Is* exp)
//    {
//        auto e_nExp = TranslateSIsExpToMExp(exp, contexts);
//        RETURN_ON_ERROR(e_nExp);
//
//        return HandleExp(move(*e_nExp));
//    }
//
//    ResultType Visit(SExp_As* exp)
//    {
//        auto e_nExp = TranslateSAsExpToMExp(exp, contexts);
//        RETURN_ON_ERROR(e_nExp);
//
//        return HandleExp(move(*e_nExp));
//    }
//};
//
//} // namespace 
//
//expected<MLoc*, DiagPtr> TranslateSExpToMLoc(SExp* sExp, RType* hintType, bool bMaterializeExp, IDesignatedDiagnostic* notLocationDiag, TranslationContexts& contexts)
//{
//    SExpToMLocTranslator translator{hintType, bMaterializeExp, notLocationDiag, contexts};
//    return Accept(translator, sExp);
//}
//
//} // namespace Citron
