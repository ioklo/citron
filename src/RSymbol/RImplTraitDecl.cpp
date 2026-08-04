#include "RImplTraitDecl.h"
#include "RImplTraitMemberDecl.h"
#include "RImplTraitFuncDecl.h"
#include "RMember.h"
#include "RNames.h"

using namespace std;

namespace Citron {

// struct S<T, U> : Trait { ... }
// impl S<T, U> : Trait { ... } 에서 <T, U>는 type parameter이다.
// 즉 impl<T, U> S<T, U> : Trait { ... } 란 뜻이다.
// 따라서 impl S<X, Y> : Trait 라고 해도 가능하다.
RImplTraitDecl::RImplTraitDecl(RDecl* target)
    : target{target}
{
}

void RImplTraitDecl::Init(RDeclKey&& key, std::vector<RTypeParam*>&& typeParams, RAppliedDecl<RTraitDecl> appliedTraitDecl)
{
    o_lazyInit.emplace(move(key), move(appliedTraitDecl));

    assert(target->GetTypeParamCount() == typeParams.size());
    genericsComp.InitTypeParams(move(typeParams));
}

void RImplTraitDecl::AddMember(RImplTraitMemberDecl&& decl)
{
    members.push_back(move(decl));
}

RDeclKey& RImplTraitDecl::GetDeclKey()
{
    assert(o_lazyInit);
    return o_lazyInit->key;
}

// from RDecl
RDecl* RImplTraitDecl::GetOuter()
{
    return target->GetOuter(); // target과 outer가 같다
}

// 이름으로 검색할 수 없다
RName* RImplTraitDecl::TryGetName()
{
    return nullptr;
}

// impl<> 
size_t RImplTraitDecl::GetTypeParamCount()
{
    return genericsComp.GetTypeParamCount();
}

RTypeParam* RImplTraitDecl::GetTypeParam(size_t index)
{
    return genericsComp.GetTypeParam(index);
}

RTypeParam* RImplTraitDecl::GetTypeParam(InRef<RName> name)
{
    return genericsComp.GetTypeParam(name);
}

RTypeDecl* RImplTraitDecl::GetTypeMember(InRef<RName> name)
{
    return nullptr;
}

optional<RMember> RImplTraitDecl::GetMember(InRef<RName> name)
{
    // RImplTrait의 멤버를 사용자가 name으로 lookup할 일이 없다
    return nullopt;
}

} // namespace Citron