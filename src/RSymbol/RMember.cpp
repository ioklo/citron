#include "RMember.h"
#include "Infra/Exceptions.h"
#include "RTypeDecl.h"

using namespace std;

namespace Citron {

RMember ToRMember(RTypeDecl* typeDecl)
{
    struct Visitor
    {
        using ResultType = RMember;

        RMember Visit(RClassDecl* rTypeDecl) { return RMember_Class{rTypeDecl}; }
        RMember Visit(RStructDecl* rTypeDecl) { return RMember_Struct{rTypeDecl}; }
        RMember Visit(REnumDecl* rTypeDecl) { return RMember_Enum{rTypeDecl}; }
        RMember Visit(REnumElemDecl* rTypeDecl) { return RMember_EnumElem{rTypeDecl}; }
        RMember Visit(RInterfaceDecl* rTypeDecl) { return RMember_Interface{rTypeDecl}; }
        RMember Visit(RLambdaDecl* rTypeDecl) { return RMember_Lambda{rTypeDecl}; }
        RMember Visit(RTraitDecl* rTypeDecl) { return RMember_Trait{rTypeDecl}; }
        RMember Visit(RTypeAliasDecl* rTypeDecl) { throw NotImplementedException{}; }
    };

    return Accept(Visitor{}, typeDecl);
}

} // namespace Citron
