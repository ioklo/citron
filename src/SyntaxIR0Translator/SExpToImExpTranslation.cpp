#include "SExpToImExpTranslation.h"

#include <expected>

#include "Infra/Ptr.h"
#include "Infra/Exceptions.h"
#include "Infra/Expected.h"

#include "Syntax/Syntax.h"
#include "Logging/Logger.h"
#include "RSymbol/RTypes.h"
#include "MIR/MLoc.h"
#include "MIR/MExp.h"

#include "ImExp.h"
#include "ReExp.h"

#include "SExpToMExpTranslation.h"
#include "SExpToReExpTranslation.h"
#include "ReExpToMExpTranslation.h"
#include "ReExpToMLocTranslation.h"
#include "ImExpAndMemberNameToImExpTranslation.h"

#include "TranslationContext.h"
#include "ScopeContext.h"

#include "Misc.h"

using namespace std;

namespace Citron::SyntaxIR0Translator {

namespace {

class SExpToImExpTranslator
{
public:
    using ResultType = expected<ImExp*, DiagPtr>;

private:
    RType* hintType;
    TranslationContext& context;

public:
    SExpToImExpTranslator(RType* hintType, TranslationContext& context)
        : hintType{hintType}, context{context}
    {
    }

private:
    ResultType HandleExp(expected<MExp*, DiagPtr>&& eExp)
    {
        if (!eExp)
            return unexpected{move(eExp).error()};
        else
            return context.MakeImExp<ImExp_Else>(*eExp);
    }

    template<typename TValue, typename... TArgs> requires std::derived_from<TValue, ImExp>
    ResultType Value(TArgs&&... args)
    {
        return context.MakeImExp<TValue>(forward<TArgs>(args)...);
    }

    template<typename TValue>
    ResultType Error(expected<TValue, DiagPtr>&& e)
    {
        return unexpected{move(e).error()};
    }

    template<typename TDiag, typename... TArgs> requires std::derived_from<TDiag, Diag>
    ResultType Error(TArgs&&... args)
    {
        return unexpected{MakePtr<TDiag>(forward<TArgs>(args)...)};
    }

public:
    // x
    ResultType Visit(SExp_Identifier* exp)
    {
        auto eRTypeArgs = MakeRTypeArgs(exp->typeArgs, context);
        RETURN_ON_ERROR(eRTypeArgs);

        auto eImExp = context.ResolveIdentifier(RName_Normal(exp->value), *eRTypeArgs);
        RETURN_ON_ERROR(eImExp);

        return *eImExp;
    }

    ResultType Visit(SExp_String* exp)
    {
        return HandleExp(TranslateSStringExpToNStringExp(exp, context));
    }

    ResultType Visit(SExp_IntLiteral* exp)
    {
        return HandleExp(TranslateSIntLiteralExpToMExp(exp, context));
    }

    ResultType Visit(SExp_BoolLiteral* exp)
    {
        return HandleExp(TranslateSBoolLiteralExpToMExp(exp, context));
    }

    // 'null'
    ResultType Visit(SExp_NullLiteral* exp)
    {
        return HandleExp(TranslateSNullLiteralExpToMExp(exp, hintType, context));
    }

    ResultType Visit(SExp_BinaryOp* exp)
    {
        return HandleExp(TranslateSBinaryOpExpToMExp(exp, context));
    }

    ResultType Visit(SExp_UnaryOp* exp)
    {
        // *d
        if (exp->kind == SUnaryOpKind::Deref)
        {
            auto eTarget = TranslateSExpToReExp(exp->operand, /*hintType*/nullptr, context);
            if (!eTarget) return Error(move(eTarget));

            auto targetType = context.GetType(*eTarget);

            if (dynamic_cast<RType_BoxPtr*>(targetType))
                return Value<ImExp_BoxDeref>(*eTarget);

            if (dynamic_cast<RType_LocalPtr*>(targetType))
                return Value<ImExp_LocalDeref>(*eTarget);

            // 에러를 내야 할 것 같다
            throw NotImplementedException{};
        }
        else
        {
            return HandleExp(TranslateSUnaryOpExpToMExpExceptDeref(exp, context));
        }
    }

    ResultType Visit(SExp_Call* exp)
    {
        return HandleExp(TranslateSCallExpToMExp(exp, hintType, context));
    }

    ResultType Visit(SExp_Lambda* exp)
    {
        return HandleExp(TranslateSLambdaExpToMExp(exp, context));
    }

    ResultType Visit(SExp_Indexer* exp)
    {
        auto eReObj = TranslateSExpToReExp(exp->obj, /*hintType*/ nullptr, context);
        if (!eReObj) return Error(move(eReObj));

        auto eReIndex = TranslateSExpToReExp(exp->index, /*hintType*/ nullptr, context);
        if (!eReIndex) return Error(move(eReIndex));

        auto intType = context.MakeIntType();

        MLoc* nIndexLoc;
        if (context.GetType(*eReIndex) != intType)
        {
            auto eNIndexExp = TranslateReExpToMExp(*eReIndex, context);
            if (!eNIndexExp) return Error(move(eNIndexExp));

            auto eNCastIndex = CastMExp(*eNIndexExp, intType, context);
            if (!eNCastIndex) return Error(move(eNCastIndex));

            nIndexLoc = context.MakeNLoc<MLoc_Temp>(*eNCastIndex);
        }
        else
        {
            DesignatedDiagnostic<Error_ResolveIdentifier_ExpressionIsNotLocation> designatedDiag;

            auto eNLoc = TranslateReExpToMLoc(*eReIndex, /*bWrapExpAsLoc*/ true, &designatedDiag, context);
            if (!eNLoc) return Error(move(eNLoc));

            nIndexLoc = *eNLoc;
        }

        // TODO: custom indexer를 만들수 있으면 좋은가
        // var memberResult = objResult.TypeSymbol.QueryMember(new M.Name(M.SpecialName.IndexerGet, null), 0);

        // 리스트 타입의 경우,
        RType* itemType;
        if (context.IsListType(context.GetType(*eReObj), &itemType))
        {
            return Value<ImExp_ListIndexer>(*eReObj, *eReIndex, itemType);
        }

        throw NotImplementedException{};

        //// objTypeValue에 indexTypeValue를 인자로 갖고 있는 indexer가 있는지
        //if (!context.TypeValueService.GetMemberFuncValue(objType, SpecialNames.IndexerGet, ImmutableArray<TypeValue>.Empty, out var funcValue))
        //{
        //    context.ErrorCollector.Add(exp, "객체에 indexer함수가 없습니다");
        //    return false;
        //}

        //if (IsFuncStatic(funcValue.FuncId))
        //{
        //    Debug.Fail("객체에 indexer가 있는데 Static입니다");
        //    return false;
        //}

        //var funcTypeValue = context.TypeValueService.GetTypeValue(funcValue);

        //if (!analyzer.CheckParamTypes(exp, funcTypeValue.Params, new[] { indexType }))
        //    return false;

        //var listType = analyzer.GetListTypeValue()

        //// List타입인가 확인
        //if (analyzer.IsAssignable(listType, objType))
        //{
        //    var objTypeId = context.GetTypeId(objType);
        //    var indexTypeId = context.GetTypeId(indexType);

        //    outExp = new ListIndexerExp(new ExpInfo(obj, objTypeId), new ExpInfo(index, indexTypeId));
        //    outTypeValue = funcTypeValue.Return;
        //    return true;
        //}
    }

    // parent."x"<>
    ResultType Visit(SExp_Member* exp)
    {
        auto eImParent = TranslateSExpToImExp(exp->parent, hintType, context);
        if (!eImParent) return Error(move(eImParent));

        auto eRTypeArgs = MakeRTypeArgs(exp->memberTypeArgs, context);
        if (!eRTypeArgs) return Error(move(eRTypeArgs));

        return TranslateImExpAndMemberNameToImExp(*eImParent, exp->memberName, *eRTypeArgs, context);
    }

    ResultType Visit(SExp_IndirectMember* exp)
    {
        throw NotImplementedException{};
    }

    ResultType Visit(SExp_List* exp)
    {
        return HandleExp(TranslateSListExpToMExp(exp, context));
    }

    // 'new C(...)'
    ResultType Visit(SExp_New* exp)
    {
        return HandleExp(TranslateSNewExpToMExp(exp, context));
    }

    ResultType Visit(SExp_Box* exp)
    {
        return HandleExp(TranslateSBoxExpToMExp(exp, hintType, context));
    }

    ResultType Visit(SExp_Is* exp)
    {
        return HandleExp(TranslateSIsExpToMExp(exp, context));
    }

    ResultType Visit(SExp_As* exp)
    {
        return HandleExp(TranslateSAsExpToMExp(exp, context));
    }
};

}

expected<ImExp*, DiagPtr> TranslateSExpToImExp(SExp* exp, RType* hintType, TranslationContext& context)
{
    SExpToImExpTranslator translator{hintType, context};
    return Accept(translator, exp);
}

} // namespace Citron::SyntaxIR0Translator