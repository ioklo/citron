#pragma once

#include <vector>
#include <string>
#include <optional>

#include "Infra/Ref.h"
#include "Syntax/Syntax.h"

namespace Citron {

enum class RAccessor;
class RTypeParam;
class RDecl;
using RFactoryPtr = std::shared_ptr<class RFactory>;
class NFactory;
class RTypeDeclOuter;

enum class RNamespaceMemberAccessor;
enum class RStructMemberAccessor;
enum class RClassMemberAccessor;

// RAccessor MakeAccessor(std::optional<SAccessModifier> modifier, AccessorContext context);
RNamespaceMemberAccessor MakeNamespaceMemberAccessor(std::optional<SAccessModifier> modifier);
RStructMemberAccessor MakeStructMemberAccessor(std::optional<SAccessModifier> modifier);
RClassMemberAccessor MakeClassMemberAccessor(std::optional<SAccessModifier> modifier);

std::vector<RTypeParam*> MakeTypeParams(size_t baseIndex, RDecl* rDecl, const std::vector<STypeParam>& sTypeParams, InRef<RFactoryPtr> rFactory);

} // namespace Citron