#pragma once

namespace Citron {

class STypeExp;
class RType;
class NDecl;

class BuildTypeHierarchyContext
{
public:
    RType* MakeType(STypeExp* sType, NDecl* decl);
};

} // namespace Citron