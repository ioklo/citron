#pragma once
#include <vector>

namespace Citron {

class RTraitDecl;
class RTypeArguments;
class NImplTraitMember;

struct NImplTrait
{
    // 뭘 구현했는가
    RTraitDecl* trait;
    RTypeArguments* traitTypeArgs;

    std::vector<NImplTraitMember> members; // trait 선언 순서를 따른다

public:
    NImplTrait();
    ~NImplTrait();
};

} // namespace Citron 