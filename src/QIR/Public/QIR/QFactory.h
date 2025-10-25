#pragma once
#include "QIRConfig.h"

#include <vector>
#include <memory>

namespace Citron {

class QBlock;

class QValue;
class QValue_ConstInteger;
class QValue_ConstBool;

class QInst;
class QInst_Load;
class QInst_Store;

class QJumpInst;
class QJumpInst_CondJump;
class QJumpInst_Jump;

class QFactory
{
    std::vector<std::unique_ptr<QValue>> values;
    std::vector<std::unique_ptr<QInst>> insts;

public:
    QIR_API QBlock* MakeQBlock();

    QIR_API QInst_Load* MakeQInst_Load(QValue* v, QValue* lv);
    QIR_API QInst_Store* MakeQInst_Store(QValue* lv, QValue* v);

    QIR_API QJumpInst_CondJump* MakeQJumpInst_CondJump(QValue* cond, QBlock* trueBlock, QBlock* falseBlock);
    QIR_API QJumpInst_Jump* MakeQJumpInst_Jump(QBlock* block);

    QIR_API QValue_ConstBool* MakeQValue_ConstBool(bool b);
    QIR_API QValue_ConstInteger* MakeQValue_ConstInteger(int i);
};

}