export module Citron.SyntaxIR0Translator:StructTranslation;

import :CommonTranslation;
import <memory>;

import Citron.Syntax;
import Citron.RDecls;
import Citron.NDecls;

namespace Citron::SyntaxIR0Translator {

export class SkeletonPhaseContext;

export std::shared_ptr<NStructDecl> InnerMakeStruct(const std::shared_ptr<SStructDecl>& sDecl, const std::shared_ptr<NTypeDeclOuter>& nOuter, RAccessor accessor, SkeletonPhaseContext& context);

export template<typename TNOuter, typename TMakeAccessor>
std::shared_ptr<NStructDecl> MakeStruct(const std::shared_ptr<TNOuter>& nOuter, const std::shared_ptr<SStructDecl>& sStruct, TMakeAccessor makeAccessor, SkeletonPhaseContext& context)
{
    auto accessor = makeAccessor(sStruct->accessModifier);
    return InnerMakeStruct(sStruct, nOuter, accessor, context);
}

} // namespace Citron::SyntaxIR0Translator