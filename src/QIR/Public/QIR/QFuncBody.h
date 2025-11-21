#pragma once
#include <vector>
#include <string>

namespace Citron {

class QBlock;
class NFuncDecl;
struct QType;

struct QStackSlot
{   
    std::string name;
    QType* qType;
};

struct QFuncBody
{   
    NFuncDecl* nFuncDecl;
    std::vector<QStackSlot> stackSlots;
    QBlock* entry;
    size_t registerCount;
};


} // namespace Citron
