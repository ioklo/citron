#pragma once

#include <memory>
#include <optional>

#include "RSymbol/RNames.h"

namespace Citron {

class RType;
class RFactory;
class RTypeArguments;
class RClassVarDecl;
class RStructVarDecl;
class REnumElemVarDecl;

struct MExp;

class NLambdaVarDecl;
struct ReExpVisitor;

// NameResolvedExp 
class ReExp
{
public:
    virtual ~ReExp() { }
    virtual void Accept(ReExpVisitor& visitor) = 0;
    virtual RType* GetType() = 0;
};

class ReExp_ThisVar : public ReExp
{ 
public:
    RType* type;

public:
    ReExp_ThisVar(RType* type);
    void Accept(ReExpVisitor& visitor) override;
    RType* GetType() override { return type; }
};

class ReExp_LocalVar : public ReExp
{
public:
    RType* type;
    RName name;
    
public:
    ReExp_LocalVar(RType* type, const RName& name);
    void Accept(ReExpVisitor& visitor) override;
    RType* GetType() override { return type; }
};

class ReExp_LocalRef : public ReExp
{
public:
    RType* type;
    RName name;

public:
    ReExp_LocalRef(RType* type, const RName& name);
    void Accept(ReExpVisitor& visitor) override;
    RType* GetType() override { return type; }
};


class ReExp_LambdaVar : public ReExp
{
public:
    NLambdaVarDecl* decl;
    RTypeArguments* typeArgs;
    
public:
    ReExp_LambdaVar(NLambdaVarDecl* decl, RTypeArguments* typeArgs);
    void Accept(ReExpVisitor& visitor) override;
    RType* GetType() override;
};

class ReExp_ClassVar : public ReExp
{
public:
    RClassVarDecl* decl;
    RTypeArguments* typeArgs;
    bool hasExplicitInstance;
    ReExp* explicitInstance;
    
public:
    ReExp_ClassVar(RClassVarDecl* decl, RTypeArguments* typeArgs, bool hasExplicitInstance, ReExp* explicitInstance);
    void Accept(ReExpVisitor& visitor) override;
    RType* GetType() override;
};

class ReExp_StructVar : public ReExp
{
public:
    RStructVarDecl* decl;
    RTypeArguments* typeArgs;
    bool hasExplicitInstance;
    ReExp* explicitInstance;
    
public:
    ReExp_StructVar(RStructVarDecl* decl, RTypeArguments* typeArgs, bool hasExplicitInstance, ReExp* explicitInstance);
    void Accept(ReExpVisitor& visitor) override;
    RType* GetType() override;
};

class ReExp_EnumElemVar : public ReExp
{
public:
    REnumElemVarDecl* decl;
    RTypeArguments* typeArgs;
    ReExp* instance;

public:
    ReExp_EnumElemVar(REnumElemVarDecl* decl, RTypeArguments* typeArgs, ReExp* instance);
    void Accept(ReExpVisitor& visitor) override;
    RType* GetType() override;
};

class ReExp_PtrDeref : public ReExp
{
public:
    ReExp* target;
    
public:
    ReExp_PtrDeref(ReExp* target);
    void Accept(ReExpVisitor& visitor) override;
    RType* GetType() override;
};

class ReExp_BoxDeref : public ReExp
{
public:
    ReExp* target;

public:
    ReExp_BoxDeref(ReExp* target);
    void Accept(ReExpVisitor& visitor) override;
    RType* GetType() override;
};

class ReExp_ListIndexer : public ReExp
{   
public:
    ReExp* instance;
    ReExp* index;
    RType* itemType;
    
public:
    ReExp_ListIndexer(ReExp* instance, ReExp* index, RType* itemType);
    void Accept(ReExpVisitor& visitor) override;
    RType* GetType() override { return itemType; }
};

// 기타의 경우, Value
class ReExp_Else : public ReExp
{
public:
    MExp* mExp;
    
public:
    ReExp_Else(MExp* ptr);

    void Accept(ReExpVisitor& visitor) override;
    RType* GetType() override;
};

} // namespace Citron

#include "ReExpVisitor.g.h"