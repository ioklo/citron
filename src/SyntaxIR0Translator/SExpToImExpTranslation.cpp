module Citron.SyntaxIR0Translator:SExpToImExpTranslation;

import <expected>;

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

import :Misc;

using namespace std;

namespace Citron::SyntaxIR0Translator {

namespace {

class SExpToImExpTranslator : public SExpVisitor
{
    expected<ImExpPtr, DiagPtr>* result;
    RTypePtr hintType;

    TranslationContext& context;

public:
    SExpToImExpTranslator(expected<ImExpPtr, DiagPtr>* result, const RTypePtr& hintType, TranslationContext& context)
        : result(result), hintType(hintType), context(context)
    {
    }

private:
    void HandleExp(expected<NExpPtr, DiagPtr>&& eExp)
    {
        if (!eExp)
            *result = nullptr;
        else
            *result = MakePtr<ImExp_Else>(move(*eExp));
    }

    void Forward(expected<ImExpPtr, DiagPtr>&& r)
    {
        *result = move(r);
    }

    template<typename TValue, typename... TArgs> requires std::is_base_of_v<ImExp, TValue>
    void Value(TArgs&&... args)
    {
        *result = MakePtr<TValue>(forward<TArgs>(args)...);
    }

    template<typename TValue>
    void Error(expected<TValue, DiagPtr>&& e)
    {
        *result = unexpected{move(e).error()};
    }

    template<typename TDiag, typename... TArgs> requires std::is_base_of_v<Diag, TDiag>
    void Error(TArgs&&... args)
    {
        *result = unexpected{MakePtr<TDiag>(forward<TArgs>(args)...)};
    }

public:
    // x
    void Visit(SExp_Identifier& exp) override
    {
        throw NotImplementedException();
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
        return HandleExp(TranslateSStringExpToNStringExp(exp, context));
    }

    void Visit(SExp_IntLiteral& exp) override
    {
        return HandleExp(TranslateSIntLiteralExpToNExp(exp));
    }

    void Visit(SExp_BoolLiteral& exp) override
    {
        return HandleExp(TranslateSBoolLiteralExpToNExp(exp));
    }

    // 'null'
    void Visit(SExp_NullLiteral& exp) override
    {
        return HandleExp(TranslateSNullLiteralExpToNExp(exp, hintType, context));
    }

    void Visit(SExp_BinaryOp& exp) override
    {
        return HandleExp(TranslateSBinaryOpExpToNExp(exp, context));
    }

    void Visit(SExp_UnaryOp& exp) override
    {
        // *d
        if (exp.kind == SUnaryOpKind::Deref)
        {
            auto eTarget = TranslateSExpToReExp(*exp.operand, /*hintType*/nullptr, context);
            if (!eTarget) return Error(move(eTarget));

            auto targetType = context.GetType(**eTarget);

            if (dynamic_cast<RType_BoxPtr*>(targetType.get()))
                return Value<ImExp_BoxDeref>(move(*eTarget));

            if (dynamic_cast<RType_LocalPtr*>(targetType.get()))
                return Value<ImExp_LocalDeref>(move(*eTarget));

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
        return HandleExp(TranslateSCallExpToNExp(exp, hintType, context));
    }

    void Visit(SExp_Lambda& exp) override
    {
        return HandleExp(TranslateSLambdaExpToNExp(exp, context));
    }

    void Visit(SExp_Indexer& exp) override
    {
        auto eReObj = TranslateSExpToReExp(*exp.obj, /*hintType*/ nullptr, context);
        if (!eReObj) return Error(move(eReObj));

        auto eReIndex = TranslateSExpToReExp(*exp.index, /*hintType*/ nullptr, context);
        if (!eReIndex) return Error(move(eReIndex));

        auto intType = context.MakeIntType();

        NLocPtr nIndexLoc;
        if (context.GetType(**eReIndex) != intType)
        {
            context.SetSyntax(exp.index);
            auto eNIndexExp = TranslateReExpToNExp(**eReIndex, context);
            if (!eNIndexExp) return Error(move(eNIndexExp));

            auto eNCastIndex = CastNExp(move(*eNIndexExp), intType, context);
            if (!eNCastIndex) return Error(move(eNCastIndex));

            nIndexLoc = MakePtr<NLoc_Temp>(move(*eNCastIndex));
        }
        else
        {
            DesignatedDiagnostic<Error_ResolveIdentifier_ExpressionIsNotLocation> designatedDiag;

            auto eNLoc = TranslateReExpToNLoc(**eReIndex, /*bWrapExpAsLoc*/ true, &designatedDiag, context);
            if (!eNLoc) return Error(move(eNLoc));

            nIndexLoc = *eNLoc;
        }

        // TODO: custom indexer를 만들수 있으면 좋은가
        // var memberResult = objResult.TypeSymbol.QueryMember(new M.Name(M.SpecialName.IndexerGet, null), 0);

        // 리스트 타입의 경우,
        RTypePtr itemType;
        if (context.IsListType(context.GetType(**eReObj), &itemType))
        {
            return Value<ImExp_ListIndexer>(move(*eReObj), move(*eReIndex), move(itemType));
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
        auto eImParent = TranslateSExpToImExp(*exp.parent, hintType, context);
        if (!eImParent) return Error(move(eImParent));

        auto eTypeArgs = MakeTypeArgs(exp.memberTypeArgs, context);
        if (!eTypeArgs) return Error(move(eTypeArgs));

        // logger.SetSyntax(exp);
        return Forward(TranslateImExpAndMemberNameToImExp(**eImParent, exp.memberName, *eTypeArgs, context));
    }

    void Visit(SExp_IndirectMember& exp) override
    {
        throw NotImplementedException();
    }

    void Visit(SExp_List& exp) override
    {
        return HandleExp(TranslateSListExpToNExp(exp, context));
    }

    // 'new C(...)'
    void Visit(SExp_New& exp) override
    {
        return HandleExp(TranslateSNewExpToNExp(exp, context));
    }

    void Visit(SExp_Box& exp) override
    {
        return HandleExp(TranslateSBoxExpToNExp(exp, hintType, context));
    }

    void Visit(SExp_Is& exp) override
    {
        return HandleExp(TranslateSIsExpToNExp(exp, context));
    }

    void Visit(SExp_As& exp) override
    {
        return HandleExp(TranslateSAsExpToNExp(exp, context));
    }
};

}

expected<ImExpPtr, DiagPtr> TranslateSExpToImExp(SExp& exp, const RTypePtr& hintType, TranslationContext& context)
{   
    expected<ImExpPtr, DiagPtr> imExp;
    SExpToImExpTranslator translator{&imExp, hintType, context};
    exp.Accept(translator);
    return imExp;
}

} // namespace Citron::SyntaxIR0Translator