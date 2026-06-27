#pragma once
#include <variant>

namespace Citron {

class RNamespaceDecl;
class RGlobalFuncDecl;
class RStructDecl;
class RStructCtorDecl;
class RStructDtorDecl;
class RStructFuncDecl;
class RStructVarDecl;
class RClassDecl;
class RClassCtorDecl;
class RClassFuncDecl;
class RClassVarDecl;
class REnumDecl;
class REnumElemDecl;
class REnumElemVarDecl;
class RLambdaDecl;
class RLambdaVarDecl;
class RInterfaceDecl;
class RTypeParamDecl;

class RNodeDecl
{
    using Variant = std::variant<
        RNamespaceDecl*,
        RGlobalFuncDecl*,
        RStructDecl*,
        RStructCtorDecl*,
        RStructDtorDecl*,
        RStructFuncDecl*,
        RStructVarDecl*,
        RClassDecl*,
        RClassCtorDecl*,
        RClassFuncDecl*,
        RClassVarDecl*,
        REnumDecl*,
        REnumElemDecl*,
        REnumElemVarDecl*,
        RLambdaDecl*,
        RLambdaVarDecl*,
        RInterfaceDecl*,
        RTypeParamDecl*
    >;

    Variant v;

public:
    template<typename T> requires (!std::same_as<T, RNodeDecl>)
    RNodeDecl(T&& t) : v{std::forward<T>(t)} 
    { }

}


} // namespace Citron
