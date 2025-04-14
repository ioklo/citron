module Citron.SyntaxIR0Translator:TranslationContext;

import Citron.Ptr;
import Citron.Exceptions;

import Citron.Logger;

import Citron.RDecls;
import Citron.NDecls;

import :ReExp;
import :IrExp;
import :TranslationContext;
import :ScopeContext;
import :FuncContext;
import :BinOpQueryService;

import :Misc;

using namespace std;

namespace Citron::SyntaxIR0Translator {

TranslationContext::TranslationContext(const GlobalContextPtr& globalContext, const FuncContextPtr& funcContext, const ScopeContextPtr& scopeContext, const LoggerPtr& logger, const RTypeFactoryPtr& factory, const BinOpQueryServicePtr& binOpQueryService)
    : globalContext(globalContext), funcContext(funcContext), scopeContext(scopeContext), logger(logger), factory(factory), binOpQueryService(binOpQueryService)
{
}

TranslationContext TranslationContext::MakeNestedScopeContext()
{
    auto newScopeContext = MakePtr<ScopeContext>(funcContext, scopeContext, scopeContext->nestedLoop);
    return { globalContext, funcContext, newScopeContext, logger, factory, binOpQueryService };
}

TranslationContext TranslationContext::MakeNestedLoopScopeContext()
{
    auto newScopeContext = MakePtr<ScopeContext>(funcContext, scopeContext, scopeContext->nestedLoop + 1);
    return { globalContext, funcContext, newScopeContext, logger, factory, binOpQueryService };
}

TranslationContext TranslationContext::MakeLambdaBodyContext(RFuncReturn&& funcRet, std::vector<RFuncParameter>&& funcParams, bool bLastParamVariadic)
{   
    auto newFuncContext = MakePtr<FuncContext_Lambda>(scopeContext, /*bSeqFunc*/ false, move(funcRet), move(funcParams), bLastParamVariadic);
    auto newScopeContext = MakePtr<ScopeContext>(newFuncContext, nullptr, 0);

    return { globalContext, newFuncContext, newScopeContext, logger, factory, binOpQueryService };
}

Citron::RTypePtr TranslationContext::GetType(NLoc& loc)
{
    return loc.GetType(*factory);
}

RTypePtr TranslationContext::GetType(ReExp& reExp)
{
    return reExp.GetType(*factory);
}

Citron::RTypePtr TranslationContext::GetType(NExp& exp)
{
    return exp.GetType(*factory);
}

RTypePtr TranslationContext::GetTargetType(IrExp_BoxRef& boxRef)
{
    return boxRef.GetTargetType(*factory);
}

std::shared_ptr<Citron::NLoc_This> TranslationContext::MakeThisLoc()
{
    return scopeContext->MakeThisLoc(*factory);
}

expected<NExpPtr, DiagPtr> TranslationContext::MakeNExp_As(NExpPtr&& targetExp, const RTypePtr& testType)
{
    auto targetType = targetExp->GetType(*factory);
    auto targetTypeKind = targetType->GetCustomTypeKind();
    auto testTypeKind = testType->GetCustomTypeKind();

    // 5가지 케이스로 나뉜다
    if (testTypeKind == RCustomTypeKind::Class)
    {
        if (targetTypeKind == RCustomTypeKind::Class)
            return MakePtr<NExp_ClassAsClass>(move(targetExp), testType);

        else if (targetTypeKind == RCustomTypeKind::Interface)
            return MakePtr<NExp_InterfaceAsClass>(move(targetExp), testType);
        else
            throw NotImplementedException(); // 에러 처리
    }
    else if (testTypeKind == RCustomTypeKind::Interface)
    {
        if (targetTypeKind == RCustomTypeKind::Class)
            return MakePtr<NExp_ClassAsInterface>(move(targetExp), testType);
        else if (targetTypeKind == RCustomTypeKind::Interface)
            return MakePtr<NExp_InterfaceAsInterface>(move(targetExp), testType);
        else
            throw NotImplementedException(); // 에러 처리
    }
    else if (testTypeKind == RCustomTypeKind::EnumElem)
    {
        if (targetTypeKind == RCustomTypeKind::Enum)
            return MakePtr<NExp_EnumAsEnumElem>(move(targetExp), testType);
        else
            throw NotImplementedException(); // 에러 처리
    }
    else
        throw NotImplementedException(); // 에러 처리
}

bool TranslationContext::IsInLoop()
{
    return scopeContext->IsInLoop();
}

struct DeclTypeVisitor : public STypeExpVisitor
{
    DeclTypeInfo* result;
    TranslationContext& context;

public:
    DeclTypeVisitor(DeclTypeInfo* result, TranslationContext& context)
        : result(result), context(context)
    {
    }

private:
    void Normal(STypeExp& typeExp)
    {
        auto eRType = context.TranslateSTypeExpToRType(typeExp);
        *result = DeclTypeInfo(DeclTypeInfoKind::Normal, *eRType);
    }

public:
    void Visit(STypeExp_Id& typeExp) override
    {
        if (!IsVarType(typeExp))
            return Normal(typeExp);

        *result = DeclTypeInfo { DeclTypeInfoKind::PlainVar, /*type*/ nullptr };
    }

    void Visit(STypeExp_Member& typeExp) override
    {
        return Normal(typeExp);
    }

    // var?
    void Visit(STypeExp_Nullable& typeExp) override
    {
        if (!IsVarType(*typeExp.innerType))
            return Normal(typeExp);

        *result = DeclTypeInfo { DeclTypeInfoKind::NullableVar, /*type*/ nullptr };
    }

    void Visit(STypeExp_LocalPtr& typeExp) override
    {
        if (!IsVarType(*typeExp.innerType))
            return Normal(typeExp);

        *result = DeclTypeInfo { DeclTypeInfoKind::LocalPtrVar, /*type*/ nullptr };
    }

    void Visit(STypeExp_BoxPtr& typeExp) override
    {
        if (!IsVarType(*typeExp.innerType))
            return Normal(typeExp);

        *result = DeclTypeInfo { DeclTypeInfoKind::BoxPtrVar, /*type*/ nullptr };
    }

    // local var i = ...
    void Visit(STypeExp_Local& typeExp) override
    {
        if (!IsVarType(*typeExp.innerType))
            return Normal(typeExp);

        *result = DeclTypeInfo { DeclTypeInfoKind::LocalInterfaceVar, /*type*/ nullptr };
    }
};

DeclTypeInfo TranslationContext::GetDeclTypeInfo(STypeExp& typeExp)
{
    DeclTypeInfo info;
    DeclTypeVisitor visitor(&info, *this);
    typeExp.Accept(visitor);
    return info;
}

bool TranslationContext::CanAccess(RDecl* target)
{
    return funcContext->CanAccess(target);
}

bool TranslationContext::IsSeqFunc()
{
    return funcContext->IsSeqFunc();
}

RFuncReturn TranslationContext::GetUnboundFuncReturn()
{
    return funcContext->GetUnboundFuncReturn();
}

void TranslationContext::SetOpenFuncReturn(RTypePtr&& retType)
{
    funcContext->SetOpenFuncReturn(move(retType));
}

void TranslationContext::SetSyntax(const SSyntaxPtr& syntax)
{
    logger->SetSyntax(syntax);
}


std::expected<RTypePtr, DiagPtr> TranslationContext::TranslateSTypeExpToRType(STypeExp& typeExp)
{
    return scopeContext->TranslateSTypeExpToRType(typeExp, *factory);
}

RTypeArgumentsPtr TranslationContext::MakeTypeArguments(const std::vector<RTypePtr>& items)
{
    return factory->MakeTypeArguments(items);
}

RTypeArgumentsPtr TranslationContext::MergeTypeArguments(RTypeArguments& typeArgs0, RTypeArguments& typeArgs1)
{
    return factory->MergeTypeArguments(typeArgs0, typeArgs1);
}

RTypePtr TranslationContext::MakeVoidType()
{
    return factory->MakeVoidType();
}

RTypePtr TranslationContext::MakeBoolType()
{
    return factory->MakeBoolType();
}

RTypePtr TranslationContext::MakeIntType()
{
    return factory->MakeIntType();
}

RTypePtr TranslationContext::MakeStringType()
{
    return factory->MakeStringType();
}

bool TranslationContext::IsListType(const RTypePtr& type, RTypePtr* outItemType)
{
    return factory->IsListType(type, outItemType);
}

const vector<BinOpInfo>& TranslationContext::GetBinOpInfos(SBinaryOpKind kind)
{
    return binOpQueryService->GetInfos(kind);
}

RFuncReturn TranslationContext::GetFuncReturn(RFuncDecl& decl, RTypeArguments& typeArgs)
{
    return decl.GetFuncReturn(typeArgs, *factory);
}

RFuncParameter TranslationContext::GetFuncParam(RFuncDecl& decl, RTypeArguments& typeArgs, size_t index)
{
    return decl.GetFuncParam(typeArgs, index, *factory);
}

shared_ptr<RType_Enum> TranslationContext::GetBaseEnumType(RType_EnumElem& enumElemType)
{
    return enumElemType.GetBaseEnumType(*factory);
}


} // namespace Citron::SyntaxIR0Translator