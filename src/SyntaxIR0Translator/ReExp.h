#pragma once

#include <memory>
#include <optional>
#include <string>

namespace Citron {

class RType;
class RFactory;
class RTypeArguments;
class RClassVarDecl;
class RStructVarDecl;
class REnumElemVarDecl;

class MExp;

class NLambdaVarDecl;

namespace SyntaxIR0Translator {

class ReExp_ThisVar;
class ReExp_LocalVar;
class ReExp_LambdaVar;
class ReExp_ClassVar;
class ReExp_StructVar;
class ReExp_EnumElemVar;
class ReExp_LocalDeref;
class ReExp_BoxDeref;
class ReExp_ListIndexer;
class ReExp_Else;

class ReExpVisitor;

// NameResolvedExp 
class ReExp
{
public:
    virtual ~ReExp() { }
    virtual void Accept(ReExpVisitor& visitor) = 0;
    virtual RType* GetType(RFactory& factory) = 0;
};

class ReExpVisitor
{
public:    
    virtual ~ReExpVisitor() { }
    virtual void Visit(ReExp_ThisVar* exp) = 0;
    virtual void Visit(ReExp_LocalVar* exp) = 0;
    virtual void Visit(ReExp_LambdaVar* exp) = 0;
    virtual void Visit(ReExp_ClassVar* exp) = 0;
    virtual void Visit(ReExp_StructVar* exp) = 0;
    virtual void Visit(ReExp_EnumElemVar* exp) = 0;
    virtual void Visit(ReExp_LocalDeref* exp) = 0;
    virtual void Visit(ReExp_BoxDeref* exp) = 0;
    virtual void Visit(ReExp_ListIndexer* exp) = 0;
    virtual void Visit(ReExp_Else* exp) = 0;
};

class ReExp_ThisVar : public ReExp
{ 
public:
    RType* type;

public:
    ReExp_ThisVar(RType* type);
    void Accept(ReExpVisitor& visitor) override { visitor.Visit(this); }
    RType* GetType(RFactory& factory) override { return type; }
};

class ReExp_LocalVar : public ReExp
{
public:
    RType* type;
    std::string name;
    
public:
    ReExp_LocalVar(RType* type, const std::string& name);
    void Accept(ReExpVisitor& visitor) override { visitor.Visit(this); }
    RType* GetType(RFactory& factory) override { return type; }
};

class ReExp_LambdaVar : public ReExp
{
public:
    NLambdaVarDecl* decl;
    RTypeArguments* typeArgs;
    
public:
    ReExp_LambdaVar(NLambdaVarDecl* decl, RTypeArguments* typeArgs);
    void Accept(ReExpVisitor& visitor) override { visitor.Visit(this); }
    RType* GetType(RFactory& factory) override;
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
    void Accept(ReExpVisitor& visitor) override { visitor.Visit(this); }
    RType* GetType(RFactory& factory) override;
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
    void Accept(ReExpVisitor& visitor) override { visitor.Visit(this); }
    RType* GetType(RFactory& factory) override;
};

class ReExp_EnumElemVar : public ReExp
{
public:
    REnumElemVarDecl* decl;
    RTypeArguments* typeArgs;
    ReExp* instance;

public:
    ReExp_EnumElemVar(REnumElemVarDecl* decl, RTypeArguments* typeArgs, ReExp* instance);
    void Accept(ReExpVisitor& visitor) override { visitor.Visit(this); }
    RType* GetType(RFactory& factory) override;
};

class ReExp_LocalDeref : public ReExp
{
public:
    ReExp* target;
    
public:
    ReExp_LocalDeref(ReExp* target);
    void Accept(ReExpVisitor& visitor) override { visitor.Visit(this); }
    RType* GetType(RFactory& factory) override;
};

class ReExp_BoxDeref : public ReExp
{
public:
    ReExp* target;

public:
    ReExp_BoxDeref(ReExp* target);
    void Accept(ReExpVisitor& visitor) override { visitor.Visit(this); }
    RType* GetType(RFactory& factory) override;
};

class ReExp_ListIndexer : public ReExp
{   
public:
    ReExp* instance;
    ReExp* index;
    RType* itemType;
    
public:
    ReExp_ListIndexer(ReExp* instance, ReExp* index, RType* itemType);
    void Accept(ReExpVisitor& visitor) override { visitor.Visit(this); }
    RType* GetType(RFactory& factory) override { return itemType; }
};

// 기타의 경우, Value
class ReExp_Else : public ReExp
{
public:
    MExp* mExp;
    
public:
    ReExp_Else(MExp* ptr);

    void Accept(ReExpVisitor& visitor) override { visitor.Visit(this); }
    RType* GetType(RFactory& factory) override;
};

template<class TFrom, class TVisitor>
concept ReExpConvertibleToResultType = std::convertible_to<TFrom, typename std::remove_cvref_t<TVisitor>::ResultType>;

// TResult타입은 &가 안되므로, reference_wrapper<TResult>를 쓰도록 합니다
template<typename TVisitor, typename... TVisitorArgs>
concept ReExpVisitable = requires(TVisitor&& v, TVisitorArgs&&... args)
{
    typename std::remove_cvref_t<TVisitor>::ResultType;

    { v.Visit(std::declval<ReExp_ThisVar*>(), std::forward<TVisitorArgs>(args)...) } -> ReExpConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<ReExp_LocalVar*>(), std::forward<TVisitorArgs>(args)...) } -> ReExpConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<ReExp_LambdaVar*>(), std::forward<TVisitorArgs>(args)...) } -> ReExpConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<ReExp_ClassVar*>(), std::forward<TVisitorArgs>(args)...) } -> ReExpConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<ReExp_StructVar*>(), std::forward<TVisitorArgs>(args)...) } -> ReExpConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<ReExp_EnumElemVar*>(), std::forward<TVisitorArgs>(args)...) } -> ReExpConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<ReExp_LocalDeref*>(), std::forward<TVisitorArgs>(args)...) } -> ReExpConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<ReExp_BoxDeref*>(), std::forward<TVisitorArgs>(args)...) } -> ReExpConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<ReExp_ListIndexer*>(), std::forward<TVisitorArgs>(args)...) } -> ReExpConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<ReExp_Else*>(), std::forward<TVisitorArgs>(args)...) } -> ReExpConvertibleToResultType<TVisitor>;
};

template<typename TVisitor, typename... TVisitorArgs> requires ReExpVisitable<TVisitor, TVisitorArgs...>
typename std::remove_cvref_t<TVisitor>::ResultType Accept(TVisitor&& v, ReExp* reExp, TVisitorArgs&&... args)
{
    using TResult = typename std::remove_cvref_t<TVisitor>::ResultType;

    // 계약 타입으로 변환(값/참조 정책을 Visit 시그니처가 결정)
    auto caller = [&](auto* e) { return v.Visit(e, std::forward<TVisitorArgs>(args)...); };

    if constexpr (std::is_void_v<TResult>)
    {
        struct Bridge : ReExpVisitor {
            decltype(caller)& call;
            Bridge(decltype(caller)& call) : call(call) {}
            void Visit(ReExp_ThisVar* reExp) override { call(reExp); }
            void Visit(ReExp_LocalVar* reExp) override { call(reExp); }
            void Visit(ReExp_LambdaVar* reExp) override { call(reExp); }
            void Visit(ReExp_ClassVar* reExp) override { call(reExp); }
            void Visit(ReExp_StructVar* reExp) override { call(reExp); }
            void Visit(ReExp_EnumElemVar* reExp) override { call(reExp); }
            void Visit(ReExp_LocalDeref* reExp) override { call(reExp); }
            void Visit(ReExp_BoxDeref* reExp) override { call(reExp); }
            void Visit(ReExp_ListIndexer* reExp) override { call(reExp); }
            void Visit(ReExp_Else* reExp) override { call(reExp); }
        };

        Bridge bridge{caller};
        reExp->Accept(bridge);
    }
    else
    {
        struct Bridge : ReExpVisitor {
            decltype(caller)& call;
            std::optional<TResult> result{};
            Bridge(decltype(caller)& call) : call(call) {}

            void Visit(ReExp_ThisVar* reExp) override { result.emplace(call(reExp)); }
            void Visit(ReExp_LocalVar* reExp) override { result.emplace(call(reExp)); }
            void Visit(ReExp_LambdaVar* reExp) override { result.emplace(call(reExp)); }
            void Visit(ReExp_ClassVar* reExp) override { result.emplace(call(reExp)); }
            void Visit(ReExp_StructVar* reExp) override { result.emplace(call(reExp)); }
            void Visit(ReExp_EnumElemVar* reExp) override { result.emplace(call(reExp)); }
            void Visit(ReExp_LocalDeref* reExp) override { result.emplace(call(reExp)); }
            void Visit(ReExp_BoxDeref* reExp) override { result.emplace(call(reExp)); }
            void Visit(ReExp_ListIndexer* reExp) override { result.emplace(call(reExp)); }
            void Visit(ReExp_Else* reExp) override { result.emplace(call(reExp)); }
        };

        Bridge bridge{caller};
        reExp->Accept(bridge);
        return *bridge.result;
    }
}

} // namespace SyntaxIR0Translator
} // namespace Citron