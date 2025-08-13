#include "TranslationContext.h"

#include "Infra/Ptr.h"
#include "Infra/Exceptions.h"
#include "Logging/Logger.h"
#include "IR0/IR0Factory.h"
#include "IR0/RFuncDecl.h"
#include "IR0/RDecl.h"
#include "IR0/NLoc.h"
#include "IR0/NExp.h"

#include "ReExp.h"
#include "IrExp.h"
#include "TranslationContext.h"
#include "ScopeContext.h"
#include "FuncContext.h"
#include "BinOpQueryService.h"

#include "Misc.h"

using namespace std;

namespace Citron::SyntaxIR0Translator {

TranslationContext::TranslationContext(const GlobalContextPtr& globalContext, const FuncContextPtr& funcContext, const ScopeContextPtr& scopeContext, const LoggerPtr& logger, const IR0FactoryPtr& factory, const BinOpQueryServicePtr& binOpQueryService)
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

expected<RType*, DiagPtr> TranslationContext::TranslateSTypeExpToRType(STypeExp& typeExp)
{
    return scopeContext->TranslateSTypeExpToRType(typeExp, *factory);
}

Citron::RType* TranslationContext::GetType(NLoc& loc)
{
    return loc.GetType(*factory);
}

RType* TranslationContext::GetType(ReExp& reExp)
{
    return reExp.GetType(*factory);
}

Citron::RType* TranslationContext::GetType(NExp& exp)
{
    return exp.GetType(*factory);
}

RType* TranslationContext::GetTargetType(IrExp_BoxRef& boxRef)
{
    return boxRef.GetTargetType(*factory);
}

NLoc_This* TranslationContext::MakeThisLoc()
{
    return scopeContext->MakeThisLoc(*factory);
}

expected<NExp*, DiagPtr> TranslationContext::MakeNExp_As(NExp* targetExp, RType* testType)
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

bool TranslationContext::DoesLocalVarNameExistInScope(const std::string& name)
{
    throw NotImplementedException();
}

void TranslationContext::AddLocalVarInfo(RType* type, RName&& name)
{
    throw NotImplementedException();
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

void TranslationContext::SetOpenFuncReturn(RType* retType)
{
    funcContext->SetOpenFuncReturn(move(retType));
}

NLambdaDeclAndArgs TranslationContext::MakeLambdaDeclAndArgs(std::vector<NStmt*>&& body)
{
    throw NotImplementedException();
}

RTypeArguments* TranslationContext::MakeTypeArguments(const std::vector<RType*>& items)
{
    return factory->MakeTypeArguments(items);
}

RTypeArguments* TranslationContext::MergeTypeArguments(RTypeArguments& typeArgs0, RTypeArguments& typeArgs1)
{
    return factory->MergeTypeArguments(typeArgs0, typeArgs1);
}

RType* TranslationContext::MakeVoidType()
{
    return factory->MakeVoidType();
}

RType* TranslationContext::MakeBoolType()
{
    return factory->MakeBoolType();
}

RType* TranslationContext::MakeIntType()
{
    return factory->MakeIntType();
}

RType* TranslationContext::MakeStringType()
{
    return factory->MakeStringType();
}

bool TranslationContext::IsListType(RType* type, RType* outItemType)
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

RType_Enum* TranslationContext::GetBaseEnumType(RType_EnumElem& enumElemType)
{
    return enumElemType.GetBaseEnumType(*factory);
}

expected<ImExp*, shared_ptr<ResolveIdentifierError>> TranslationContext::ResolveIdentifier(RName&& name, RTypeArguments* typeArgs)
{
    throw NotImplementedException();
}

} // namespace Citron::SyntaxIR0Translator