export module Citron.SyntaxIR0Translator:ImExp;

import <memory>;
import <string>;

import Citron.RDecls;
import Citron.NDecls;

import :FuncsWithPartialTypeArgsComponent;

namespace Citron::SyntaxIR0Translator {

export class ReExp;
export using ReExpPtr = std::shared_ptr<ReExp>;

class ImExp_Namespace;
class ImExp_GlobalFuncs;
class ImExp_TypeVar;
class ImExp_Class;
class ImExp_ClassFuncs;
class ImExp_Struct;
class ImExp_StructFuncs;
class ImExp_Enum;
class ImExp_EnumElem;
class ImExp_ThisVar;
class ImExp_LocalVar;
class ImExp_LambdaVar;
class ImExp_ClassVar;
class ImExp_StructVar;
class ImExp_EnumElemVar;
class ImExp_ListIndexer;
class ImExp_LocalDeref;
class ImExp_BoxDeref;
class ImExp_Else;
class ImExpVisitor;

export class ImExp
{
public:
    virtual ~ImExp() { }
    virtual void Accept(ImExpVisitor& visitor) = 0;
};

export using ImExpPtr = std::shared_ptr<ImExp>;

export class ImExpVisitor
{
public:
    virtual ~ImExpVisitor() {}
    virtual void Visit(ImExp_Namespace& imExp) = 0;
    virtual void Visit(ImExp_GlobalFuncs& imExp) = 0;
    virtual void Visit(ImExp_TypeVar& imExp) = 0;
    virtual void Visit(ImExp_Class& imExp) = 0;
    virtual void Visit(ImExp_ClassFuncs& imExp) = 0;
    virtual void Visit(ImExp_Struct& imExp) = 0;
    virtual void Visit(ImExp_StructFuncs& imExp) = 0;
    virtual void Visit(ImExp_Enum& imExp) = 0;
    virtual void Visit(ImExp_EnumElem& imExp) = 0;
    virtual void Visit(ImExp_ThisVar& imExp) = 0;
    virtual void Visit(ImExp_LocalVar& imExp) = 0;
    virtual void Visit(ImExp_LambdaVar& imExp) = 0;
    virtual void Visit(ImExp_ClassVar& imExp) = 0;
    virtual void Visit(ImExp_StructVar& imExp) = 0;
    virtual void Visit(ImExp_EnumElemVar& imExp) = 0;
    virtual void Visit(ImExp_ListIndexer& imExp) = 0;
    virtual void Visit(ImExp_LocalDeref& imExp) = 0;
    virtual void Visit(ImExp_BoxDeref& imExp) = 0;
    virtual void Visit(ImExp_Else& imExp) = 0;
};

export class ImExp_Namespace : public ImExp
{
public:
    std::shared_ptr<RNamespaceDecl> _namespace; // namespace를 뭘로 저장하고 있어야 하나

public:
    ImExp_Namespace(const std::shared_ptr<RNamespaceDecl>& _namespace);
    void Accept(ImExpVisitor& visitor) override { visitor.Visit(*this); }
};

// 
export class ImExp_GlobalFuncs
    : public ImExp
    , private FuncsWithPartialTypeArgsComponent<RGlobalFuncDecl>
{
    using FuncComp = FuncsWithPartialTypeArgsComponent<RGlobalFuncDecl>;

public:
    using FuncComp::items;

public:
    ImExp_GlobalFuncs(const std::vector<DeclWithOuterTypeArgs<RGlobalFuncDecl>>& items, const std::shared_ptr<RTypeArguments>& partialTypeArgsExceptOuter);

public:
    using FuncComp::GetCount;
    using FuncComp::GetDecl;
    using FuncComp::GetOuterTypeArgs;
    using FuncComp::GetPartialTypeArgsExceptOuter;

    void Accept(ImExpVisitor& visitor) override { visitor.Visit(*this); }
};

export class ImExp_TypeVar : public ImExp
{
public:
    std::shared_ptr<RType_TypeVar> type;

public:
    ImExp_TypeVar(std::shared_ptr<RType_TypeVar>&& type);

public:
    void Accept(ImExpVisitor& visitor) override { visitor.Visit(*this); }
};

export class ImExp_Class : public ImExp
{
public:
    std::shared_ptr<RClassDecl> classDecl;
    RTypeArgumentsPtr typeArgs;

public:
    ImExp_Class(const std::shared_ptr<RClassDecl>& classDecl, RTypeArgumentsPtr&& typeArgs);
    void Accept(ImExpVisitor& visitor) override { visitor.Visit(*this); }
};

export class ImExp_ClassFuncs 
    : public ImExp
    , private FuncsWithPartialTypeArgsComponent<RClassFuncDecl>
{
public:
    // HasExplicitInstance: x.F 처럼 x가 명시적으로 있는 경우 true, F 처럼 this.F 나 C.F 를 암시적으로 나타낸 경우라면 false, C.F는 명시적이지만 인스턴스가 아니므로 false
    // ExplicitInstance: HasExplicitInstance가 true일때만 의미가 있다

    // C.F => HasExplicitInstance: true, null
    // x.F => HasExplicitInstance: true, "x"
    // F   => HasExplicitInstance: false, null
    using FuncsWithPartialTypeArgsComponent::items;
    bool hasExplicitInstance;
    ReExpPtr explicitInstance;

private:
    using FuncComp = FuncsWithPartialTypeArgsComponent<RClassFuncDecl>;

public:
    ImExp_ClassFuncs(const std::vector<DeclWithOuterTypeArgs<RClassFuncDecl>>& items, const std::shared_ptr<RTypeArguments>& partialTypeArgsExceptOuter, bool hasExplicitInstance, const ReExpPtr& explicitInstance);

public:
    using FuncComp::GetCount;
    using FuncComp::GetDecl;
    using FuncComp::GetOuterTypeArgs;
    using FuncComp::GetPartialTypeArgsExceptOuter;

    void Accept(ImExpVisitor& visitor) override { visitor.Visit(*this); }
};

export class ImExp_Struct : public ImExp
{
public:
    std::shared_ptr<RStructDecl> structDecl;
    RTypeArgumentsPtr typeArgs;

public:
    ImExp_Struct(const std::shared_ptr<RStructDecl>& structDecl, RTypeArgumentsPtr&& typeArgs);

public:
    void Accept(ImExpVisitor& visitor) override { visitor.Visit(*this); }
};

export class ImExp_StructFuncs 
    : public ImExp
    , private FuncsWithPartialTypeArgsComponent<RStructFuncDecl>
{
    using FuncComp = FuncsWithPartialTypeArgsComponent<RStructFuncDecl>;

public:
    using FuncComp::items;
    bool hasExplicitInstance;
    ReExpPtr explicitInstance;

public:
    ImExp_StructFuncs(const std::vector<DeclWithOuterTypeArgs<RStructFuncDecl>>& items, const std::shared_ptr<RTypeArguments>& partialTypeArgsExceptOuter, bool hasExplicitInstance, const ReExpPtr& explicitInstance);

public:
    using FuncComp::GetCount;
    using FuncComp::GetDecl;
    using FuncComp::GetOuterTypeArgs;
    using FuncComp::GetPartialTypeArgsExceptOuter;

    void Accept(ImExpVisitor& visitor) override { visitor.Visit(*this); }
};

export class ImExp_Enum : public ImExp
{
public:
    std::shared_ptr<REnumDecl> decl;
    RTypeArgumentsPtr typeArgs;

public:
    ImExp_Enum(const std::shared_ptr<REnumDecl>& decl, const RTypeArgumentsPtr& typeArgs);

public:
    void Accept(ImExpVisitor& visitor) override { visitor.Visit(*this); }
};

export class ImExp_EnumElem : public ImExp
{
public:
    std::shared_ptr<REnumElemDecl> decl;
    RTypeArgumentsPtr typeArgs;

public:
    ImExp_EnumElem(const std::shared_ptr<REnumElemDecl>& decl, const RTypeArgumentsPtr& typeArgs);

public:
    void Accept(ImExpVisitor& visitor) override { visitor.Visit(*this); }
};

// exp로 사용할 수 있는
export class ImExp_ThisVar : public ImExp
{
public:
    RTypePtr type;

public:
    ImExp_ThisVar(const RTypePtr& type);

public:
    void Accept(ImExpVisitor& visitor) override { visitor.Visit(*this); }
};

export class ImExp_LocalVar : public ImExp
{
public:
    RTypePtr type;
    std::string name;

public: 
    ImExp_LocalVar(const RTypePtr& type, const std::string& name);

public:
    void Accept(ImExpVisitor& visitor) override { visitor.Visit(*this); }
};

export class ImExp_LambdaVar : public ImExp
{
public:
    std::shared_ptr<NLambdaVarDecl> decl;
    RTypeArgumentsPtr typeArgs;

public:
    ImExp_LambdaVar(const std::shared_ptr<NLambdaVarDecl>& decl, const RTypeArgumentsPtr& typeArgs);

public:
    void Accept(ImExpVisitor& visitor) override { visitor.Visit(*this); }
};

export class ImExp_ClassVar : public ImExp
{
public:
    std::shared_ptr<RClassVarDecl> decl;
    RTypeArgumentsPtr typeArgs;
    
    bool hasExplicitInstance;
    ReExpPtr explicitInstance;

public:
    ImExp_ClassVar(const std::shared_ptr<RClassVarDecl>& decl, const RTypeArgumentsPtr& typeArgs, bool hasExplicitInstance, const ReExpPtr& explicitInstance);

public:
    void Accept(ImExpVisitor& visitor) override { visitor.Visit(*this); }
};

export class ImExp_StructVar : public ImExp
{
public:
    std::shared_ptr<RStructVarDecl> decl;
    RTypeArgumentsPtr typeArgs;
    
    bool hasExplicitInstance;
    ReExpPtr explicitInstance;

public:
    ImExp_StructVar(const std::shared_ptr<RStructVarDecl>& decl, const RTypeArgumentsPtr& typeArgs, bool hasExplicitInstance, const ReExpPtr& explicitInstance);

public:
    void Accept(ImExpVisitor& visitor) override { visitor.Visit(*this); }
};

export class ImExp_EnumElemVar : public ImExp
{
public:
    std::shared_ptr<REnumElemVarDecl> decl;
    RTypeArgumentsPtr typeArgs;
    ReExpPtr instance;

public:
    ImExp_EnumElemVar(const std::shared_ptr<REnumElemVarDecl>& decl, const RTypeArgumentsPtr& typeArgs, const ReExpPtr& instance);

public:
    void Accept(ImExpVisitor& visitor) override { visitor.Visit(*this); }
};

export class ImExp_ListIndexer : public ImExp
{
public:
    ReExpPtr instance;
    NLocPtr index;
    RTypePtr itemType;

public:
    ImExp_ListIndexer(ReExpPtr&& instance, NLocPtr&& index, RTypePtr&& itemType);

public:    
    void Accept(ImExpVisitor& visitor) override { visitor.Visit(*this); }
};

export class ImExp_LocalDeref : public ImExp
{
public:
    ReExpPtr target;

public:
    ImExp_LocalDeref(const ReExpPtr& target);

public:
    void Accept(ImExpVisitor& visitor) override { visitor.Visit(*this); }
};

export class ImExp_BoxDeref : public ImExp
{
public:
    ReExpPtr target;

public:
    ImExp_BoxDeref(const ReExpPtr& target);

public:
    void Accept(ImExpVisitor& visitor) override { visitor.Visit(*this); }
};

// 기타의 경우
export class ImExp_Else : public ImExp
{
public:
    NExpPtr exp;

public:
    ImExp_Else(const NExpPtr& exp);

public:
    void Accept(ImExpVisitor& visitor) override { visitor.Visit(*this); }
};

} // namespace Citron::SyntaxIR0Translator