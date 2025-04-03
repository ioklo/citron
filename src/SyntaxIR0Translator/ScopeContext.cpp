module Citron.SyntaxIR0Translator:ScopeContext;

import Citron.Ptr;
import Citron.Syntax;
import Citron.RDecls;

import :FuncContext;

using namespace std;

namespace Citron::SyntaxIR0Translator {

optional<RMember> ScopeContext::ResolveIdentifier(const RName& name, size_t explicitTypeParamsExceptOuterCount, RTypeFactory& factory)
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

RTypeArgumentsPtr ScopeContext::MakeOpenTypeArgs(RTypeFactory& factory)
{
    // funcContext로 점프
    return funcContext->MakeOpenTypeArgs(factory);
}

};