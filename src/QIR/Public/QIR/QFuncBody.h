#pragma once
#include <vector>

namespace Citron {

class QBlock;
class NFuncDecl;

struct QFuncBody
{   
    NFuncDecl* nFuncDecl;
    QBlock* entry;
    size_t registerCount;
};


} // namespace Citron
