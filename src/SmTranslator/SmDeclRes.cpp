#include "SmDeclRes.h"
#include "Infra/Exceptions.h"
#include "RSymbol/RMember.h"

using namespace std;

namespace Citron {

SmDeclRes ToSmDeclRes(RTypeArguments* outerTypeArgs, RMember member)
{
    struct Visitor
    {
        RTypeArguments* outerTypeArgs;

        SmDeclRes operator()(RMember_Namespace& member) { throw NotImplementedException{}; }
        SmDeclRes operator()(RMember_GlobalFuncs& member) { return SmDeclRes_GlobalFuncs{{outerTypeArgs, member.items}}; }
        SmDeclRes operator()(RMember_Class& member) { return SmDeclRes_Class{outerTypeArgs, member.decl}; }
        SmDeclRes operator()(RMember_ClassFuncs& member) { return SmDeclRes_ClassFuncs{outerTypeArgs, member.items}; }
        SmDeclRes operator()(RMember_ClassVar& member) { return SmDeclRes_ClassVar{member.decl, outerTypeArgs}; }
        SmDeclRes operator()(RMember_Struct& member) { return SmDeclRes_Struct{outerTypeArgs, member.decl}; }
        SmDeclRes operator()(RMember_StructFuncs& member) { return SmDeclRes_StructFuncs{outerTypeArgs, member.items}; }
        SmDeclRes operator()(RMember_StructVar& member) { return SmDeclRes_StructVar{member.decl, outerTypeArgs}; }
        SmDeclRes operator()(RMember_Enum& member) { return SmDeclRes_Enum{outerTypeArgs, member.decl}; }
        SmDeclRes operator()(RMember_EnumElem& member) { return SmDeclRes_EnumElem{outerTypeArgs, member.decl}; }
        SmDeclRes operator()(RMember_EnumElemVar& member) { return SmDeclRes_EnumElemVar{outerTypeArgs, member.decl}; }
        SmDeclRes operator()(RMember_Interface& member) { return SmDeclRes_Interface{outerTypeArgs, member.decl}; }
        SmDeclRes operator()(RMember_Lambda& member) { return SmDeclRes_Lambda{outerTypeArgs, member.decl}; }
        SmDeclRes operator()(RMember_LambdaVar& member) { return SmDeclRes_LambdaVar{outerTypeArgs, member.decl}; }
        SmDeclRes operator()(RMember_TupleVar& member) { return SmDeclRes_TupleVar{}; }
        SmDeclRes operator()(RMember_Trait& member) { return SmDeclRes_Trait{outerTypeArgs, member.decl}; }
        SmDeclRes operator()(RMember_TraitFuncs& member) { return SmDeclRes_TraitFuncs{outerTypeArgs, member.items}; }
    };

    return member.Visit(Visitor{outerTypeArgs});
}

} // namespace Citron
