export module Citron.SyntaxIR0Translator:EnumTranslation;

import <memory>;
import Citron.Syntax;
import Citron.RDecls;
import Citron.NDecls;

import :CommonTranslation;

namespace Citron::SyntaxIR0Translator {

export class SkeletonPhaseContext;

export std::shared_ptr<NEnumDecl> InnerMakeEnum(NTypeDeclOuterWPtr nOuter, SEnumDecl& sDecl, RAccessor accessor, SkeletonPhaseContext& context);

export template<typename TNOuter, typename TMakeAccessor>
std::shared_ptr<NEnumDecl> MakeEnum(const std::shared_ptr<TNOuter>& rOuter, SEnumDecl& sDecl, TMakeAccessor makeAccessor, SkeletonPhaseContext& context)
{
    auto accessor = makeAccessor(sDecl.accessModifier);
    return InnerMakeEnum(rOuter, sDecl, accessor, context);
}

} // namespace Citron::SyntaxIR0Translator