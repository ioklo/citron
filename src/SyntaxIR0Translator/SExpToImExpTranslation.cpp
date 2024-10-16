#include "pch.h"
#include "SExpToImExpTranslation.h"

#include <Infra/Ptr.h>
#include <Infra/Exceptions.h>
#include <Syntax/Syntax.h>
#include <Logging/Logger.h>

#include <IR0/RTypeFactory.h>
#include <IR0/RType.h>

#include "ImExp.h"
#include "ReExp.h"

#include "SExpToRExpTranslation.h"
#include "SExpToReExpTranslation.h"
#include "ReExpToRExpTranslation.h"
#include "ReExpToRLocTranslation.h"
#include "ImExpAndMemberNameToImExpTranslation.h"

#include "ScopeContext.h"

#include "DesignatedErrorLogger.h"
#include "Misc.h"

namespace Citron::SyntaxIR0Translator {

namespace {

class SExpToImExpTranslator : public SExpVisitor
{
    RTypePtr hintType;
    ImExpPtr* result;

    ScopeContext& context;
    Logger& logger;
    RTypeFactory& factory;

public:
    SExpToImExpTranslator(const RTypePtr& hintType, ImExpPtr* result, ScopeContext& context, Logger& logger, RTypeFactory& factory)
        : hintType(hintType), result(result), context(context), logger(logger), factory(factory)
    {
    }

    void HandleExp(RExpPtr&& exp)
    {
        if (!exp)
            *result = nullptr;
        else
            *result = MakePtr<ImExp_Else>(std::move(exp));
    }

    // x
    void Visit(SExp_Identifier& exp) override
    {
        static_assert(false);
        /*try
        {
            auto typeArgs = MakeTypeArgs(exp.typeArgs, context, factory);

            var imExp = context.ResolveIdentifier(new Name.Normal(exp.Value), typeArgs);
            if (imExp == null)
            {
                context.AddFatalError(A2007_ResolveIdentifier_NotFound, exp);
                return Error();
            }

            return Valid(imExp);
        }
        catch (IdentifierResolverMultipleCandidatesException)
        {
            context.AddFatalError(A2001_ResolveIdentifier_MultipleCandidatesForIdentifier, exp);
            return Error();
        }*/
    }

    void Visit(SExp_String& exp) override
    {
        HandleExp(TranslateSStringExpToRStringExp(exp, context, logger, factory));
    }

    void Visit(SExp_IntLiteral& exp) override
    {
        HandleExp(TranslateSIntLiteralExpToRExp(exp));
    }

    void Visit(SExp_BoolLiteral& exp) override
    {
        HandleExp(TranslateSBoolLiteralExpToRExp(exp));
    }

    // 'null'
    void Visit(SExp_NullLiteral& exp) override
    {
        HandleExp(TranslateSNullLiteralExpToRExp(exp, hintType, context, logger));
    }

    void Visit(SExp_BinaryOp& exp) override
    {
        HandleExp(TranslateSBinaryOpExpToRExp(exp, context, logger, factory));
    }

    void Visit(SExp_UnaryOp& exp) override
    {
        // *d
        if (exp.kind == SUnaryOpKind::Deref)
        {
            auto target = TranslateSExpToReExp(*exp.operand, /*hintType*/nullptr, context, logger, factory);
            if (!target)
            {
                *result = nullptr;
                return;
            }

            auto targetType = target->GetType(factory);

            if (dynamic_cast<RType_BoxPtr*>(targetType.get()))
            {
                *result = MakePtr<ImExp_BoxDeref>(std::move(target));
                return;
            }

            if (dynamic_cast<RType_LocalPtr*>(targetType.get()))
            {
                *result = MakePtr<ImExp_LocalDeref>(std::move(target));
                return;
            }

            // 에러를 내야 할 것 같다
            throw NotImplementedException();
        }
        else
        {
            return HandleExp(TranslateSUnaryOpExpToRExpExceptDeref(exp, context, logger, factory));
        }
    }

    void Visit(SExp_Call& exp) override
    {
        HandleExp(TranslateSCallExpToRExp(exp, hintType, context, logger, factory));
    }

    void Visit(SExp_Lambda& exp) override
    {
        HandleExp(TranslateSLambdaExpToRExp(exp, logger));
    }

    void Visit(SExp_Indexer& exp) override
    {
        auto reObj = TranslateSExpToReExp(*exp.obj, /*hintType*/ nullptr, context, logger, factory);
        if (!reObj)
        {
            *result = nullptr;
            return;
        }

        auto reIndex = TranslateSExpToReExp(*exp.index, /*hintType*/ nullptr, context, logger, factory);
        if (!reIndex)
        {
            *result = nullptr;
            return;
        }

        auto intType = factory.MakeIntType();

        RLocPtr rIndexLoc;
        if (reIndex->GetType(factory) != intType)
        {
            logger.SetSyntax(exp.index);
            auto rIndexExp = TranslateReExpToRExp(*reIndex, context, logger, factory);
            if (!rIndexExp)
            {
                *result = nullptr;
                return;
            }

            auto rCastIndex = CastRExp(std::move(rIndexExp), intType, context, logger);
            if (!rCastIndex)
            {
                *result = nullptr;
                return;
            }

            rIndexLoc = MakePtr<RLoc_Temp>(std::move(rCastIndex));
        }
        else
        {
            DesignatedErrorLogger designatedErrorLogger(logger, &Logger::Fatal_ResolveIdentifier_ExpressionIsNotLocation);

            rIndexLoc = TranslateReExpToRLoc(*reIndex, /*bWrapExpAsLoc*/ true, &designatedErrorLogger, context, logger, factory);
            if (!rIndexLoc)
            {
                *result = nullptr;
                return;
            }
        }

        // TODO: custom indexer를 만들수 있으면 좋은가
        // var memberResult = objResult.TypeSymbol.QueryMember(new M.Name(M.SpecialName.IndexerGet, null), 0);

        // 리스트 타입의 경우,
        RTypePtr itemType;
        if (context.IsListType(reObj->GetType(factory), &itemType))
        {
            *result = MakePtr<ImExp_ListIndexer>(std::move(reObj), std::move(reIndex), std::move(itemType));
            return;
        }

        throw NotImplementedException();

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
    void Visit(SExp_Member& exp) override
    {
        auto imParent = TranslateSExpToImExp(*exp.parent, hintType, context);
        if (!imParent)
        {
            *result = nullptr;
            return;
        }

        auto typeArgs = MakeTypeArgs(exp.memberTypeArgs, context, factory);

        // logger.SetSyntax(exp);
        *result = TranslateImExpAndMemberNameToImExp(*imParent, exp.memberName, typeArgs, context, logger);
    }

    void Visit(SExp_IndirectMember& exp) override
    {
        static_assert(false);
    }

    void Visit(SExp_List& exp) override
    {
        HandleExp(TranslateSListExpToRExp(exp, context, logger, factory));
    }

    // 'new C(...)'
    void Visit(SExp_New& exp) override
    {
        HandleExp(TranslateSNewExpToRExp(exp, context, logger, factory));
    }

    void Visit(SExp_Box& exp) override
    {
        HandleExp(TranslateSBoxExpToRExp(exp, hintType, context, logger, factory));
    }

    void Visit(SExp_Is& exp) override
    {
        HandleExp(TranslateSIsExpToRExp(exp, context, logger, factory));
    }

    void Visit(SExp_As& exp) override
    {
        HandleExp(TranslateSAsExpToRExp(exp, context, logger, factory));
    }
};

}

ImExpPtr TranslateSExpToImExp(SExp& exp, const RTypePtr& hintType, ScopeContext& context, Logger& logger, RTypeFactory& factory)
{   
    ImExpPtr imExp;
    SExpToImExpTranslator translator(hintType, &imExp, context, logger, factory);
    exp.Accept(translator);
    return imExp;
}

} // namespace Citron::SyntaxIR0Translator