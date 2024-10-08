#include "RMember.h"

namespace Citron {

RMember_Namespace::RMember_Namespace(const std::shared_ptr<RNamespaceDecl>& decl)
    : decl(decl)
{
}

RMember_GlobalFuncs::RMember_GlobalFuncs(std::vector<RDeclWithOuterTypeArgs<RGlobalFuncDecl>>&& items)
    : items(std::move(items))
{
}

RMember_Class::RMember_Class(const RTypeArgumentsPtr& outerTypeArgs, const std::shared_ptr<RClassDecl>& decl)
    : outerTypeArgs(outerTypeArgs), decl(decl)
{

}

RMember_ClassMemberFuncs::RMember_ClassMemberFuncs(std::vector<RDeclWithOuterTypeArgs<RClassMemberFuncDecl>>&& items)
    : items(std::move(items))
{

}

RMember_ClassMemberVar::RMember_ClassMemberVar(const std::shared_ptr<RClassMemberVarDecl>& decl, const RTypeArgumentsPtr& typeArgs)
    : decl(decl), typeArgs(typeArgs)
{

}

RMember_Struct::RMember_Struct(const RTypeArgumentsPtr& outerTypeArgs, const std::shared_ptr<RStructDecl>& decl)
    : outerTypeArgs(outerTypeArgs), decl(decl)
{

}

RMember_StructMemberFuncs::RMember_StructMemberFuncs(std::vector<RDeclWithOuterTypeArgs<RStructMemberFuncDecl>>&& items)
    : items(std::move(items))
{

}

RMember_StructMemberVar::RMember_StructMemberVar(const std::shared_ptr<RStructMemberVarDecl>& decl, const RTypeArgumentsPtr& typeArgs)
    : decl(decl), typeArgs(typeArgs)
{

}

RMember_Enum::RMember_Enum(const RTypeArgumentsPtr& outerTypeArgs, const std::shared_ptr<REnumDecl>& decl)
    : outerTypeArgs(outerTypeArgs), decl(decl)
{

}

RMember_EnumElem::RMember_EnumElem(const std::shared_ptr<REnumElemDecl>& decl, const RTypeArgumentsPtr& typeArgs)
    : decl(decl), typeArgs(typeArgs)
{

}

RMember_EnumElemMemberVar::RMember_EnumElemMemberVar(const std::shared_ptr<REnumElemMemberVarDecl>& decl, const RTypeArgumentsPtr& typeArgs)
    : decl(decl), typeArgs(typeArgs)
{

}

RMember_LambdaMemberVar::RMember_LambdaMemberVar(const std::shared_ptr<RLambdaMemberVarDecl>& decl, const RTypeArgumentsPtr& typeArgs)
    : decl(decl), typeArgs(typeArgs)
{

}

RMember_TupleMemberVar::RMember_TupleMemberVar()
{

}

} // namespace Citron::SyntaxIR0Translator
