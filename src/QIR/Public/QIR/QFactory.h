#pragma once
#include "QIRConfig.h"

#include <vector>
#include <memory>
#include <string>

#include "QTypes.h"

namespace Citron {

class QData;
class QBlock;
struct QFuncBody;

class QFactory
{
    std::vector<std::unique_ptr<QData>> dataList;
    std::vector<std::unique_ptr<QBlock>> blocks;
    QType_Void voidType;
    QType_Ptr ptrType;
    QType_Struct boolType;
    QType_Struct intType;
    QType_Class stringType;

public:
    QIR_API QData* MakeQData(std::vector<QFuncBody>&& funcBodies);
    QIR_API QBlock* MakeQBlock(std::string&& debugText);

    QIR_API QType_Void* MakeVoidType();
    QIR_API QType_Ptr* MakePtrType();
    QIR_API QType_Struct* MakeBoolType();
    QIR_API QType_Struct* MakeIntType();
    QIR_API QType_Class* MakeStringType();
};

using QFactoryPtr = std::shared_ptr<QFactory>;

}