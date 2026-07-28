#include "RDecl.h"

#include "Infra/Exceptions.h"
#include "Infra/Unreachable.h"

#include "RFactory.h"
#include "RTypeArguments.h"
#include "RMember.h"
#include "RDeclKey.h"

using namespace std;

namespace Citron {

bool RDecl::IsDescendantOf(RDecl* container)
{
    auto* outer = GetOuter();
    if (outer == nullptr) return false;

    if (outer == container) return true;
    return outer->IsDescendantOf(container);
}

void MakeOpenTypeArgsCore(RDecl* decl, vector<RType*>& typeArgItems, size_t totalSize, RFactory& rFactory)
{
    size_t typeParamCount = decl->GetTypeParamCount();

    auto* outer = decl->GetOuter();
    if (!outer)
        typeArgItems.reserve(totalSize); // base case
    else
        MakeOpenTypeArgsCore(outer, typeArgItems, totalSize + typeParamCount, rFactory);

    for (size_t i = 0; i < typeParamCount; i++)
    {
        auto* typeParam = decl->GetTypeParam(i);
        auto* typeVar = rFactory.MakeTypeVarType(typeParam);
        typeArgItems.push_back(typeVar);
    }
}

RTypeArguments* RDecl::MakeOpenTypeArgs(RFactory& factory)
{
    vector<RType*> typeArgItems;
    MakeOpenTypeArgsCore(this, typeArgItems, 0, factory);
    return factory.MakeTypeArguments(typeArgItems);
}

size_t RDecl::GetAllTypeParamCount()
{
    auto* outer = GetOuter();
    if (!outer) return GetTypeParamCount();

    return outer->GetAllTypeParamCount() + GetTypeParamCount();
}

RIdentifier RDecl::GetIdentifier()
{
    std::string buffer;
    FillIdentifier(buffer);
    return RIdentifier{std::move(buffer)};
}

RTypeArguments* GetOuterTypeArgs(RDecl* decl, RTypeArguments* typeArgs)
{
    return typeArgs->Remove(decl->GetTypeParamCount());
}

bool RDecl::CanAccess(RDecl* target)
{
    // TODO: [68] 2026-07-11, CanAccess 제대로 구현
    return true;

    //auto accessModifier = target->GetAccessor();
    //auto* targetOuter = target->GetROuter();
    //if (targetOuter == nullptr)
    //    return false;

    //switch (accessModifier)
    //{
    //case RAccessor::Public: return true;
    //case RAccessor::Protected: throw NotImplementedException();
    //case RAccessor::Private:
    //{
    //    // 같은 경우는 허용
    //    if (this == targetOuter)
    //        return true;

    //    // base클래스가 아니라 container에 속하는지를 본다
    //    return IsDescendantOf(targetOuter);
    //}

    //default: unreachable();
    //}
}

bool RDecl::FillIdentifier(std::string& buffer)
{
    // outer 먼저
    auto* outer = GetOuter();
    assert(outer); // RModule만 outer가 nullptr일 수 있다

    // outer 먼저
    if (outer->FillIdentifier(buffer))
        buffer.append("::");
    else 
        buffer.append(".");

    auto& key = GetDeclKey();
    buffer.append(key.GetValue());
    return false;
}

}