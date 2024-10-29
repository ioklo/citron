#pragma once

#include <memory>
#include <vector>

#include "RFuncDecl.h"
#include "RDeclWithOuterTypeArgs.h"

namespace Citron {

class NNamespaceDecl;
class NGlobalFuncDecl;
class NClassDecl;
class NClassMemberFuncDecl;
class NClassMemberVarDecl;
class NStructDecl;
class NStructMemberFuncDecl;
class NStructMemberVarDecl;
class NEnumDecl;
class NEnumElemDecl;
class NEnumElemMemberVarDecl;
class NLambdaMemberVarDecl;

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
    virtual std::vector<RDeclWithOuterTypeArgs<RFuncDecl>> GetFuncDeclWithOuterTypeArgs() = 0;
    virtual void Accept(RMemberVisitor& visitor) = 0;
};

using RMemberPtr = std::shared_ptr<RMember>;

class RMember_Namespace : public RMember 
{
public:
    std::shared_ptr<NNamespaceDecl> decl;
public:
    RMember_Namespace(const std::shared_ptr<NNamespaceDecl>& decl);

public:
    std::vector<RDeclWithOuterTypeArgs<RFuncDecl>> GetFuncDeclWithOuterTypeArgs() override { return {}; }
    void Accept(RMemberVisitor& visitor) override { visitor.Visit(*this); }
};

class RMember_GlobalFuncs : public RMember 
{
public:
    std::vector<RDeclWithOuterTypeArgs<NGlobalFuncDecl>> items;

public:
    RMember_GlobalFuncs(std::vector<RDeclWithOuterTypeArgs<NGlobalFuncDecl>>&& items);

public:
    std::vector<RDeclWithOuterTypeArgs<RFuncDecl>> GetFuncDeclWithOuterTypeArgs() override;
    void Accept(RMemberVisitor& visitor) override { visitor.Visit(*this); }
};

class RMember_Class : public RMember 
{
public:
    RTypeArgumentsPtr outerTypeArgs;
    std::shared_ptr<NClassDecl> decl;
public:
    RMember_Class(const RTypeArgumentsPtr& outerTypeArgs, const std::shared_ptr<NClassDecl>& decl);
public:
    std::vector<RDeclWithOuterTypeArgs<RFuncDecl>> GetFuncDeclWithOuterTypeArgs() override { return {}; }
    void Accept(RMemberVisitor& visitor) override { visitor.Visit(*this); }
};

class RMember_ClassMemberFuncs : public RMember 
{
public:
    std::vector<RDeclWithOuterTypeArgs<NClassMemberFuncDecl>> items;
public:
    RMember_ClassMemberFuncs(std::vector<RDeclWithOuterTypeArgs<NClassMemberFuncDecl>>&& items);
public:
    std::vector<RDeclWithOuterTypeArgs<RFuncDecl>> GetFuncDeclWithOuterTypeArgs() override;
    void Accept(RMemberVisitor& visitor) override { visitor.Visit(*this); }
};

class RMember_ClassMemberVar : public RMember 
{   
public:
    std::shared_ptr<NClassMemberVarDecl> decl;
    RTypeArgumentsPtr typeArgs;

public:
    RMember_ClassMemberVar(const std::shared_ptr<NClassMemberVarDecl>& decl, const RTypeArgumentsPtr& typeArgs);

public:
    std::vector<RDeclWithOuterTypeArgs<RFuncDecl>> GetFuncDeclWithOuterTypeArgs() override { return {}; }
    void Accept(RMemberVisitor& visitor) override { visitor.Visit(*this); }
};

class RMember_Struct : public RMember 
{
public:
    RTypeArgumentsPtr outerTypeArgs;
    std::shared_ptr<NStructDecl> decl;

public:
    RMember_Struct(const RTypeArgumentsPtr& outerTypeArgs, const std::shared_ptr<NStructDecl>& decl);

public:
    std::vector<RDeclWithOuterTypeArgs<RFuncDecl>> GetFuncDeclWithOuterTypeArgs() override { return {}; }
    void Accept(RMemberVisitor& visitor) override { visitor.Visit(*this); }
};

class RMember_StructMemberFuncs : public RMember 
{
public:
    std::vector<RDeclWithOuterTypeArgs<NStructMemberFuncDecl>> items;

public:
    RMember_StructMemberFuncs(std::vector<RDeclWithOuterTypeArgs<NStructMemberFuncDecl>>&& items);

public:
    std::vector<RDeclWithOuterTypeArgs<RFuncDecl>> GetFuncDeclWithOuterTypeArgs() override;
    void Accept(RMemberVisitor& visitor) override { visitor.Visit(*this); }
};

class RMember_StructMemberVar : public RMember 
{
public:
    std::shared_ptr<NStructMemberVarDecl> decl;
    RTypeArgumentsPtr typeArgs;

public:
    RMember_StructMemberVar(const std::shared_ptr<NStructMemberVarDecl>& decl, const RTypeArgumentsPtr& typeArgs);

public:
    std::vector<RDeclWithOuterTypeArgs<RFuncDecl>> GetFuncDeclWithOuterTypeArgs() override { return {}; }
    void Accept(RMemberVisitor& visitor) override { visitor.Visit(*this); }
};

class RMember_Enum : public RMember 
{
public:
    RTypeArgumentsPtr outerTypeArgs;
    std::shared_ptr<NEnumDecl> decl;

public:
    RMember_Enum(const RTypeArgumentsPtr& outerTypeArgs, const std::shared_ptr<NEnumDecl>& decl);

public:
    std::vector<RDeclWithOuterTypeArgs<RFuncDecl>> GetFuncDeclWithOuterTypeArgs() override { return {}; }
    void Accept(RMemberVisitor& visitor) override { visitor.Visit(*this); }
};

class RMember_EnumElem : public RMember 
{
public:
    std::shared_ptr<NEnumElemDecl> decl;
    RTypeArgumentsPtr typeArgs;

public:
    RMember_EnumElem(const std::shared_ptr<NEnumElemDecl>& decl, const RTypeArgumentsPtr& typeArgs);

public:
    std::vector<RDeclWithOuterTypeArgs<RFuncDecl>> GetFuncDeclWithOuterTypeArgs() override { return {}; }
    void Accept(RMemberVisitor& visitor) override { visitor.Visit(*this); }
};

class RMember_EnumElemMemberVar : public RMember 
{
public:
    std::shared_ptr<NEnumElemMemberVarDecl> decl;
    RTypeArgumentsPtr typeArgs;

public:
    RMember_EnumElemMemberVar(const std::shared_ptr<NEnumElemMemberVarDecl>& decl, const RTypeArgumentsPtr& typeArgs);

public:
    std::vector<RDeclWithOuterTypeArgs<RFuncDecl>> GetFuncDeclWithOuterTypeArgs() override { return {}; }
    void Accept(RMemberVisitor& visitor) override { visitor.Visit(*this); }
};

class RMember_LambdaMemberVar : public RMember 
{
public:
    std::shared_ptr<NLambdaMemberVarDecl> decl;
    RTypeArgumentsPtr typeArgs;

public:
    RMember_LambdaMemberVar(const std::shared_ptr<NLambdaMemberVarDecl>& decl, const RTypeArgumentsPtr& typeArgs);

public:
    std::vector<RDeclWithOuterTypeArgs<RFuncDecl>> GetFuncDeclWithOuterTypeArgs() override { return {}; }
    void Accept(RMemberVisitor& visitor) override { visitor.Visit(*this); }
};

// 어떻게 쓰일지 몰라서, 실제로 만들때 채워넣는다
class RMember_TupleMemberVar : public RMember 
{
public:
    RMember_TupleMemberVar();

public:
    std::vector<RDeclWithOuterTypeArgs<RFuncDecl>> GetFuncDeclWithOuterTypeArgs() override { return {}; }
    void Accept(RMemberVisitor& visitor) override { visitor.Visit(*this); }
};

} // namespace Citron

