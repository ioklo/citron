#include "RImplTraitDecl.h"
#include "RImplTraitMemberDecl.h"
#include "RImplTraitFuncDecl.h"
#include "RMember.h"

using namespace std;

namespace Citron {

// struct S<T, U> : Trait { ... }
// impl S<T, U> : Trait { ... } 에서 <T, U>는 type parameter이다.
// 즉 impl<T, U> S<T, U> : Trait { ... } 란 뜻이다
// 따라서 impl S<X, Y> : Trait 라고 해도 가능하다
RImplTraitDecl::RImplTraitDecl(vector<RTypeParam*>&& typeParams, RDecl* target, RTraitDecl* trait, RTypeArguments* typeArgs)
    : target{target}, trait{trait}, typeArgs{typeArgs}
{
    genericsComp.InitTypeParams(move(typeParams));
    assert(target->GetTypeParamCount() == typeParams.size());
}

void RImplTraitDecl::AddMember(RImplTraitMemberDecl&& decl)
{
    members.push_back(move(decl));
}

// from RDecl
RDecl* RImplTraitDecl::GetOuter()
{
    return target->GetOuter(); // target과 outer가 같다
}

RIdentifier RImplTraitDecl::GetIdentifier()
{
    return RIdentifier{RName_};
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
    vector<RImplTraitFuncDecl*> implTraitFuncs;

    for (auto& member : members)
    {
        member.Visit([&name, &implTraitFuncs](auto* member) {
            using T = remove_cvref_t<decltype(member)>;

            if constexpr (same_as<T, RImplTraitFuncDecl*>)
            {
                if (member->GetIdentifier().name == *name)
                    implTraitFuncs.push_back(member);
            }
            else static_assert(false);
        });
    }

    if (!implTraitFuncs.empty())
        return RMember_ImplTraitFuncs{move(implTraitFuncs)};

    return nullopt;
}

} // namespace Citron