#pragma once

namespace Citron {

class STypeExp;
class RType;
class NDecl;

namespace SyntaxIR0Translator {

class ResolveTypeHierarchyContext
{
public:
    RType* MakeType(STypeExp* sType, NDecl* decl);

};


} // namespace SyntaxIR0Translator
} // namespace Citron