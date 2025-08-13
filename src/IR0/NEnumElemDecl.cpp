#include "NEnumElemDecl.h"
#include <cassert>

#include "Infra/Exceptions.h"

#include "RFuncParameter.h"

#include "NEnumDecl.h"

using namespace std;

namespace Citron {

NEnumElemDecl::NEnumElemDecl(NEnumDecl* _enum, const string& name, size_t varCount)
    : _enum{_enum}, name{name}
{
    vars.reserve(varCount);
}

void NEnumElemDecl::AddVar(NEnumElemVarDecl* var)
{
    vars.push_back(var);
    varsMap.emplace(var->name, var);
}

NDecl* NEnumElemDecl::GetNOuter()
{
    return _enum;
}

RMember NEnumElemDecl::ToRMember(RTypeArguments* typeArgs)
{
    return RMember_EnumElem(typeArgs, this);
}

RDecl* NEnumElemDecl::GetROuter()
{
    return _enum;
}

RIdentifier NEnumElemDecl::GetIdentifier()
{
    return RIdentifier { RName_Normal(name), 0, {} };
}

optional<RMember> NEnumElemDecl::GetMember(RTypeArguments* typeArgs, const RName& name, size_t explicitTypeParamsExceptOuterCount)
{
    if (explicitTypeParamsExceptOuterCount != 0) return nullopt;

    return GetVar(typeArgs, name);
}

optional<RMember> NEnumElemDecl::ResolveIdentifier(const RName& name, size_t explicitTypeParamsExceptOuterCount, IR0Factory& factory)
{
    // VarDecl의 자식이 ResolveIdentifier를 호출할 수 없고, bodyspace도 아니기 때문에 직접 호출할 일이 없다
    throw RuntimeFatalException();
}

REnumDecl* NEnumElemDecl::GetBaseEnumDecl()
{
    return _enum;
}

optional<RMember_EnumElemVar> NEnumElemDecl::GetVar(RTypeArguments* typeArgs, const RName& name)
{
    auto* normalName = get_if<RName_Normal>(&name);
    if (!normalName) return nullopt;

    auto i = varsMap.find(normalName->text);
    if (i == varsMap.end()) return nullopt;

    return RMember_EnumElemVar(typeArgs, i->second);
}

size_t NEnumElemDecl::GetVarCount()
{
    return vars.size();
}

vector<RFuncParameter> NEnumElemDecl::GetUnboundCtorParams()
{
    vector<RFuncParameter> result;

    result.reserve(vars.size());
    for (auto& var : vars)
        result.emplace_back(/*bOut*/ false, var->declType, RName_Normal(var->name));

    return result;
}

} // namespace Citron