#pragma once
#include <memory>
#include <expected>

namespace Citron {
struct MFuncBody;
using DiagPtr = std::shared_ptr<struct Diag>;

class BuildTypeHierarchyContext;
struct PostBuildTypeHierarchyContexts;
class BuildNonTypeSymbolContext;
struct PostBuildNonTypeSymbolContexts;
class BuildImplicitSymbolContext;
class TranslateBodyContext;

// Translation은 다음 단계로 나뉜다
// 1. Type류들을 symbol tree에 추가한다
// 2. BuildTypeHierarhcy 1번에서 만들어진 symbol tree만 가지고, base type들을 resolve한다 => hierarchy tree가
// 3. BuildNonTypeSymbol 2번으로 만들어진 symbol tree와 hierarchy tree를 가지고, type expression들을 resolve한다
// 4. BuildImplicitSymbol 자동으로 생성되는 함수들을 symbol tree에 추가한다.
// 5. TranslateBody

// base struct / interface 관계를 resolve 하고 상속 트리를 확정하는 단계
class IBuildTypeHierarchyTask 
{
public:
    virtual ~IBuildTypeHierarchyTask() = default;
    virtual std::expected<void, DiagPtr> BuildTypeHierarchy(BuildTypeHierarchyContext& context) = 0;
};

class IPostBuildTypeHierarchyTask
{
public:
    virtual ~IPostBuildTypeHierarchyTask() = default;
    virtual std::expected<void, DiagPtr> OnPostBuildTypeHierarchy(PostBuildTypeHierarchyContexts& contexts) = 0;
};

// 위의 TypeHierarchy가 있어야 TypeExp를 제대로 resolve할 수 있다
// TypeExp를 resolve하고, 타입을 제외한 나머지 symbol을 추가하는 단계
class IBuildNonTypeSymbolTask
{
public:
    virtual ~IBuildNonTypeSymbolTask() = default;
    virtual std::expected<void, DiagPtr> BuildNonTypeSymbol(BuildNonTypeSymbolContext& context) = 0;
};

// BuildNonTypeSymbolPhase가 끝난뒤 수행할 작업.
class IPostBuildNonTypeSymbolTask
{
public:
    virtual ~IPostBuildNonTypeSymbolTask() = default;
    virtual std::expected<void, DiagPtr> PostBuildNonTypeSymbol(PostBuildNonTypeSymbolContexts& context) = 0;

};

// struct / class 등에 자동으로 생성되는 implicit symbol을 추가하는 단계 (memberwise constructor 등)
class IBuildImplicitSymbolTask
{
public:
    virtual ~IBuildImplicitSymbolTask() = default;
    virtual void BuildImplicitSymbol(BuildImplicitSymbolContext& context) = 0;
};

class ITranslateBodyTask
{
public:
    virtual ~ITranslateBodyTask() = default;
    virtual std::expected<MFuncBody, DiagPtr> TranslateBody(TranslateBodyContext& context) = 0;
};


} // namespace Citron
