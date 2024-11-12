#include "pch.h"
#include "ScopeContext.h"

#include <Infra/Ptr.h>
#include <Syntax/Syntax.h>
#include <IR0/RType.h>

#include "FuncContext.h"
#include "ImExp.h"

namespace Citron::SyntaxIR0Translator {

ImExpPtr ScopeContext::ResolveIdentifier(const RName& name, size_t explicitTypeParamsExceptOuterCount, RTypeFactory& factory)
{
    if (auto* normalName = get_if<RName_Normal>(&name))
    {
        // 로컬을 검색한다
        auto i = locals.find(normalName->text);
        if (i != locals.end())
            return MakePtr<ImExp_LocalVar>(i->second, normalName->text);
    }

    if (parentContext)
        return parentContext->ResolveIdentifier(name, explicitTypeParamsExceptOuterCount, factory);

    return funcContext->ResolveIdentifier(name, explicitTypeParamsExceptOuterCount, factory);
}

};