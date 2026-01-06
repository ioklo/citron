#include "ScopeContext.h"

#include "Infra/Ptr.h"
#include "Infra/Exceptions.h"
#include "Syntax/Syntax.h"
#include "RSymbol/RTypeArguments.h"
#include "RSymbol/RMember.h"
#include "RSymbol/RNames.h"
#include "RSymbol/RFuncParameter.h"
#include "RSymbol/RFactory.h"
#include "NSymbol/NLambdaDecl.h"

#include "FuncContext.h"

using namespace std;

namespace Citron {

ScopeContext::ScopeContext(const FuncContextPtr& funcContext, const ScopeContextPtr& parentContext, int nestedLoop, const RFactoryPtr& rFactory)
    : funcContext{funcContext}, parentContext{parentContext}, nestedLoop{nestedLoop}, rFactory{rFactory}
{
}

ScopeContextPtr ScopeContext::Clone(CloneContext& context)
{
    throw NotImplementedException{};
}

void ScopeContext::Update(ScopeContext& src, UpdateContext& context)
{
    throw NotImplementedException{};
}

RTypeArguments* ScopeContext::MakeOpenTypeArgs()
{
    // funcContext로 점프
    return funcContext->MakeOpenTypeArgs();
}

void ScopeContext::SetFlowEndsCompletely()
{
    throw NotImplementedException{};
}

shared_ptr<ScopeContext> ScopeContext::MakeTranslationContexts_NestedScope(shared_ptr<ScopeContext> sharedThis)
{
    throw NotImplementedException{};
}

shared_ptr<ScopeContext> ScopeContext::MakeLoopNestedScopeContext(shared_ptr<ScopeContext> sharedThis)
{
    throw NotImplementedException{};
}

tuple<ScopeContextPtr, NLambdaDecl> ScopeContext::MakeTranslationContexts_Lambda(const RFuncReturn& ret, vector<RFuncParameter> params, bool bLastParamVariadic)
{
    throw NotImplementedException{};
}

void ScopeContext::AddLocalVarInfo(RType* type, const RName& name)
{
    auto [i, b] = localInfos.try_emplace(name, LocalInfo{LocalInfoKind::Var, type});
    assert(b);
}

void ScopeContext::AddLocalRefInfo(RType* type, const RName& name)
{
    auto [i, b] = localInfos.try_emplace(name, LocalInfo{LocalInfoKind::Ref, type});
    assert(b);
}

bool ScopeContext::DoesLocalNameExistInScope(const RName& name)
{
    auto i = localInfos.find(name);
    return i != localInfos.end();
}

bool ScopeContext::IsFailed() 
{
    throw NotImplementedException{};
}

expected<RType*, DiagPtr> ScopeContext::TranslateSTypeExpToRType(STypeExp* sTypeExp)
{
    // TODO: BuildTypeDependentSymbolContext::MakeType 에도 같은 코드가 있다
    struct Visitor
    {
        using ResultType = RType*;

        RFactory* rFactory;
        ScopeContext& scopeContext;

        RType* Visit(STypeExp_Id* idExp)
        {
            // 예약어 처리
            if (idExp->name == "void" && idExp->typeArgs.empty())
                return rFactory->MakeVoidType();
            else if (idExp->name == "bool" && idExp->typeArgs.empty())
                return rFactory->MakeBoolType();
            else if (idExp->name == "int" && idExp->typeArgs.empty())
                return rFactory->MakeIntType();
            else if (idExp->name == "string" && idExp->typeArgs.empty())
                return rFactory->MakeStringType();

            auto* rTypeDecl = scopeContext.funcContext->ResolveTypeDecl(RName_Normal{idExp->name}, idExp->typeArgs.size());
            if (!rTypeDecl) return nullptr;

            vector<RType*> rTypeArgVector;
            rTypeArgVector.reserve(idExp->typeArgs.size());
            for (auto* sTypeArg : idExp->typeArgs)
            {
                auto e_rTypeArg = scopeContext.TranslateSTypeExpToRType(sTypeArg);
                if (!e_rTypeArg) return nullptr;
                rTypeArgVector.push_back(*e_rTypeArg);
            }
            auto* rTypeArgs = rFactory->MakeTypeArguments(rTypeArgVector);
            return rFactory->MakeType(rTypeDecl, rTypeArgs);
        }

        RType* Visit(STypeExp* e)
        {
            throw NotImplementedException{};
        }
    } visitor{rFactory.get(), *this};

    return Accept(visitor, sTypeExp);
}

expected<optional<RMember>, DiagPtr> ScopeContext::ResolveIdentifier(const RName& name, size_t explicitTypeParamsExceptOuterCount)
{
    // 로컬을 검색한다
    auto i = localInfos.find(name);
    if (i != localInfos.end())
    {
        if (i->second.kind == LocalInfoKind::Var)
            return RMember_LocalVar(i->second.type, name);
        else if (i->second.kind == LocalInfoKind::Ref)
            return RMember_LocalRef(i->second.type, name);
        else assert(false);
    }

    // 상위 스코프가 있으면 그곳을 검색한다
    if (parentContext)
        return parentContext->ResolveIdentifier(name, explicitTypeParamsExceptOuterCount);

    // 상위 스코프가 없으면 scope가 속해있는 함수 컨텍스트를 검색한다
    return funcContext->ResolveIdentifier(name, explicitTypeParamsExceptOuterCount);
}

};