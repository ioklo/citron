#pragma once
#include "MIRConfig.h"

#include <variant>
#include <string>
#include <optional>

#include "RSymbol/RNames.h"
#include "MCreate.h"

namespace Citron {

class RType;
class RTypeArguments;

class RFactory;
class RLambdaVarDecl;
class RStructVarDecl;
class RClassVarDecl;
class REnumElemVarDecl;

class MLoc_Materialize;
class MLoc_LocalVar;
class MLoc_LocalRef;
class MLoc_LambdaVar;
class MLoc_ListIndexer;
class MLoc_StructVar;
class MLoc_ClassVar;
class MLoc_EnumElemVar;
class MLoc_This;
class MLoc_PtrDeref;
class MLoc_BoxDeref;
class MLoc_NullableValue;

class MExp;

class NLambdaVarDecl;

class MLocVisitor
{
public:
    virtual ~MLocVisitor() {}
    virtual void Visit(MLoc_Materialize* loc) = 0;
    virtual void Visit(MLoc_LocalVar* loc) = 0;
    virtual void Visit(MLoc_LocalRef* loc) = 0;
    virtual void Visit(MLoc_LambdaVar* loc) = 0;
    virtual void Visit(MLoc_ListIndexer* loc) = 0;
    virtual void Visit(MLoc_StructVar* loc) = 0;
    virtual void Visit(MLoc_ClassVar* loc) = 0;
    virtual void Visit(MLoc_EnumElemVar* loc) = 0;
    virtual void Visit(MLoc_This* loc) = 0;
    virtual void Visit(MLoc_PtrDeref* loc) = 0;
    virtual void Visit(MLoc_BoxDeref* loc) = 0;
    virtual void Visit(MLoc_NullableValue* loc) = 0;
};

class MLoc
{
public:
    virtual ~MLoc() {}
    virtual void Accept(MLocVisitor& visitor) = 0;
    virtual RType* GetType() = 0;
};

class MLoc_Materialize : public MLoc
{
public:
    MCreate create;

public:
    MIR_API MLoc_Materialize(MCreate&& create);
    void Accept(MLocVisitor& visitor) override { visitor.Visit(this); }
    MIR_API RType* GetType() override;
};

class MLoc_LocalVar : public MLoc
{
public:
    RName name;
    RType* declType;

public:
    MIR_API MLoc_LocalVar(const RName& name, RType* declType);
    void Accept(MLocVisitor& visitor) override { visitor.Visit(this); }
    MIR_API RType* GetType() override;
};

class MLoc_LocalRef : public MLoc
{
public:
    RName name;
    RType* declType;

public:
    MIR_API MLoc_LocalRef(const RName& name, RType* declType);
    void Accept(MLocVisitor& visitor) override { visitor.Visit(this); }
    MIR_API RType* GetType() override;
};

// only this member allowed, so no need this
class MLoc_LambdaVar : public MLoc
{
public:
    RLambdaVarDecl* decl;
    RTypeArguments* typeArgs;

public:
    MIR_API MLoc_LambdaVar(RLambdaVarDecl* decl, RTypeArguments* typeArgs);
    void Accept(MLocVisitor& visitor) override { visitor.Visit(this); }
    MIR_API RType* GetType() override;
};

// l[b], l is list
class MLoc_ListIndexer : public MLoc
{
public:
    MLoc* list;
    MLoc* index;
    RType* itemType;

public:
    MIR_API MLoc_ListIndexer(MLoc* list, MLoc* index, RType* itemType);
    void Accept(MLocVisitor& visitor) override { visitor.Visit(this); }
    MIR_API RType* GetType() override;
};

// Instance가 null이면 static
class MLoc_StructVar : public MLoc
{
public:
    MLoc* instance;
    RStructVarDecl* decl;
    RTypeArguments* typeArgs;

public:
    MIR_API MLoc_StructVar(MLoc* instance, RStructVarDecl* decl, RTypeArguments* typeArgs);
    void Accept(MLocVisitor& visitor) override { visitor.Visit(this); }
    MIR_API RType* GetType() override;
};

class MLoc_ClassVar : public MLoc
{
public:
    MLoc* instance;
    RClassVarDecl* decl;
    RTypeArguments* typeArgs;

public:
    MIR_API MLoc_ClassVar(MLoc* instance, RClassVarDecl* decl, RTypeArguments* typeArgs);
    void Accept(MLocVisitor& visitor) override { visitor.Visit(this); }
    MIR_API RType* GetType() override;
};

class MLoc_EnumElemVar : public MLoc
{
public:
    MLoc* instance;
    REnumElemVarDecl* decl;
    RTypeArguments* typeArgs;

public:
    MIR_API MLoc_EnumElemVar(MLoc* instance, REnumElemVarDecl* decl, RTypeArguments* typeArgs);
    void Accept(MLocVisitor& visitor) override { visitor.Visit(this); }
    MIR_API RType* GetType() override;
};

class MLoc_This : public MLoc
{
public:
    RType* type;

public:
    MIR_API MLoc_This(RType* type);
    void Accept(MLocVisitor& visitor) override { visitor.Visit(this); }
    MIR_API RType* GetType() override;
};

// dereference pointer, *
class MLoc_PtrDeref : public MLoc
{
public:
    MLoc* innerLoc;
public:
    MIR_API MLoc_PtrDeref(MLoc* innerLoc);
    void Accept(MLocVisitor& visitor) override { visitor.Visit(this); }
    MIR_API RType* GetType() override;
};

// dereference box pointer, *
class MLoc_BoxDeref : public MLoc
{
public:
    MLoc* innerLoc;

public:
    MIR_API MLoc_BoxDeref(MLoc* innerLoc);
    void Accept(MLocVisitor& visitor) override { visitor.Visit(this); }
    MIR_API RType* GetType() override;
};

// nullable value에서 value를 가져온다
class MLoc_NullableValue : public MLoc
{
public:
    MLoc* loc;
public:
    MIR_API MLoc_NullableValue(MLoc* loc);
    void Accept(MLocVisitor& visitor) override { visitor.Visit(this); }
    MIR_API RType* GetType() override;
};

template<class TFrom, class TVisitor>
concept MLocConvertibleToResultType = std::convertible_to<TFrom, typename std::remove_cvref_t<TVisitor>::ResultType>;

// TResult타입은 &가 안되므로, reference_wrapper<TResult>를 쓰도록 합니다
template<typename TVisitor, typename... TVisitorArgs>
concept MLocVisitable = requires(TVisitor && v, TVisitorArgs&&... args)
{
    typename std::remove_cvref_t<TVisitor>::ResultType;

    { v.Visit(std::declval<MLoc_Materialize*>(), std::forward<TVisitorArgs>(args)...) } -> MLocConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<MLoc_LocalVar*>(), std::forward<TVisitorArgs>(args)...) } -> MLocConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<MLoc_LocalRef*>(), std::forward<TVisitorArgs>(args)...) } -> MLocConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<MLoc_LambdaVar*>(), std::forward<TVisitorArgs>(args)...) } -> MLocConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<MLoc_ListIndexer*>(), std::forward<TVisitorArgs>(args)...) } -> MLocConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<MLoc_StructVar*>(), std::forward<TVisitorArgs>(args)...) } -> MLocConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<MLoc_ClassVar*>(), std::forward<TVisitorArgs>(args)...) } -> MLocConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<MLoc_EnumElemVar*>(), std::forward<TVisitorArgs>(args)...) } -> MLocConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<MLoc_This*>(), std::forward<TVisitorArgs>(args)...) } -> MLocConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<MLoc_PtrDeref*>(), std::forward<TVisitorArgs>(args)...) } -> MLocConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<MLoc_BoxDeref*>(), std::forward<TVisitorArgs>(args)...) } -> MLocConvertibleToResultType<TVisitor>;
    { v.Visit(std::declval<MLoc_NullableValue*>(), std::forward<TVisitorArgs>(args)...) } -> MLocConvertibleToResultType<TVisitor>;
};

template<typename TVisitor, typename... TVisitorArgs> requires MLocVisitable<TVisitor, TVisitorArgs...>
typename std::remove_cvref_t<TVisitor>::ResultType Accept(TVisitor&& v, MLoc* mLoc, TVisitorArgs&&... args)
{
    using TResult = typename std::remove_cvref_t<TVisitor>::ResultType;

    // 계약 타입으로 변환(값/참조 정책을 Visit 시그니처가 결정)
    auto caller = [&](auto* e) { return v.Visit(e, std::forward<TVisitorArgs>(args)...); };

    if constexpr (std::is_void_v<TResult>)
    {
        struct Bridge : MLocVisitor {
            decltype(caller)& call;
            Bridge(decltype(caller)& call) : call(call) {}

            void Visit(MLoc_Materialize* loc) override { call(loc); }
            void Visit(MLoc_LocalVar* loc) override { call(loc); }
            void Visit(MLoc_LocalRef* loc) override { call(loc); }
            void Visit(MLoc_LambdaVar* loc) override { call(loc); }
            void Visit(MLoc_ListIndexer* loc) override { call(loc); }
            void Visit(MLoc_StructVar* loc) override { call(loc); }
            void Visit(MLoc_ClassVar* loc) override { call(loc); }
            void Visit(MLoc_EnumElemVar* loc) override { call(loc); }
            void Visit(MLoc_This* loc) override { call(loc); }
            void Visit(MLoc_PtrDeref* loc) override { call(loc); }
            void Visit(MLoc_BoxDeref* loc) override { call(loc); }
            void Visit(MLoc_NullableValue* loc) override { call(loc); }
        };

        Bridge bridge{caller};
        mLoc->Accept(bridge);
    }
    else
    {
        struct Bridge : MLocVisitor {
            decltype(caller)& call;
            std::optional<TResult> result{};
            Bridge(decltype(caller)& call) : call(call) {}

            void Visit(MLoc_Materialize* loc) override { result.emplace(call(loc)); }
            void Visit(MLoc_LocalVar* loc) override { result.emplace(call(loc)); }
            void Visit(MLoc_LocalRef* loc) override { result.emplace(call(loc)); }
            void Visit(MLoc_LambdaVar* loc) override { result.emplace(call(loc)); }
            void Visit(MLoc_ListIndexer* loc) override { result.emplace(call(loc)); }
            void Visit(MLoc_StructVar* loc) override { result.emplace(call(loc)); }
            void Visit(MLoc_ClassVar* loc) override { result.emplace(call(loc)); }
            void Visit(MLoc_EnumElemVar* loc) override { result.emplace(call(loc)); }
            void Visit(MLoc_This* loc) override { result.emplace(call(loc)); }
            void Visit(MLoc_PtrDeref* loc) override { result.emplace(call(loc)); }
            void Visit(MLoc_BoxDeref* loc) override { result.emplace(call(loc)); }
            void Visit(MLoc_NullableValue* loc) override { result.emplace(call(loc)); }
        };

        Bridge bridge{caller};
        mLoc->Accept(bridge);
        return *bridge.result;
    }
}

}

