#pragma once

#include "Infra/Hash.h"

namespace Citron {

class RTypeArguments;

// fully applied typeParam
template<typename TRDecl>
struct RAppliedDecl
{
    TRDecl* decl;
    RTypeArguments* typeArgs;
    
    RAppliedDecl<TRDecl> Apply(RTypeArguments* typeArgs)
    {
        auto* appliedTypeArgs = this->typeArgs->Apply(typeArgs);
        return RAppliedDecl<TRDecl>{this->decl, appliedTypeArgs};
    }
};


} // namespace Citron

namespace std {

template<typename TRDecl>
struct hash<Citron::RAppliedDecl<TRDecl>>
{
    size_t operator()(const Citron::RAppliedDecl<TRDecl>& appliedDecl) const noexcept
    {
        size_t s = 0;
        Citron::hash_combine(s, appliedDecl.decl);
        Citron::hash_combine(s, appliedDecl.typeArgs);
        return s;
    }
};

} // namespace std
