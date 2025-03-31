export module Citron.RDecls:RMember;

import "IR0Config.h";

import <memory>;
import <vector>;
import <variant>;
import <string>;

namespace Citron {

export class RTypeArguments;
export using RTypeArgumentsPtr = std::shared_ptr<RTypeArguments>;

export class RType;
export using RTypePtr = std::shared_ptr<RType>;

export template<typename TDecl>
struct DeclWithOuterTypeArgs;

export class RNamespaceDecl;
export class RGlobalFuncDecl;
export class RClassDecl;
export class RClassFuncDecl;
export class RClassVarDecl;
export class RStructDecl;
export class RStructFuncDecl;
export class RStructVarDecl;
export class REnumDecl;
export class REnumElemDecl;
export class REnumElemVarDecl;
export class RLambdaVarDecl;
export class RFuncDecl;

export class RMember_Namespace
{
public:
    std::shared_ptr<RNamespaceDecl> decl;
public:
    RMember_Namespace(const std::shared_ptr<RNamespaceDecl>& decl);
};

export class RMember_GlobalFuncs
{
public:
    std::vector<DeclWithOuterTypeArgs<RGlobalFuncDecl>> items;

public:
    RMember_GlobalFuncs(std::vector<DeclWithOuterTypeArgs<RGlobalFuncDecl>>&& items);
    RMember_GlobalFuncs(const RMember_GlobalFuncs&);
    ~RMember_GlobalFuncs();
};

export class RMember_Class
{
public:
    RTypeArgumentsPtr outerTypeArgs;
    std::shared_ptr<RClassDecl> decl;
public:
    RMember_Class(const RTypeArgumentsPtr& outerTypeArgs, const std::shared_ptr<RClassDecl>& decl);
};

export class RMember_ClassFuncs
{
public:
    std::vector<DeclWithOuterTypeArgs<RClassFuncDecl>> items;
public:
    RMember_ClassFuncs(std::vector<DeclWithOuterTypeArgs<RClassFuncDecl>>&& items);
    RMember_ClassFuncs(const RMember_ClassFuncs&);
    ~RMember_ClassFuncs();
};

export class RMember_ClassVar
{
public:
    std::shared_ptr<RClassVarDecl> decl;
    RTypeArgumentsPtr typeArgs;

public:
    RMember_ClassVar(const std::shared_ptr<RClassVarDecl>& decl, const RTypeArgumentsPtr& typeArgs);
};

export class RMember_Struct
{
public:
    RTypeArgumentsPtr outerTypeArgs;
    std::shared_ptr<RStructDecl> decl;

public:
    RMember_Struct(const RTypeArgumentsPtr& outerTypeArgs, const std::shared_ptr<RStructDecl>& decl);
};

export class RMember_StructFuncs
{
public:
    std::vector<DeclWithOuterTypeArgs<RStructFuncDecl>> items;

public:
    RMember_StructFuncs(std::vector<DeclWithOuterTypeArgs<RStructFuncDecl>>&& items);
    RMember_StructFuncs(const RMember_StructFuncs&);
    ~RMember_StructFuncs();
};

export class RMember_StructVar
{
public:
    std::shared_ptr<RStructVarDecl> decl;
    RTypeArgumentsPtr typeArgs;

public:
    RMember_StructVar(const std::shared_ptr<RStructVarDecl>& decl, const RTypeArgumentsPtr& typeArgs);
};

export class RMember_Enum
{
public:
    RTypeArgumentsPtr outerTypeArgs;
    std::shared_ptr<REnumDecl> decl;

public:
    RMember_Enum(const RTypeArgumentsPtr& outerTypeArgs, const std::shared_ptr<REnumDecl>& decl);
};

export class RMember_EnumElem
{
public:
    RTypeArgumentsPtr outerTypeArgs;
    std::shared_ptr<REnumElemDecl> decl;

public:
    RMember_EnumElem(const RTypeArgumentsPtr& outerTypeArgs, const std::shared_ptr<REnumElemDecl>& decl);
};

export class RMember_EnumElemVar
{
public:
    RTypeArgumentsPtr outerTypeArgs;
    std::shared_ptr<REnumElemVarDecl> decl;

public:
    RMember_EnumElemVar(const RTypeArgumentsPtr& outerTypeArgs, const std::shared_ptr<REnumElemVarDecl>& decl);
};

export class RMember_LambdaVar
{
public:
    RTypeArgumentsPtr outerTypeArgs;
    std::shared_ptr<RLambdaVarDecl> decl;

public:
    RMember_LambdaVar(RTypeArgumentsPtr&& outerTypeArgs, std::shared_ptr<RLambdaVarDecl>&& decl);
    RMember_LambdaVar(const RTypeArgumentsPtr& outerTypeArgs, const std::shared_ptr<RLambdaVarDecl>& decl);
};

// 어떻게 쓰일지 몰라서, 실제로 만들때 채워넣는다
export class RMember_TupleVar
{
public:
    RMember_TupleVar();
};

export class RMember_TypeVar
{
public:
    size_t index;

public:
    RMember_TypeVar(size_t index);
};

export class RMember_LocalVar
{
public:
    RTypePtr type;
    std::string name;
public:
    RMember_LocalVar(const RTypePtr& type, const std::string& name);
};

export class RMember_ThisVar
{
public:
    RTypePtr type;

public:
    RMember_ThisVar(const RTypePtr& type);
};

export using RMember = std::variant<
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

export IR0_API std::vector<DeclWithOuterTypeArgs<RFuncDecl>> GetFuncDeclWithOuterTypeArgs(RMember& member);

} // namespace Citron

