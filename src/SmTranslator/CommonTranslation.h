#pragma once

#include <vector>
#include <string>
#include <optional>

#include "Infra/Ref.h"
#include "Syntax/Syntax.h"

namespace Citron {

enum class RAccessor;
class RTypeParamDecl;
class RDecl;
using RFactoryPtr = std::shared_ptr<class RFactory>;
class NFactory;

enum class RNamespaceMemberAccessor;
enum class RStructMemberAccessor;
enum class RClassMemberAccessor;

// RAccessor MakeAccessor(std::optional<SAccessModifier> modifier, AccessorContext context);
RNamespaceMemberAccessor MakeNamespaceMemberAccessor(std::optional<SAccessModifier> modifier);
RStructMemberAccessor MakeStructMemberAccessor(std::optional<SAccessModifier> modifier);
RClassMemberAccessor MakeClassMemberAccessor(std::optional<SAccessModifier> modifier);

std::vector<RTypeParamDecl*> MakeTypeParams(RDecl* outer, const std::vector<STypeParam>& sTypeParams, InRef<RFactoryPtr> rFactory);

} // namespace Citron