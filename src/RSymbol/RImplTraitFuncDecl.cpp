#include "RImplTraitFuncDecl.h"
#include "RImplTraitDecl.h"

using namespace std;

namespace Citron {

RDecl* RImplTraitFuncDecl::GetOuter()
{
    return implTrait;
}

RIdentifier RImplTraitFuncDecl::GetIdentifier()
{
    
}

size_t RImplTraitFuncDecl::GetTypeParamCount()
{
    return genericsComp.GetTypeParamCount();
}

RTypeParam* RImplTraitFuncDecl::GetTypeParam(size_t index)
{
    return genericsComp.GetTypeParam(index);
}

RTypeParam* RImplTraitFuncDecl::GetTypeParam(InRef<RName> name)
{
    return genericsComp.GetTypeParam(name);
}

RTypeDecl* RImplTraitFuncDecl::GetTypeMember(InRef<RName> name)
{
    return nullptr;
}

optional<RMember> RImplTraitFuncDecl::GetMember(InRef<RName> name)
{
    return nullopt;
}

} // namespace Citron