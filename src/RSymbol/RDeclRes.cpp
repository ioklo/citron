#include "RDeclRes.h"
#include "RMember.h"

using namespace std;

namespace Citron {

RDeclRes ToRDeclRes(RTypeArguments* outerTypeArgs, RMember member)
{
    struct Visitor
    {
        RTypeArguments* outerTypeArgs;

        RDeclRes operator()(RMember_Namespace& member) { return RDeclRes_Namespaces{member.namespaces}; }
        RDeclRes operator()(RMember_GlobalFuncs& member) { return RDeclRes_GlobalFuncs{outerTypeArgs, member.items}; }
        RDeclRes operator()(RMember_Class& member) { return RDeclRes_Class{outerTypeArgs, member.decl}; }
        RDeclRes operator()(RMember_ClassFuncs& member) { return RDeclRes_ClassFuncs{outerTypeArgs, member.items}; }
        RDeclRes operator()(RMember_ClassVar& member) { return RDeclRes_ClassVar{member.decl, outerTypeArgs}; }
        RDeclRes operator()(RMember_Struct& member) { return RDeclRes_Struct{outerTypeArgs, member.decl}; }
        RDeclRes operator()(RMember_StructFuncs& member) { return RDeclRes_StructFuncs{outerTypeArgs, member.items}; }
        RDeclRes operator()(RMember_StructVar& member) { return RDeclRes_StructVar{member.decl, outerTypeArgs}; }
        RDeclRes operator()(RMember_Enum& member) { return RDeclRes_Enum{outerTypeArgs, member.decl}; }
        RDeclRes operator()(RMember_EnumElem& member) { return RDeclRes_EnumElem{outerTypeArgs, member.decl}; }
        RDeclRes operator()(RMember_EnumElemVar& member) { return RDeclRes_EnumElemVar{outerTypeArgs, member.decl}; }
        RDeclRes operator()(RMember_Interface& member) { return RDeclRes_Interface{outerTypeArgs, member.decl}; }
        RDeclRes operator()(RMember_Lambda& member) { return RDeclRes_Lambda{outerTypeArgs, member.decl}; }
        RDeclRes operator()(RMember_LambdaVar& member) { return RDeclRes_LambdaVar{outerTypeArgs, member.decl}; }
        RDeclRes operator()(RMember_TupleVar& member) { return RDeclRes_TupleVar{}; }
        RDeclRes operator()(RMember_Trait& member) { return RDeclRes_Trait{outerTypeArgs, member.decl}; }
        RDeclRes operator()(RMember_TraitFuncs& member) { return RDeclRes_TraitFuncs{outerTypeArgs, member.items}; }
    };

    return member.Visit(Visitor{outerTypeArgs});
}

} // namespace Citron
