#include "RClassCtorDecl.h"
#include "RClassDecl.h"

using namespace std;

namespace Citron {

RClassCtorDecl::RClassCtorDecl(RDeclKey&& key, RClassDecl* _class, RClassMemberAccessor accessor, bool bTrivial)
    : key{std::move(key)}
    , _class{_class}
    , accessor{accessor}
    , bTrivial{bTrivial}
    , genericsComp{}
    , commonFuncDeclComp{/*bSeqFunc*/false}
    , ImplRFuncDeclUsingCommonComponents{this, commonFuncDeclComp}
{
}

void RClassCtorDecl::InitTypeParams(vector<RTypeParam*>&& typeParams)
{
    return genericsComp.InitTypeParams(move(typeParams));
}

// from RDecl
RDeclKey& RClassCtorDecl::GetDeclKey()
{
    return key;
}

RDecl* RClassCtorDecl::GetOuter()
{
    return _class;
}

RName* RClassCtorDecl::TryGetName()
{
    return nullptr;
}

size_t RClassCtorDecl::GetTypeParamCount()
{
    return genericsComp.GetTypeParamCount();
}

RTypeParam* RClassCtorDecl::GetTypeParam(size_t index)
{
    return genericsComp.GetTypeParam(index);
}

RTypeParam* RClassCtorDecl::GetTypeParam(InRef<RName> name)
{
    return genericsComp.GetTypeParam(name);
}

RTypeDecl* RClassCtorDecl::GetTypeMember(InRef<RName> name)
{
    return nullptr;
}

optional<RMember> RClassCtorDecl::GetMember(InRef<RName> name)
{
    return nullopt;
}

} // namespace Citron