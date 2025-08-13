#include "ScopeContext.h"

#include "Infra/Ptr.h"
#include "Infra/Exceptions.h"
#include "Syntax/Syntax.h"
#include "IR0/RTypeArguments.h"
#include "IR0/RMember.h"
#include "IR0/RNames.h"
#include "IR0/RFuncParameter.h"
#include "IR0/NLambdaDecl.h"

#include "FuncContext.h"

using namespace std;

namespace Citron::SyntaxIR0Translator {

ScopeContext::ScopeContext(const FuncContextPtr& funcContext, const ScopeContextPtr& parentContext, int nestedLoop)
    : funcContext{funcContext}, parentContext{parentContext}, nestedLoop{nestedLoop}
{
}

ScopeContextPtr ScopeContext::Clone(CloneContext& context)
{
    throw NotImplementedException();
}

void ScopeContext::Update(ScopeContext& src, UpdateContext& context)
{
    throw NotImplementedException();
}

RTypeArguments* ScopeContext::MakeOpenTypeArgs(RFactory& factory)
{
    // funcContext로 점프
    return funcContext->MakeOpenTypeArgs(factory);
}

void ScopeContext::SetFlowEndsCompletely()
{
    throw NotImplementedException();
}

shared_ptr<ScopeContext> ScopeContext::MakeNestedScopeContext(shared_ptr<ScopeContext> sharedThis)
{
    throw NotImplementedException();
}

shared_ptr<ScopeContext> ScopeContext::MakeLoopNestedScopeContext(shared_ptr<ScopeContext> sharedThis)
{
    throw NotImplementedException();
}

tuple<ScopeContextPtr, NLambdaDecl> ScopeContext::MakeLambdaBodyContext(const RFuncReturn& ret, vector<RFuncParameter> params, bool bLastParamVariadic)
{
    throw NotImplementedException();
}

void ScopeContext::AddLocalVarInfo(RType* type, const RName& name)
{
    throw NotImplementedException();
}


bool ScopeContext::DoesLocalVarNameExistInScope(const string& name)
{
    throw NotImplementedException();
}

bool ScopeContext::IsFailed() 
{
    throw NotImplementedException();
}

expected<RType*, DiagPtr> ScopeContext::TranslateSTypeExpToRType(STypeExp* typeExp, RFactory& factory)
{
    throw NotImplementedException();
}

NLoc_This* ScopeContext::MakeThisLoc(RFactory& factory)
{
    throw NotImplementedException();
}

optional<RMember> ScopeContext::ResolveIdentifier(const RName& name, size_t explicitTypeParamsExceptOuterCount, RFactory& factory)
{
    if (auto* normalName = get_if<RName_Normal>(&name))
    {
        // 로컬을 검색한다
        auto i = locals.find(normalName->text);
        if (i != locals.end())
            return RMember_LocalVar(i->second, normalName->text);
    }

    // 상위 스코프가 있으면 그곳을 검색한다
    if (parentContext)
        return parentContext->ResolveIdentifier(name, explicitTypeParamsExceptOuterCount, factory);

    // 상위 스코프가 없으면 scope가 속해있는 함수 컨텍스트를 검색한다
    return funcContext->ResolveIdentifier(name, explicitTypeParamsExceptOuterCount, factory);
}

};