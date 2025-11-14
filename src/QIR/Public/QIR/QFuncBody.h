#pragma once
#include <vector>

namespace Citron {

class QBlock;
class NFuncDecl;

struct QFuncBody
{   
    NFuncDecl* nFuncDecl;
    QBlock* entry;
};


} // namespace Citron
