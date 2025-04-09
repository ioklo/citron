module Citron.NDecls:NStructVarDecl;

import <cassert>;
import Citron.Exceptions;
import :NStructDecl;

using namespace std;

namespace Citron {

NStructVarDecl::NStructVarDecl(std::weak_ptr<NStructDecl> _struct, RAccessor accessor, bool bStatic, std::string name)
    : _struct(move(_struct))
    , accessor(accessor)
    , bStatic(bStatic)
    , name(move(name))
{
}

void NStructVarDecl::InitDeclType(const RTypePtr& declType)
{
    this->declType = declType;
}

RTypePtr NStructVarDecl::GetUnboundDeclType()
{
    assert(declType);
    return declType;
}

NDecl* NStructVarDecl::GetNOuter()
{
    return _struct.lock().get();
}

RDecl* NStructVarDecl::GetROuter()
{
    return _struct.lock().get();
}

RIdentifier NStructVarDecl::GetIdentifier()
{
    return RIdentifier { RName_Normal(name), 0, {} };
}

optional<RMember> NStructVarDecl::GetMember(const RTypeArgumentsPtr& typeArgs, const RName& name, size_t explicitTypeParamsExceptOuterCount)
{
    return nullopt;
}

optional<Citron::RMember> NStructVarDecl::ResolveIdentifier(const RName& name, size_t explicitTypeParamsExceptOuterCount, RTypeFactory& factory)
{
    // VarDecl의 자식이 ResolveIdentifier를 호출할 수 없고, bodyspace도 아니기 때문에 직접 호출할 일이 없다
    throw RuntimeFatalException();
}

RTypePtr NStructVarDecl::GetDeclType(RTypeArguments& typeArgs, RTypeFactory& factory)
{
    assert(declType != nullptr);
    return declType->Apply(typeArgs, factory);
}


} // namespace Citron