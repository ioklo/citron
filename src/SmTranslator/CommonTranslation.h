#pragma once

#include <vector>
#include <span>
#include <optional>
#include <expected>
#include <memory>
#include "Infra/RefPtr.h"
#include "Syntax/Syntax.h"

namespace Citron {

using DiagPtr = std::shared_ptr<struct Diag>;
enum class RAccessor;
class RTypeParam;
class RDecl;
class RTypeDeclOuter;
class RFuncReturn;
struct RFuncParameter;
class RFactory;
struct SmTypeTranslationContexts;


enum class RNamespaceMemberAccessor;
enum class RStructMemberAccessor;
enum class RClassMemberAccessor;

// RAccessor MakeAccessor(std::optional<SAccessModifier> modifier, AccessorContext context);
RNamespaceMemberAccessor MakeNamespaceMemberAccessor(std::optional<SAccessModifier> modifier);
RStructMemberAccessor MakeStructMemberAccessor(std::optional<SAccessModifier> modifier);
RClassMemberAccessor MakeClassMemberAccessor(std::optional<SAccessModifier> modifier);

std::vector<RTypeParam*> MakeTypeParams(size_t baseIndex, RDecl* rDecl, std::span<STypeParam> sTypeParams, InRefPtr<RFactory> rFactory);

std::expected<RFuncReturn, DiagPtr> MakeFuncReturn(SFuncReturn& funcRet, RDecl* decl, std::span<RTypeParam*> typeParams, SmTypeTranslationContexts& contexts);

std::expected<std::tuple<std::vector<RFuncParameter>, bool>, DiagPtr> MakeFuncParameters(std::span<SFuncParam> sParams, SmTypeTranslationContexts& contexts);

} // namespace Citron