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
class RClassVarDecl;
class RStructDecl;
class RStructFuncDecl;
class RStructVarDecl;
class REnumDecl;
class REnumElemDecl;
class REnumElemVarDecl;
class NLambdaVarDecl;

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

class RMember_ClassVar 
{   
public:
    std::shared_ptr<RClassVarDecl> decl;
    RTypeArgumentsPtr typeArgs;

public:
    RMember_ClassVar(const std::shared_ptr<RClassVarDecl>& decl, const RTypeArgumentsPtr& typeArgs);
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

class RMember_StructVar 
{
public:
    std::shared_ptr<RStructVarDecl> decl;
    RTypeArgumentsPtr typeArgs;

public:
    RMember_StructVar(const std::shared_ptr<RStructVarDecl>& decl, const RTypeArgumentsPtr& typeArgs);
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

class RMember_EnumElemVar 
{
public:
    RTypeArgumentsPtr outerTypeArgs;
    std::shared_ptr<REnumElemVarDecl> decl;

public:
    RMember_EnumElemVar(const RTypeArgumentsPtr& outerTypeArgs, const std::shared_ptr<REnumElemVarDecl>& decl);
};

class RMember_LambdaVar 
{
public:
    RTypeArgumentsPtr outerTypeArgs;
    std::shared_ptr<NLambdaVarDecl> decl;

public:
    RMember_LambdaVar(RTypeArgumentsPtr&& outerTypeArgs, std::shared_ptr<NLambdaVarDecl>&& decl);
    RMember_LambdaVar(const RTypeArgumentsPtr& outerTypeArgs, const std::shared_ptr<NLambdaVarDecl>& decl);
};

// 어떻게 쓰일지 몰라서, 실제로 만들때 채워넣는다
class RMember_TupleVar 
{
public:
    RMember_TupleVar();
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
    RMember_ClassVar,
    RMember_Struct,
    RMember_StructFuncs,
    RMember_StructVar,
    RMember_Enum,
    RMember_EnumElem,
    RMember_EnumElemVar,
    RMember_LambdaVar,
    RMember_TupleVar,
    RMember_TypeVar,

    RMember_LocalVar,
    RMember_ThisVar
>;

IR0_API std::vector<DeclWithOuterTypeArgs<RFuncDecl>> GetFuncDeclWithOuterTypeArgs(RMember& member);

} // namespace Citron

