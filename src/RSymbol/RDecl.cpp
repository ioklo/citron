#include "RDecl.h"

#include "Infra/Exceptions.h"
#include "Infra/Unreachable.h"

#include "RFactory.h"

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

//bool RDecl::CanAccess(RDecl* target)
//{
//    auto accessModifier = target->GetAccessor();
//    auto* targetOuter = target->GetROuter();
//    if (targetOuter == nullptr)
//        return false;
//
//    switch (accessModifier)
//    {
//    case RAccessor::Public: return true;
//    case RAccessor::Protected: throw NotImplementedException();
//    case RAccessor::Private:
//    {
//        // 같은 경우는 허용
//        if (this == targetOuter)
//            return true;
//
//        // base클래스가 아니라 container에 속하는지를 본다
//        return IsDescendantOf(targetOuter);
//    }
//
//    default: unreachable();
//    }
//}

}