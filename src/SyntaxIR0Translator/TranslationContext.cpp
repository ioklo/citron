#include "pch.h"

#include <Infra/Ptr.h>
#include <Infra/Exceptions.h>

#include <Logging/Logger.h>

#include <IR0/RLoc.h>
#include <IR0/RTypeFactory.h>
#include <IR0/RType.h>

#include "ReExp.h"
#include "IrExp.h"
#include "TranslationContext.h"
#include "ScopeContext.h"
#include "BodyContext.h"
#include "BinOpQueryService.h"

using namespace std;

namespace Citron::SyntaxIR0Translator {

TranslationContext::TranslationContext(const GlobalContextPtr& globalContext, const BodyContextPtr& bodyContext, const ScopeContextPtr& scopeContext, const LoggerPtr& logger, const RTypeFactoryPtr& factory, const BinOpQueryServicePtr& binOpQueryService)
    : globalContext(globalContext), bodyContext(bodyContext), scopeContext(scopeContext), logger(logger), factory(factory), binOpQueryService(binOpQueryService)
{
}

TranslationContext TranslationContext::MakeNestedScopeContext()
{
    auto newScopeContext = MakePtr<ScopeContext>(bodyContext, scopeContext, scopeContext->nestedLoop);
    return { globalContext, bodyContext, newScopeContext, logger, factory, binOpQueryService };
}

TranslationContext TranslationContext::MakeNestedLoopScopeContext()
{
    auto newScopeContext = MakePtr<ScopeContext>(bodyContext, scopeContext, scopeContext->nestedLoop + 1);
    return { globalContext, bodyContext, newScopeContext, logger, factory, binOpQueryService };
}

TranslationContext TranslationContext::MakeLambdaBodyContext(RFuncReturn&& funcRet, std::vector<RFuncParameter>&& funcParams, bool bLastParamVariadic)
{   
    auto newBodyContext = bodyContext->MakeLambdaBodyContext(scopeContext, std::move(funcRet), std::move(funcParams), bLastParamVariadic);
    auto newScopeContext = MakePtr<ScopeContext>(newBodyContext, nullptr, 0);

    return { globalContext, newBodyContext, newScopeContext, logger, factory, binOpQueryService };
}

DesignatedErrorLogger TranslationContext::MakeDesignatedErrorLogger(void (Logger::* func)())
{
    // logger가 heap에 생성되어있으니 참조를 생성해도 괜찮다
    return { *logger, func }; 
}

Citron::RTypePtr TranslationContext::GetType(RLoc& loc)
{
    return loc.GetType(*factory);
}

RTypePtr TranslationContext::GetType(ReExp& reExp)
{
    return reExp.GetType(*factory);
}

Citron::RTypePtr TranslationContext::GetType(RExp& exp)
{
    return exp.GetType(*factory);
}

RTypePtr TranslationContext::GetTargetType(IrExp_BoxRef& boxRef)
{
    return boxRef.GetTargetType(*factory);
}

std::shared_ptr<Citron::RLoc_This> TranslationContext::MakeThisLoc()
{
    return scopeContext->MakeThisLoc(*factory);
}

RExpPtr TranslationContext::MakeRExp_As(RExpPtr&& targetExp, const RTypePtr& testType)
{
    auto targetType = targetExp->GetType(*factory);
    auto targetTypeKind = targetType->GetCustomTypeKind();
    auto testTypeKind = testType->GetCustomTypeKind();

    // 5가지 케이스로 나뉜다
    if (testTypeKind == RCustomTypeKind::Class)
    {
        if (targetTypeKind == RCustomTypeKind::Class)
            return MakePtr<RExp_ClassAsClass>(std::move(targetExp), testType);

        else if (targetTypeKind == RCustomTypeKind::Interface)
            return MakePtr<RExp_InterfaceAsClass>(std::move(targetExp), testType);
        else
            throw NotImplementedException(); // 에러 처리
    }
    else if (testTypeKind == RCustomTypeKind::Interface)
    {
        if (targetTypeKind == RCustomTypeKind::Class)
            return MakePtr<RExp_ClassAsInterface>(std::move(targetExp), testType);
        else if (targetTypeKind == RCustomTypeKind::Interface)
            return MakePtr<RExp_InterfaceAsInterface>(std::move(targetExp), testType);
        else
            throw NotImplementedException(); // 에러 처리
    }
    else if (testTypeKind == RCustomTypeKind::EnumElem)
    {
        if (targetTypeKind == RCustomTypeKind::Enum)
            return MakePtr<RExp_EnumAsEnumElem>(std::move(targetExp), testType);
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
    bool IsVarType(STypeExp& typeExp)
    {
        auto* idTypeExp = dynamic_cast<STypeExp_Id*>(&typeExp);
        return idTypeExp && idTypeExp->name == "var" && idTypeExp->typeArgs.size() == 0;
    }

    void Normal(STypeExp& typeExp)
    {
        auto type = context.TranslateSTypeExpToRType(typeExp);
        *result = DeclTypeInfo(DeclTypeInfoKind::Normal, type);
    }

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
    return bodyContext->CanAccess(target);
}

bool TranslationContext::IsSeqFunc()
{
    return bodyContext->bSeqFunc;
}

RFuncReturn TranslationContext::GetFuncReturn()
{
    return bodyContext->GetFuncReturn();
}

void TranslationContext::SetFuncReturn(RTypePtr&& retType)
{
    bodyContext->SetFuncReturn(std::move(retType));
}

void TranslationContext::SetSyntax(const SSyntaxPtr& syntax)
{
    logger->SetSyntax(syntax);
}


RTypePtr TranslationContext::TranslateSTypeExpToRType(STypeExp& typeExp)
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


} // namespace Citron::SyntaxIR0Translator