#pragma once
#include <vector>
#include <string>

#include "QRegisterType.h"

namespace Citron {

class QBlock;
class NFuncDecl;
struct QType;

// 실행속도는 생각하지 않는다
struct QRegisterInfo
{
    QRegisterType type;
    std::string name; // %i2_a
};

struct QStackSlotInfo
{   
    std::string name;
    QType* qType;
};

struct QFuncBody
{   
    NFuncDecl* nFuncDecl;
    std::vector<QRegisterInfo> regInfos;
    std::vector<QStackSlotInfo> slotInfos;
    QBlock* entry;
};


} // namespace Citron
