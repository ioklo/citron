export module Citron.NDecls:NLoc;

import "IR0Config.h";
import <variant>;
import <memory>;
import <string>;

import Citron.RDecls;

namespace Citron {

export class RType;
export using RTypePtr = std::shared_ptr<RType>;

export class RTypeFactory;

export class NLoc_Temp;
export class NLoc_LocalVar;
export class NLoc_LambdaVar;
export class NLoc_ListIndexer;
export class NLoc_StructVar;
export class NLoc_ClassVar;
export class NLoc_EnumElemVar;
export class NLoc_This;
export class NLoc_LocalDeref;
export class NLoc_BoxDeref;
export class NLoc_NullableValue;

export class NExp;
export using NExpPtr = std::shared_ptr<NExp>;

export class NLambdaVarDecl;

export class NLocVisitor
{
public:
    virtual ~NLocVisitor() {}
    virtual void Visit(NLoc_Temp& loc) = 0;
    virtual void Visit(NLoc_LocalVar& loc) = 0;
    virtual void Visit(NLoc_LambdaVar& loc) = 0;
    virtual void Visit(NLoc_ListIndexer& loc) = 0;
    virtual void Visit(NLoc_StructVar& loc) = 0;
    virtual void Visit(NLoc_ClassVar& loc) = 0;
    virtual void Visit(NLoc_EnumElemVar& loc) = 0;
    virtual void Visit(NLoc_This& loc) = 0;
    virtual void Visit(NLoc_LocalDeref& loc) = 0;
    virtual void Visit(NLoc_BoxDeref& loc) = 0;
    virtual void Visit(NLoc_NullableValue& loc) = 0;
};

export class NLoc
{
public:
    virtual ~NLoc() {}
    virtual void Accept(NLocVisitor& visitor) = 0;
    virtual RTypePtr GetType(RTypeFactory& factory) = 0;
};

export using NLocPtr = std::shared_ptr<NLoc>;

export class NLoc_Temp : public NLoc
{
public:
    NExpPtr exp;

public:
    IR0_API NLoc_Temp(const NExpPtr& exp);
    IR0_API NLoc_Temp(NExpPtr&& exp);
    void Accept(NLocVisitor& visitor) override { visitor.Visit(*this); }
    IR0_API RTypePtr GetType(RTypeFactory& factory) override;
};

export class NLoc_LocalVar : public NLoc
{
public:
    RName name;
    RTypePtr declType;

public:
    IR0_API NLoc_LocalVar(const RName& name, const RTypePtr& declType);
    void Accept(NLocVisitor& visitor) override { visitor.Visit(*this); }
    IR0_API RTypePtr GetType(RTypeFactory& factory) override;
};

// only this member allowed, so no need this
export class NLoc_LambdaVar : public NLoc
{
public:
    std::shared_ptr<NLambdaVarDecl> decl;
    RTypeArgumentsPtr typeArgs;

public:
    IR0_API NLoc_LambdaVar(const std::shared_ptr<NLambdaVarDecl>& decl, const RTypeArgumentsPtr& typeArgs);
    void Accept(NLocVisitor& visitor) override { visitor.Visit(*this); }
    IR0_API RTypePtr GetType(RTypeFactory& factory) override;
};

// l[b], l is list
export class NLoc_ListIndexer : public NLoc
{
public:
    NLocPtr list;
    NLocPtr index;
    RTypePtr itemType;

public:
    IR0_API NLoc_ListIndexer(NLocPtr&& list, const NLocPtr& index, const RTypePtr& itemType);
    void Accept(NLocVisitor& visitor) override { visitor.Visit(*this); }
    IR0_API RTypePtr GetType(RTypeFactory& factory) override;
};

// Instance가 null이면 static
export class NLoc_StructVar : public NLoc
{
public:
    NLocPtr instance;
    std::shared_ptr<RStructVarDecl> decl;
    RTypeArgumentsPtr typeArgs;

public:
    IR0_API NLoc_StructVar(const NLocPtr& instance, const std::shared_ptr<RStructVarDecl>& decl, const RTypeArgumentsPtr& typeArgs);
    void Accept(NLocVisitor& visitor) override { visitor.Visit(*this); }
    IR0_API RTypePtr GetType(RTypeFactory& factory) override;
};

export class NLoc_ClassVar : public NLoc
{
public:
    NLocPtr instance;
    std::shared_ptr<RClassVarDecl> decl;
    RTypeArgumentsPtr typeArgs;

public:
    NLoc_ClassVar(NLocPtr&& instance, const std::shared_ptr<RClassVarDecl>& decl, const RTypeArgumentsPtr& typeArgs);
    void Accept(NLocVisitor& visitor) override { visitor.Visit(*this); }
    IR0_API RTypePtr GetType(RTypeFactory& factory) override;
};

export class NLoc_EnumElemVar : public NLoc
{
public:
    NLocPtr instance;
    std::shared_ptr<REnumElemVarDecl> decl;
    RTypeArgumentsPtr typeArgs;

public:
    IR0_API NLoc_EnumElemVar(const NLocPtr& instance, std::shared_ptr<REnumElemVarDecl>& decl, const RTypeArgumentsPtr& typeArgs);
    void Accept(NLocVisitor& visitor) override { visitor.Visit(*this); }
    IR0_API RTypePtr GetType(RTypeFactory& factory) override;
};

export class NLoc_This : public NLoc
{
public:
    RTypePtr type;

public:
    IR0_API NLoc_This(RTypePtr type);
    void Accept(NLocVisitor& visitor) override { visitor.Visit(*this); }
    IR0_API RTypePtr GetType(RTypeFactory& factory) override;
};

// dereference pointer, *
export class NLoc_LocalDeref : public NLoc
{
public:
    NLocPtr innerLoc;
public:
    IR0_API NLoc_LocalDeref(NLocPtr&& innerLoc);
    void Accept(NLocVisitor& visitor) override { visitor.Visit(*this); }
    IR0_API RTypePtr GetType(RTypeFactory& factory) override;
};

// dereference box pointer, *
export class NLoc_BoxDeref : public NLoc
{
public:
    NLocPtr innerLoc;

public:
    IR0_API NLoc_BoxDeref(NLocPtr&& innerLoc);
    void Accept(NLocVisitor& visitor) override { visitor.Visit(*this); }
    IR0_API RTypePtr GetType(RTypeFactory& factory) override;
};

// nullable value에서 value를 가져온다
export class NLoc_NullableValue : public NLoc
{
public:
    NLocPtr loc;
public:
    IR0_API NLoc_NullableValue(const NLocPtr& loc);
    void Accept(NLocVisitor& visitor) override { visitor.Visit(*this); }
    IR0_API RTypePtr GetType(RTypeFactory& factory) override;
};

}

