#pragma once

#include <memory>
#include <vector>

#include "RDeclWithOuterTypeArgs.h"

namespace Citron {

class RNamespaceDecl;
class RGlobalFuncDecl;
class RClassDecl;
class RClassMemberFuncDecl;
class RClassMemberVarDecl;
class RStructDecl;
class RStructMemberFuncDecl;
class RStructMemberVarDecl;
class REnumDecl;
class REnumElemDecl;
class REnumElemMemberVarDecl;
class RLambdaMemberVarDecl;

using RTypeArgumentsPtr = std::shared_ptr<class RTypeArguments>;

class RMember;
class RMember_Namespace;
class RMember_GlobalFuncs;
class RMember_Class;
class RMember_ClassMemberFuncs;
class RMember_ClassMemberVar;
class RMember_Struct;
class RMember_StructMemberFuncs;
class RMember_StructMemberVar;
class RMember_Enum;
class RMember_EnumElem;
class RMember_EnumElemMemberVar;
class RMember_LambdaMemberVar;
class RMember_TupleMemberVar;

class RMemberVisitor
{
public:
    virtual ~RMemberVisitor() { }
    virtual void Visit(RMember_Namespace& member) = 0;
    virtual void Visit(RMember_GlobalFuncs& member) = 0;
    virtual void Visit(RMember_Class& member) = 0;
    virtual void Visit(RMember_ClassMemberFuncs& member) = 0;
    virtual void Visit(RMember_ClassMemberVar& member) = 0;
    virtual void Visit(RMember_Struct& member) = 0;
    virtual void Visit(RMember_StructMemberFuncs& member) = 0;
    virtual void Visit(RMember_StructMemberVar& member) = 0;
    virtual void Visit(RMember_Enum& member) = 0;
    virtual void Visit(RMember_EnumElem& member) = 0;
    virtual void Visit(RMember_EnumElemMemberVar& member) = 0;
    virtual void Visit(RMember_LambdaMemberVar& member) = 0;
    virtual void Visit(RMember_TupleMemberVar& member) = 0;
};

class RMember
{
public:
    virtual ~RMember() { }
    virtual void Accept(RMemberVisitor& visitor) = 0;
};

using RMemberPtr = std::shared_ptr<RMember>;

class RMember_Namespace : public RMember 
{
public:
    std::shared_ptr<RNamespaceDecl> decl;
public:
    RMember_Namespace(const std::shared_ptr<RNamespaceDecl>& decl);

public:
    void Accept(RMemberVisitor& visitor) override { visitor.Visit(*this); }
};

class RMember_GlobalFuncs : public RMember 
{
public:
    std::vector<RDeclWithOuterTypeArgs<RGlobalFuncDecl>> items;

public:
    RMember_GlobalFuncs(std::vector<RDeclWithOuterTypeArgs<RGlobalFuncDecl>>&& items);

public:
    void Accept(RMemberVisitor& visitor) override { visitor.Visit(*this); }
};

class RMember_Class : public RMember 
{
public:
    RTypeArgumentsPtr outerTypeArgs;
    std::shared_ptr<RClassDecl> decl;
public:
    RMember_Class(const RTypeArgumentsPtr& outerTypeArgs, const std::shared_ptr<RClassDecl>& decl);
public:
    void Accept(RMemberVisitor& visitor) override { visitor.Visit(*this); }
};

class RMember_ClassMemberFuncs : public RMember 
{
public:
    std::vector<RDeclWithOuterTypeArgs<RClassMemberFuncDecl>> items;
public:
    RMember_ClassMemberFuncs(std::vector<RDeclWithOuterTypeArgs<RClassMemberFuncDecl>>&& items);
public:
    void Accept(RMemberVisitor& visitor) override { visitor.Visit(*this); }
};

class RMember_ClassMemberVar : public RMember 
{   
public:
    std::shared_ptr<RClassMemberVarDecl> decl;
    RTypeArgumentsPtr typeArgs;

public:
    RMember_ClassMemberVar(const std::shared_ptr<RClassMemberVarDecl>& decl, const RTypeArgumentsPtr& typeArgs);

public:
    void Accept(RMemberVisitor& visitor) override { visitor.Visit(*this); }
};

class RMember_Struct : public RMember 
{
public:
    RTypeArgumentsPtr outerTypeArgs;
    std::shared_ptr<RStructDecl> decl;

public:
    RMember_Struct(const RTypeArgumentsPtr& outerTypeArgs, const std::shared_ptr<RStructDecl>& decl);

public:
    void Accept(RMemberVisitor& visitor) override { visitor.Visit(*this); }
};

class RMember_StructMemberFuncs : public RMember 
{
public:
    std::vector<RDeclWithOuterTypeArgs<RStructMemberFuncDecl>> items;

public:
    RMember_StructMemberFuncs(std::vector<RDeclWithOuterTypeArgs<RStructMemberFuncDecl>>&& items);

public:
    void Accept(RMemberVisitor& visitor) override { visitor.Visit(*this); }
};

class RMember_StructMemberVar : public RMember 
{
public:
    std::shared_ptr<RStructMemberVarDecl> decl;
    RTypeArgumentsPtr typeArgs;

public:
    RMember_StructMemberVar(const std::shared_ptr<RStructMemberVarDecl>& decl, const RTypeArgumentsPtr& typeArgs);

public:
    void Accept(RMemberVisitor& visitor) override { visitor.Visit(*this); }
};

class RMember_Enum : public RMember 
{
public:
    RTypeArgumentsPtr outerTypeArgs;
    std::shared_ptr<REnumDecl> decl;

public:
    RMember_Enum(const RTypeArgumentsPtr& outerTypeArgs, const std::shared_ptr<REnumDecl>& decl);

public:
    void Accept(RMemberVisitor& visitor) override { visitor.Visit(*this); }
};

class RMember_EnumElem : public RMember 
{
public:
    std::shared_ptr<REnumElemDecl> decl;
    RTypeArgumentsPtr typeArgs;

public:
    RMember_EnumElem(const std::shared_ptr<REnumElemDecl>& decl, const RTypeArgumentsPtr& typeArgs);

public:
    void Accept(RMemberVisitor& visitor) override { visitor.Visit(*this); }
};

class RMember_EnumElemMemberVar : public RMember 
{
public:
    std::shared_ptr<REnumElemMemberVarDecl> decl;
    RTypeArgumentsPtr typeArgs;

public:
    RMember_EnumElemMemberVar(const std::shared_ptr<REnumElemMemberVarDecl>& decl, const RTypeArgumentsPtr& typeArgs);

public:
    void Accept(RMemberVisitor& visitor) override { visitor.Visit(*this); }
};

class RMember_LambdaMemberVar : public RMember 
{
public:
    std::shared_ptr<RLambdaMemberVarDecl> decl;
    RTypeArgumentsPtr typeArgs;

public:
    RMember_LambdaMemberVar(const std::shared_ptr<RLambdaMemberVarDecl>& decl, const RTypeArgumentsPtr& typeArgs);

public:
    void Accept(RMemberVisitor& visitor) override { visitor.Visit(*this); }
};

// 어떻게 쓰일지 몰라서, 실제로 만들때 채워넣는다
class RMember_TupleMemberVar : public RMember 
{
public:
    RMember_TupleMemberVar();

public:
    void Accept(RMemberVisitor& visitor) override { visitor.Visit(*this); }
};

} // namespace Citron

