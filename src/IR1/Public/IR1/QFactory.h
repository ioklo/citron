#pragma once
#include "IR1Config.h"

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
    IR1_API QBlock* MakeQBlock();

    IR1_API QInst_Load* MakeQInst_Load(QValue* v, QValue* lv);
    IR1_API QInst_Store* MakeQInst_Store(QValue* lv, QValue* v);

    IR1_API QJumpInst_CondJump* MakeQJumpInst_CondJump(QValue* cond, QBlock* trueBlock, QBlock* falseBlock);
    IR1_API QJumpInst_Jump* MakeQJumpInst_Jump(QBlock* block);

    IR1_API QValue_ConstBool* MakeQValue_ConstBool(bool b);
    IR1_API QValue_ConstInteger* MakeQValue_ConstInteger(int i);
};

}