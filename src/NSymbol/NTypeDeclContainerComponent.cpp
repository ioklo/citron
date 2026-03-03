#include "NTypeDeclContainerComponent.h"

#include <optional>

#include "RSymbol/RDecl.h"
#include "NTypeDecl.h"

using namespace std;

namespace Citron {

//TypeDeclSymbolComponent::TypeDeclSymbolComponent()
//{
//
//}

/*public IEnumerable<ITypeDeclSymbol> GetEnumerable()
{
    return typeDict.Values;
}*/

NTypeDeclContainerComponent::NTypeDeclContainerComponent() = default;

size_t NTypeDeclContainerComponent::GetTypeCount()
{
    return types.size();
}

NTypeDecl* NTypeDeclContainerComponent::GetType(int index)
{
    return types[index];
}

NTypeDecl* NTypeDeclContainerComponent::GetType(const RIdentifier& identifier)
{
    auto i = typeDict.find(identifier);
    if (i == typeDict.end()) return nullptr;

    return i->second;
}

void NTypeDeclContainerComponent::AddType(NTypeDecl* typeDecl)
{
    types.push_back(typeDecl);
    typeDict.insert_or_assign(typeDecl->GetNDecl()->GetRDecl()->GetIdentifier(), typeDecl);
}

RTypeDecl* NTypeDeclContainerComponent::GetTypeMember(const RName& name, size_t typeParamCount)
{
    auto i = typeDict.find({name, typeParamCount, {}});
    if (i == typeDict.end()) return nullptr;

    return i->second->GetRTypeDecl();
}

// 첫번째 인자는 부모의 typeArgs
optional<RDeclRes> NTypeDeclContainerComponent::GetMemberType(RTypeArguments* typeArgs, const RName& name, size_t explicitTypeParamsExceptOuterCount)
{
    auto i = typeDict.find({ name, explicitTypeParamsExceptOuterCount, {} });
    if (i == typeDict.end()) return nullopt;

    return i->second->ToRDeclRes(typeArgs);
}

//bool ICyclicEqualityComparableStruct<TypeDeclSymbolComponent>.CyclicEquals(ref TypeDeclSymbolComponent other, ref CyclicEqualityCompareContext context)
//{
//    if (!typeDict.CyclicEqualsClassValue(other.typeDict, ref context))
//        return false;
//
//    return true;
//}
//
//void ISerializable.DoSerialize(ref SerializeContext context)
//{
//    context.SerializeDictRefKeyRefValue(nameof(typeDict), typeDict);
//}


}