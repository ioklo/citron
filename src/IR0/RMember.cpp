#include "RMember.h"
#include "NGlobalFuncDecl.h"
#include "NClassMemberFuncDecl.h"
#include "NStructMemberFuncDecl.h"

using namespace std;

namespace Citron {

RMember_Namespace::RMember_Namespace(const shared_ptr<NNamespaceDecl>& decl)
    : decl(decl)
{
}

RMember_GlobalFuncs::RMember_GlobalFuncs(vector<RDeclWithOuterTypeArgs<NGlobalFuncDecl>>&& items)
    : items(std::move(items))
{
}

vector<RDeclWithOuterTypeArgs<RFuncDecl>> RMember_GlobalFuncs::GetFuncDeclWithOuterTypeArgs()
{
    vector<RDeclWithOuterTypeArgs<RFuncDecl>> result;
    result.reserve(items.size());

    for(auto& item : items)
        result.emplace_back(item.decl, item.outerTypeArgs);

    return result;
}

RMember_Class::RMember_Class(const RTypeArgumentsPtr& outerTypeArgs, const shared_ptr<NClassDecl>& decl)
    : outerTypeArgs(outerTypeArgs), decl(decl)
{

}

RMember_ClassMemberFuncs::RMember_ClassMemberFuncs(vector<RDeclWithOuterTypeArgs<NClassMemberFuncDecl>>&& items)
    : items(std::move(items))
{

}

vector<RDeclWithOuterTypeArgs<RFuncDecl>> RMember_ClassMemberFuncs::GetFuncDeclWithOuterTypeArgs()
{
    vector<RDeclWithOuterTypeArgs<RFuncDecl>> result;
    result.reserve(items.size());

    for (auto& item : items)
        result.emplace_back(item.decl, item.outerTypeArgs);

    return result;
}

RMember_ClassMemberVar::RMember_ClassMemberVar(const shared_ptr<NClassMemberVarDecl>& decl, const RTypeArgumentsPtr& typeArgs)
    : decl(decl), typeArgs(typeArgs)
{

}

RMember_Struct::RMember_Struct(const RTypeArgumentsPtr& outerTypeArgs, const shared_ptr<NStructDecl>& decl)
    : outerTypeArgs(outerTypeArgs), decl(decl)
{

}

RMember_StructMemberFuncs::RMember_StructMemberFuncs(vector<RDeclWithOuterTypeArgs<NStructMemberFuncDecl>>&& items)
    : items(std::move(items))
{

}

vector<RDeclWithOuterTypeArgs<RFuncDecl>> RMember_StructMemberFuncs::GetFuncDeclWithOuterTypeArgs()
{
    vector<RDeclWithOuterTypeArgs<RFuncDecl>> result;
    result.reserve(items.size());

    for (auto& item : items)
        result.emplace_back(item.decl, item.outerTypeArgs);

    return result;
}

RMember_StructMemberVar::RMember_StructMemberVar(const shared_ptr<NStructMemberVarDecl>& decl, const RTypeArgumentsPtr& typeArgs)
    : decl(decl), typeArgs(typeArgs)
{

}

RMember_Enum::RMember_Enum(const RTypeArgumentsPtr& outerTypeArgs, const shared_ptr<NEnumDecl>& decl)
    : outerTypeArgs(outerTypeArgs), decl(decl)
{

}

RMember_EnumElem::RMember_EnumElem(const shared_ptr<NEnumElemDecl>& decl, const RTypeArgumentsPtr& typeArgs)
    : decl(decl), typeArgs(typeArgs)
{

}

RMember_EnumElemMemberVar::RMember_EnumElemMemberVar(const shared_ptr<NEnumElemMemberVarDecl>& decl, const RTypeArgumentsPtr& typeArgs)
    : decl(decl), typeArgs(typeArgs)
{

}

RMember_LambdaMemberVar::RMember_LambdaMemberVar(const shared_ptr<NLambdaMemberVarDecl>& decl, const RTypeArgumentsPtr& typeArgs)
    : decl(decl), typeArgs(typeArgs)
{

}

RMember_TupleMemberVar::RMember_TupleMemberVar()
{

}

} // namespace Citron::SyntaxIR0Translator
