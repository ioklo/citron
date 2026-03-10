#include "SExpToImExpTranslation.h"

#include <expected>

#include "Infra/Ptr.h"
#include "Infra/Exceptions.h"
#include "Infra/Expected.h"

#include "Syntax/Syntax.h"
#include "Logging/Logger.h"
#include "RSymbol/RTypes.h"
#include "RSymbol/RFactory.h"
#include "MIR/MLoc.h"
#include "MIR/MExp.h"
#include "MIR/MRead.h"
#include "MIR/MFactory.h"

#include "ImExp.h"
#include "ReExp.h"

#include "SExpToMExpTranslation.h"
#include "SExpToMLocTranslation.h"
#include "ReExpToMLocTranslation.h"
#include "ImExpAndMemberNameToImExpTranslation.h"

#include "SExpToMIRTranslation.h"

#include "ScopeContext.h"
#include "TranslationContexts.h"
#include "SRTFactory.h"

#include "Misc.h"
#include "DesignatedDiagnostic.h"

using namespace std;

namespace Citron {

namespace {

class SExpToImExpTranslator
{
public:
    using ResultType = expected<ImExp*, DiagPtr>;

private:
    RType* hintType;
    TranslationContexts& contexts;

public:
    SExpToImExpTranslator(RType* hintType, TranslationContexts& contexts)
        : hintType{hintType}, contexts{contexts}
    {
    }

private:
    ResultType HandleExp(expected<MExp*, DiagPtr>&& eExp)
    {
        if (!eExp)
            return unexpected{move(eExp).error()};
        else
            return contexts.srtFactory->MakeImExp<ImExp_Exp>(*eExp);
    }

    template<typename TValue, typename... TArgs> requires std::derived_from<TValue, ImExp>
    ResultType Value(TArgs&&... args)
    {
        return contexts.srtFactory->MakeImExp<TValue>(forward<TArgs>(args)...);
    }
    
public:
    // x
    ResultType Visit(SExp_Identifier* exp)
    {
        auto e_rTypeArgs = MakeRTypeArgs(exp->typeArgs, contexts);
        RETURN_ON_ERROR(e_rTypeArgs);

        auto e_imExp = ResolveIdentifier(RName_Normal(exp->value), *e_rTypeArgs, contexts);
        RETURN_ON_ERROR(e_imExp);

        return *e_imExp;
    }

    ResultType Visit(SExp_String* exp)
    {
        return HandleExp(TranslateSStringExpToMStringExp(exp, contexts));
    }

    ResultType Visit(SExp_IntLiteral* exp)
    {
        return HandleExp(TranslateSIntLiteralExpToMExp(exp, contexts));
    }

    ResultType Visit(SExp_BoolLiteral* exp)
    {
        return HandleExp(TranslateSBoolLiteralExpToMExp(exp, contexts));
    }

    // 'null'
    ResultType Visit(SExp_NullLiteral* exp)
    {
        return HandleExp(TranslateSNullLiteralExpToMExp(exp, hintType, contexts));
    }

    ResultType Visit(SExp_BinaryOp* exp)
    {
        return HandleExp(TranslateSBinaryOpExpToMExp(exp, contexts));
    }

    ResultType Visit(SExp_UnaryOp* exp)
    {
        // *d
        if (exp->kind == SUnaryOpKind::Deref)
        {
            DesignatedDiagnostic<Error_ResolveIdentifier_ExpressionIsNotLocation> notLocationDiag;
            auto e_target = TranslateSExpToMLoc(exp->operand, /*hintType*/nullptr, /*bMaterializeExp*/true, &notLocationDiag, contexts);
            RETURN_ON_ERROR(e_target);

            auto* targetType = GetType(*e_target, &*contexts.rFactory);

            if (dynamic_cast<RType_Shared*>(targetType))
            {
                auto* loc = contexts.mFactory->MakeMLoc<MLoc_SharedDeref>(*e_target);
                return Value<ImExp_Loc>(loc);
            }

            if (dynamic_cast<RType_Ptr*>(targetType))
            {
                auto* loc = contexts.mFactory->MakeMLoc<MLoc_PtrDeref>(*e_target);
                return Value<ImExp_Loc>(loc);
            }

            // 에러를 내야 할 것 같다
            throw NotImplementedException{};
        }
        else
        {
            return HandleExp(TranslateSUnaryOpExpToMExpExceptDeref(exp, contexts));
        }
    }

    ResultType Visit(SExp_Call* exp)
    {
        return HandleExp(TranslateSCallExpToMExp(exp, hintType, contexts));
    }

    ResultType Visit(SExp_Lambda* exp)
    {
        return HandleExp(TranslateSLambdaExpToMExp(exp, contexts));
    }

    ResultType Visit(SExp_Indexer* exp)
    {
        auto e_mObj = TranslateSExpToMRead(exp->obj, /*hintType*/nullptr, contexts);
        RETURN_ON_ERROR(e_mObj);

        // 리스트 타입의 경우,
        RType* itemType;
        if (contexts.rFactory->IsListType(GetType(*e_mObj, &*contexts.rFactory), &itemType))
        {
            auto e_mIndex = TranslateSExpToMRead(exp->index, /*hintType*/nullptr, contexts);
            RETURN_ON_ERROR(e_mIndex);

            auto* intType = contexts.rFactory->MakeIntType();
            if (GetType(*e_mIndex, &*contexts.rFactory) != intType)
                throw NotImplementedException{};

            auto* loc = contexts.mFactory->MakeMLoc<MLoc_ListIndexer>(move(get<MRead_NBC>(*e_mObj)), move(get<MRead_BC>(*e_mIndex)), itemType);
            return Value<ImExp_Loc>(loc);
        }

        // TODO: custom indexer를 만들수 있으면 좋은가
        // var memberResult = objResult.TypeSymbol.QueryMember(new M.Name(M.SpecialName.IndexerGet, null), 0);

       

        throw NotImplementedException{};

        //// objTypeValue에 indexTypeValue를 인자로 갖고 있는 indexer가 있는지
        //if (!contexts.TypeValueService.GetMemberFuncValue(objType, SpecialNames.IndexerGet, ImmutableArray<TypeValue>.Empty, out var funcValue))
        //{
        //    contexts.ErrorCollector.Add(exp, "객체에 indexer함수가 없습니다");
        //    return false;
        //}

        //if (IsFuncStatic(funcValue.FuncId))
        //{
        //    Debug.Fail("객체에 indexer가 있는데 Static입니다");
        //    return false;
        //}

        //var funcTypeValue = contexts.TypeValueService.GetTypeValue(funcValue);

        //if (!analyzer.CheckParamTypes(exp, funcTypeValue.Params, new[] { indexType }))
        //    return false;

        //var listType = analyzer.GetListTypeValue()

        //// List타입인가 확인
        //if (analyzer.IsAssignable(listType, objType))
        //{
        //    var objTypeId = contexts.GetTypeId(objType);
        //    var indexTypeId = contexts.GetTypeId(indexType);

        //    outExp = new ListIndexerExp(new ExpInfo(obj, objTypeId), new ExpInfo(index, indexTypeId));
        //    outTypeValue = funcTypeValue.Return;
        //    return true;
        //}
    }

    // parent."x"<>
    ResultType Visit(SExp_Member* exp)
    {
        auto e_imParent = TranslateSExpToImExp(exp->parent, hintType, contexts);
        RETURN_ON_ERROR(e_imParent);

        auto e_rTypeArgs = MakeRTypeArgs(exp->memberTypeArgs, contexts);
        RETURN_ON_ERROR(e_rTypeArgs);

        return TranslateImExpAndMemberNameToImExp(*e_imParent, exp->memberName, *e_rTypeArgs, contexts);
    }

    ResultType Visit(SExp_IndirectMember* exp)
    {
        throw NotImplementedException{};
    }

    ResultType Visit(SExp_List* exp)
    {
        return HandleExp(TranslateSListExpToMExp(exp, contexts));
    }

    // 'new C(...)'
    ResultType Visit(SExp_New* exp)
    {
        return HandleExp(TranslateSNewExpToMExp(exp, contexts));
    }

    ResultType Visit(SExp_Box* exp)
    {
        return HandleExp(TranslateSBoxExpToMExp(exp, hintType, contexts));
    }

    ResultType Visit(SExp_Is* exp)
    {
        return HandleExp(TranslateSIsExpToMExp(exp, contexts));
    }

    ResultType Visit(SExp_As* exp)
    {
        return HandleExp(TranslateSAsExpToMExp(exp, contexts));
    }
};

}

expected<ImExp*, DiagPtr> TranslateSExpToImExp(SExp* exp, RType* hintType, TranslationContexts& contexts)
{
    SExpToImExpTranslator translator{hintType, contexts};
    return Accept(translator, exp);
}

} // namespace Citron