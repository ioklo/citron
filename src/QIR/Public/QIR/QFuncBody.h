#pragma once
#include <vector>

namespace Citron {

class QBlock;
class NFuncDecl;

struct QFuncBody
{   
    NFuncDecl* funcDecl;
    QBlock* entry;
};


} // namespace Citron
