#include "RMember.h"
#include "RGlobalFuncDecl.h"
#include "RClassMemberFuncDecl.h"
#include "RStructMemberFuncDecl.h"

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

vector<DeclWithOuterTypeArgs<RFuncDecl>> RMember_GlobalFuncs::GetFuncDeclWithOuterTypeArgs()
{
    vector<DeclWithOuterTypeArgs<RFuncDecl>> result;
    result.reserve(items.size());

    for(auto& item : items)
        result.emplace_back(item.decl, item.outerTypeArgs);

    return result;
}

RMember_Class::RMember_Class(const RTypeArgumentsPtr& outerTypeArgs, const shared_ptr<RClassDecl>& decl)
    : outerTypeArgs(outerTypeArgs), decl(decl)
{

}

RMember_ClassMemberFuncs::RMember_ClassMemberFuncs(vector<DeclWithOuterTypeArgs<RClassMemberFuncDecl>>&& items)
    : items(std::move(items))
{

}

vector<DeclWithOuterTypeArgs<RFuncDecl>> RMember_ClassMemberFuncs::GetFuncDeclWithOuterTypeArgs()
{
    vector<DeclWithOuterTypeArgs<RFuncDecl>> result;
    result.reserve(items.size());

    for (auto& item : items)
        result.emplace_back(item.decl, item.outerTypeArgs);

    return result;
}

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

vector<DeclWithOuterTypeArgs<RFuncDecl>> RMember_StructMemberFuncs::GetFuncDeclWithOuterTypeArgs()
{
    vector<DeclWithOuterTypeArgs<RFuncDecl>> result;
    result.reserve(items.size());

    for (auto& item : items)
        result.emplace_back(item.decl, item.outerTypeArgs);

    return result;
}

RMember_StructMemberVar::RMember_StructMemberVar(const shared_ptr<RStructMemberVarDecl>& decl, const RTypeArgumentsPtr& typeArgs)
    : decl(decl), typeArgs(typeArgs)
{

}

RMember_Enum::RMember_Enum(const RTypeArgumentsPtr& outerTypeArgs, const shared_ptr<REnumDecl>& decl)
    : outerTypeArgs(outerTypeArgs), decl(decl)
{

}

RMember_EnumElem::RMember_EnumElem(const shared_ptr<REnumElemDecl>& decl, const RTypeArgumentsPtr& typeArgs)
    : decl(decl), typeArgs(typeArgs)
{

}

RMember_EnumElemMemberVar::RMember_EnumElemMemberVar(const shared_ptr<REnumElemMemberVarDecl>& decl, const RTypeArgumentsPtr& typeArgs)
    : decl(decl), typeArgs(typeArgs)
{

}

RMember_LambdaMemberVar::RMember_LambdaMemberVar(const shared_ptr<RLambdaMemberVarDecl>& decl, const RTypeArgumentsPtr& typeArgs)
    : decl(decl), typeArgs(typeArgs)
{

}

RMember_TupleMemberVar::RMember_TupleMemberVar()
{

}

} // namespace Citron::SyntaxIR0Translator
