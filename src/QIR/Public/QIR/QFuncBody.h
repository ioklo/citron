#pragma once
#include <vector>
#include <string>
#include <variant>
#include <optional>

namespace Citron {

class QBlock;
class NFuncDecl;
struct QType;

struct QSlotInfo
{   
    QType* qType;
    std::string name;
    std::optional<size_t> o_argIndex;
};

struct QFuncBody
{   
    NFuncDecl* nFuncDecl;
    std::vector<QSlotInfo> slotInfos;
    std::vector<QBlock*> blocks;
};


} // namespace Citron
