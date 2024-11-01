#include "RMember.h"

#include <Infra/Variants.h>

#include "RGlobalFuncDecl.h"
#include "RClassMemberFuncDecl.h"
#include "RStructMemberFuncDecl.h"
#include "DeclWithOuterTypeArgs.h"

using namespace std;

namespace Citron {

RMember_Namespace::RMember_Namespace(const shared_ptr<RNamespaceDecl>& decl)
    : decl(decl)
{
}

RMember_GlobalFuncs::RMember_GlobalFuncs(vector<DeclWithOuterTypeArgs<RGlobalFuncDecl>>&& items)
    : items(std::move(items))
{
}

RMember_GlobalFuncs::RMember_GlobalFuncs(const RMember_GlobalFuncs&) = default;

RMember_GlobalFuncs::~RMember_GlobalFuncs() = default;

RMember_Class::RMember_Class(const RTypeArgumentsPtr& outerTypeArgs, const shared_ptr<RClassDecl>& decl)
    : outerTypeArgs(outerTypeArgs), decl(decl)
{

}

RMember_ClassMemberFuncs::RMember_ClassMemberFuncs(vector<DeclWithOuterTypeArgs<RClassMemberFuncDecl>>&& items)
    : items(std::move(items))
{

}

RMember_ClassMemberFuncs::RMember_ClassMemberFuncs(const RMember_ClassMemberFuncs&) = default;

RMember_ClassMemberFuncs::~RMember_ClassMemberFuncs() = default;

RMember_ClassMemberVar::RMember_ClassMemberVar(const shared_ptr<RClassMemberVarDecl>& decl, const RTypeArgumentsPtr& typeArgs)
    : decl(decl), typeArgs(typeArgs)
{

}

RMember_Struct::RMember_Struct(const RTypeArgumentsPtr& outerTypeArgs, const shared_ptr<RStructDecl>& decl)
    : outerTypeArgs(outerTypeArgs), decl(decl)
{

}

RMember_StructMemberFuncs::RMember_StructMemberFuncs(vector<DeclWithOuterTypeArgs<RStructMemberFuncDecl>>&& items)
    : items(std::move(items))
{

}

RMember_StructMemberFuncs::RMember_StructMemberFuncs(const RMember_StructMemberFuncs&) = default;

RMember_StructMemberFuncs::~RMember_StructMemberFuncs() = default;

RMember_StructMemberVar::RMember_StructMemberVar(const shared_ptr<RStructMemberVarDecl>& decl, const RTypeArgumentsPtr& typeArgs)
    : decl(decl), typeArgs(typeArgs)
{

}

RMember_Enum::RMember_Enum(const RTypeArgumentsPtr& outerTypeArgs, const shared_ptr<REnumDecl>& decl)
    : outerTypeArgs(outerTypeArgs), decl(decl)
{

}

RMember_EnumElem::RMember_EnumElem(const RTypeArgumentsPtr& outerTypeArgs, const shared_ptr<REnumElemDecl>& decl)
    : outerTypeArgs(outerTypeArgs), decl(decl)
{

}

RMember_EnumElemMemberVar::RMember_EnumElemMemberVar(const RTypeArgumentsPtr& outerTypeArgs, const shared_ptr<REnumElemMemberVarDecl>& decl)
    : outerTypeArgs(outerTypeArgs), decl(decl)
{

}

RMember_LambdaMemberVar::RMember_LambdaMemberVar(const RTypeArgumentsPtr& outerTypeArgs, const shared_ptr<RLambdaMemberVarDecl>& decl)
    : outerTypeArgs(outerTypeArgs), decl(decl)
{

}

RMember_TupleMemberVar::RMember_TupleMemberVar()
{

}

template<typename TRFuncDecl>
constexpr vector<DeclWithOuterTypeArgs<RFuncDecl>> GetItems(vector<DeclWithOuterTypeArgs<TRFuncDecl>>& items)
{
    vector<DeclWithOuterTypeArgs<RFuncDecl>> result;
    result.reserve(items.size());

    for (auto& item : items)
        result.emplace_back(item.decl, item.outerTypeArgs);

    return result;
}

vector<DeclWithOuterTypeArgs<RFuncDecl>> GetFuncDeclWithOuterTypeArgs(RMember& member)
{
    return visit(overloaded {
        [](RMember_GlobalFuncs& member) { return GetItems(member.items); },
        [](RMember_ClassMemberFuncs& member) { return GetItems(member.items); },
        [](RMember_StructMemberFuncs& member) { return GetItems(member.items); },
        [](auto&&) { return vector<DeclWithOuterTypeArgs<RFuncDecl>>{}; },
    }, member);
}


} // namespace Citron::SyntaxIR0Translator
