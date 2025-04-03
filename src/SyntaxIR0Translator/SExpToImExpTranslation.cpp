module Citron.SyntaxIR0Translator:SExpToImExpTranslation;

import Citron.Ptr;
import Citron.Exceptions;
import Citron.Syntax;
import Citron.Logger;

import Citron.RDecls;

import :ImExp;
import :ReExp;

import :SExpToNExpTranslation;
import :SExpToReExpTranslation;
import :ReExpToNExpTranslation;
import :ReExpToNLocTranslation;
import :ImExpAndMemberNameToImExpTranslation;

import :TranslationContext;
import :ScopeContext;

import :DesignatedErrorLogger;
import :Misc;

namespace Citron::SyntaxIR0Translator {

namespace {

class SExpToImExpTranslator : public SExpVisitor
{
    RTypePtr hintType;
    ImExpPtr* result;

    TranslationContext& context;

public:
    SExpToImExpTranslator(const RTypePtr& hintType, ImExpPtr* result, TranslationContext& context)
        : hintType(hintType), result(result), context(context)
    {
    }

    void HandleExp(NExpPtr&& exp)
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
        HandleExp(TranslateSStringExpToNStringExp(exp, context));
    }

    void Visit(SExp_IntLiteral& exp) override
    {
        HandleExp(TranslateSIntLiteralExpToNExp(exp));
    }

    void Visit(SExp_BoolLiteral& exp) override
    {
        HandleExp(TranslateSBoolLiteralExpToNExp(exp));
    }

    // 'null'
    void Visit(SExp_NullLiteral& exp) override
    {
        HandleExp(TranslateSNullLiteralExpToNExp(exp, hintType, context));
    }

    void Visit(SExp_BinaryOp& exp) override
    {
        HandleExp(TranslateSBinaryOpExpToNExp(exp, context));
    }

    void Visit(SExp_UnaryOp& exp) override
    {
        // *d
        if (exp.kind == SUnaryOpKind::Deref)
        {
            auto target = TranslateSExpToReExp(*exp.operand, /*hintType*/nullptr, context);
            if (!target)
            {
                *result = nullptr;
                return;
            }

            auto targetType = context.GetType(*target);

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
            return HandleExp(TranslateSUnaryOpExpToNExpExceptDeref(exp, context));
        }
    }

    void Visit(SExp_Call& exp) override
    {
        HandleExp(TranslateSCallExpToNExp(exp, hintType, context));
    }

    void Visit(SExp_Lambda& exp) override
    {
        HandleExp(TranslateSLambdaExpToNExp(exp, context));
    }

    void Visit(SExp_Indexer& exp) override
    {
        auto reObj = TranslateSExpToReExp(*exp.obj, /*hintType*/ nullptr, context);
        if (!reObj)
        {
            *result = nullptr;
            return;
        }

        auto reIndex = TranslateSExpToReExp(*exp.index, /*hintType*/ nullptr, context);
        if (!reIndex)
        {
            *result = nullptr;
            return;
        }

        auto intType = context.MakeIntType();

        NLocPtr nIndexLoc;
        if (context.GetType(*reIndex) != intType)
        {
            context.SetSyntax(exp.index);
            auto nIndexExp = TranslateReExpToNExp(*reIndex, context);
            if (!nIndexExp)
            {
                *result = nullptr;
                return;
            }

            auto nCastIndex = CastNExp(std::move(nIndexExp), intType, context);
            if (!nCastIndex)
            {
                *result = nullptr;
                return;
            }

            nIndexLoc = MakePtr<NLoc_Temp>(std::move(nCastIndex));
        }
        else
        {
            auto designatedErrorLogger = context.MakeDesignatedErrorLogger(&Logger::Fatal_ResolveIdentifier_ExpressionIsNotLocation);

            nIndexLoc = TranslateReExpToNLoc(*reIndex, /*bWrapExpAsLoc*/ true, &designatedErrorLogger, context);
            if (!nIndexLoc)
            {
                *result = nullptr;
                return;
            }
        }

        // TODO: custom indexer를 만들수 있으면 좋은가
        // var memberResult = objResult.TypeSymbol.QueryMember(new M.Name(M.SpecialName.IndexerGet, null), 0);

        // 리스트 타입의 경우,
        RTypePtr itemType;
        if (context.IsListType(context.GetType(*reObj), &itemType))
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

        auto typeArgs = MakeTypeArgs(exp.memberTypeArgs, context);

        // logger.SetSyntax(exp);
        *result = TranslateImExpAndMemberNameToImExp(*imParent, exp.memberName, typeArgs, context);
    }

    void Visit(SExp_IndirectMember& exp) override
    {
        static_assert(false);
    }

    void Visit(SExp_List& exp) override
    {
        HandleExp(TranslateSListExpToNExp(exp, context));
    }

    // 'new C(...)'
    void Visit(SExp_New& exp) override
    {
        HandleExp(TranslateSNewExpToNExp(exp, context));
    }

    void Visit(SExp_Box& exp) override
    {
        HandleExp(TranslateSBoxExpToNExp(exp, hintType, context));
    }

    void Visit(SExp_Is& exp) override
    {
        HandleExp(TranslateSIsExpToNExp(exp, context));
    }

    void Visit(SExp_As& exp) override
    {
        HandleExp(TranslateSAsExpToNExp(exp, context));
    }
};

}

ImExpPtr TranslateSExpToImExp(SExp& exp, const RTypePtr& hintType, TranslationContext& context)
{   
    ImExpPtr imExp;
    SExpToImExpTranslator translator(hintType, &imExp, context);
    exp.Accept(translator);
    return imExp;
}

} // namespace Citron::SyntaxIR0Translator