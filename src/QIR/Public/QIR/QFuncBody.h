#pragma once
#include <vector>
#include <string>
#include <variant>
#include <optional>
#include "RSymbol/RNames.h"
#include "RSymbol/RFuncDecl.h"

namespace Citron {

class QBlock;
class RType;

enum QSlotRole_ArgumentKind { Direct, Indirect, Ref };

struct QSlotRole_IndirectReturn {};
struct QSlotRole_This {};
struct QSlotRole_Argument { RName name; size_t index; QSlotRole_ArgumentKind kind; };
struct QSlotRole_Local { RName name; };
struct QSlotRole_Temp { std::string debugText; };
struct QSlotRole_Parameter {};
using QSlotRole = std::variant<QSlotRole_IndirectReturn, QSlotRole_This, QSlotRole_Argument, QSlotRole_Local, QSlotRole_Temp, QSlotRole_Parameter>;
struct QSlotInfo
{   
    RType* type;
    size_t slotIndex;
    QSlotRole role;
};

struct QFuncBody
{   
    RFuncDecl* rFuncDecl;
    std::vector<QSlotInfo> slotInfos;
    std::vector<QBlock*> blocks;
};

} // namespace Citron
