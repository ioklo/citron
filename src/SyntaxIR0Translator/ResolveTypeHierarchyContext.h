#pragma once

namespace Citron {

class STypeExp;
class RType;
class NDecl;

class ResolveTypeHierarchyContext
{
public:
    RType* MakeType(STypeExp* sType, NDecl* decl);

};

} // namespace Citron