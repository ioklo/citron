#pragma once
#include "IR0Config.h"

#include <memory>
#include <vector>
#include <variant>
#include <string>

namespace Citron {

template<typename TDecl>
struct DeclWithOuterTypeArgs;

class RFuncDecl;
class RNamespaceDecl;
class RGlobalFuncDecl;
class RClassDecl;
class RClassFuncDecl;
class RClassMemberVarDecl;
class RStructDecl;
class RStructFuncDecl;
class RStructMemberVarDecl;
class REnumDecl;
class REnumElemDecl;
class REnumElemMemberVarDecl;
class NLambdaMemberVarDecl;

using RTypeArgumentsPtr = std::shared_ptr<class RTypeArguments>;
using RTypePtr = std::shared_ptr<class RType>;

class RMember_Namespace 
{
public:
    std::shared_ptr<RNamespaceDecl> decl;
public:
    RMember_Namespace(const std::shared_ptr<RNamespaceDecl>& decl);
};

class RMember_GlobalFuncs
{
public:
    std::vector<DeclWithOuterTypeArgs<RGlobalFuncDecl>> items;

public:
    RMember_GlobalFuncs(std::vector<DeclWithOuterTypeArgs<RGlobalFuncDecl>>&& items);
    RMember_GlobalFuncs(const RMember_GlobalFuncs&);
    ~RMember_GlobalFuncs();
};

class RMember_Class
{
public:
    RTypeArgumentsPtr outerTypeArgs;
    std::shared_ptr<RClassDecl> decl;
public:
    RMember_Class(const RTypeArgumentsPtr& outerTypeArgs, const std::shared_ptr<RClassDecl>& decl);
};

class RMember_ClassFuncs 
{
public:
    std::vector<DeclWithOuterTypeArgs<RClassFuncDecl>> items;
public:
    RMember_ClassFuncs(std::vector<DeclWithOuterTypeArgs<RClassFuncDecl>>&& items);
    RMember_ClassFuncs(const RMember_ClassFuncs&);
    ~RMember_ClassFuncs();
};

class RMember_ClassMemberVar 
{   
public:
    std::shared_ptr<RClassMemberVarDecl> decl;
    RTypeArgumentsPtr typeArgs;

public:
    RMember_ClassMemberVar(const std::shared_ptr<RClassMemberVarDecl>& decl, const RTypeArgumentsPtr& typeArgs);
};

class RMember_Struct 
{
public:
    RTypeArgumentsPtr outerTypeArgs;
    std::shared_ptr<RStructDecl> decl;

public:
    RMember_Struct(const RTypeArgumentsPtr& outerTypeArgs, const std::shared_ptr<RStructDecl>& decl);
};

class RMember_StructFuncs 
{
public:
    std::vector<DeclWithOuterTypeArgs<RStructFuncDecl>> items;

public:
    RMember_StructFuncs(std::vector<DeclWithOuterTypeArgs<RStructFuncDecl>>&& items);
    RMember_StructFuncs(const RMember_StructFuncs&);
    ~RMember_StructFuncs();
};

class RMember_StructMemberVar 
{
public:
    std::shared_ptr<RStructMemberVarDecl> decl;
    RTypeArgumentsPtr typeArgs;

public:
    RMember_StructMemberVar(const std::shared_ptr<RStructMemberVarDecl>& decl, const RTypeArgumentsPtr& typeArgs);
};

class RMember_Enum 
{
public:
    RTypeArgumentsPtr outerTypeArgs;
    std::shared_ptr<REnumDecl> decl;

public:
    RMember_Enum(const RTypeArgumentsPtr& outerTypeArgs, const std::shared_ptr<REnumDecl>& decl);
};

class RMember_EnumElem 
{
public:
    RTypeArgumentsPtr outerTypeArgs;
    std::shared_ptr<REnumElemDecl> decl;

public:
    RMember_EnumElem(const RTypeArgumentsPtr& outerTypeArgs, const std::shared_ptr<REnumElemDecl>& decl);
};

class RMember_EnumElemMemberVar 
{
public:
    RTypeArgumentsPtr outerTypeArgs;
    std::shared_ptr<REnumElemMemberVarDecl> decl;

public:
    RMember_EnumElemMemberVar(const RTypeArgumentsPtr& outerTypeArgs, const std::shared_ptr<REnumElemMemberVarDecl>& decl);
};

class RMember_LambdaMemberVar 
{
public:
    RTypeArgumentsPtr outerTypeArgs;
    std::shared_ptr<NLambdaMemberVarDecl> decl;

public:
    RMember_LambdaMemberVar(RTypeArgumentsPtr&& outerTypeArgs, std::shared_ptr<NLambdaMemberVarDecl>&& decl);
    RMember_LambdaMemberVar(const RTypeArgumentsPtr& outerTypeArgs, const std::shared_ptr<NLambdaMemberVarDecl>& decl);
};

// 어떻게 쓰일지 몰라서, 실제로 만들때 채워넣는다
class RMember_TupleMemberVar 
{
public:
    RMember_TupleMemberVar();
};

class RMember_TypeVar
{
public:
    size_t index;

public:
    RMember_TypeVar(size_t index);
};

class RMember_LocalVar
{
public:
    RTypePtr type;
    std::string name;
public:
    RMember_LocalVar(const RTypePtr& type, const std::string& name);
};

class RMember_ThisVar
{
public:
    RTypePtr type;

public:
    RMember_ThisVar(const RTypePtr& type);
};

using RMember = std::variant<
    RMember_Namespace,
    RMember_GlobalFuncs,
    RMember_Class,
    RMember_ClassFuncs,
    RMember_ClassMemberVar,
    RMember_Struct,
    RMember_StructFuncs,
    RMember_StructMemberVar,
    RMember_Enum,
    RMember_EnumElem,
    RMember_EnumElemMemberVar,
    RMember_LambdaMemberVar,
    RMember_TupleMemberVar,
    RMember_TypeVar,

    RMember_LocalVar,
    RMember_ThisVar
>;

IR0_API std::vector<DeclWithOuterTypeArgs<RFuncDecl>> GetFuncDeclWithOuterTypeArgs(RMember& member);

} // namespace Citron

