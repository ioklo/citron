module Citron.NDecls:NEnumElemDecl;
import <cassert>;

import Citron.Exceptions;
import :NEnumDecl;

using namespace std;

namespace Citron {

NEnumElemDecl::NEnumElemDecl(weak_ptr<NEnumDecl> _enum, string name, size_t varCount)
    : _enum(move(_enum)), name(move(name))
{
    vars.reserve(varCount);
}

void NEnumElemDecl::AddVar(const std::shared_ptr<NEnumElemVarDecl>& var)
{
    vars.push_back(var);
    varsMap.emplace(var->name, std::move(var));
}

NDecl* NEnumElemDecl::GetNOuter()
{
    return _enum.lock().get();
}

RMember NEnumElemDecl::ToRMember(const shared_ptr<NTypeDecl>& sharedThis, const RTypeArgumentsPtr& typeArgs)
{
    auto sharedEnumElemDecl = dynamic_pointer_cast<NEnumElemDecl>(sharedThis);
    assert(sharedEnumElemDecl);
    return RMember_EnumElem(typeArgs, sharedEnumElemDecl);
}

RDecl* NEnumElemDecl::GetROuter()
{
    return _enum.lock().get();
}

RIdentifier NEnumElemDecl::GetIdentifier()
{
    return RIdentifier { RName_Normal(name), 0, {} };
}

optional<RMember> NEnumElemDecl::GetMember(const RTypeArgumentsPtr& typeArgs, const RName& name, size_t explicitTypeParamsExceptOuterCount)
{
    if (explicitTypeParamsExceptOuterCount != 0) return nullopt;

    return GetVar(typeArgs, name);
}

optional<RMember> NEnumElemDecl::ResolveIdentifier(const RName& name, size_t explicitTypeParamsExceptOuterCount, RTypeFactory& factory)
{
    // VarDecl의 자식이 ResolveIdentifier를 호출할 수 없고, bodyspace도 아니기 때문에 직접 호출할 일이 없다
    throw RuntimeFatalException();
}

optional<RMember_EnumElemVar> NEnumElemDecl::GetVar(const RTypeArgumentsPtr& typeArgs, const RName& name)
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