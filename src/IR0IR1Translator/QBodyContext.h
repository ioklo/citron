#pragma once

#include <string>
#include "QIR/QFactory.h"
#include "QIR/QBlock.h"

namespace Citron {

class MExp;

class QValue;
class QValue_Named;
enum class QInst_IntrinsicKind;

namespace IR0IR1Translator {

class QBodyContext
{
    QBlock* curBlock;
    std::vector<QBlock*> blocks;
    QFactoryPtr qFactory;

public:  
    
    QBlock* AddBlock(const std::string& debugText);
    void SetCurBlock(QBlock* block) { curBlock = block; }

    template<typename TQInst, typename... TArgs> 
        requires std::derived_from<TQInst, QInst> and !std::same_as<TQInst, QInst_Intrinsic>
    void Add(TQInst&& inst)
    {
        curBlock->AddInst(std::move(inst));
    }

    QValue AddIntrinsic(QInst_IntrinsicKind kind, std::vector<QValue>&& args);

    void SetTerminator(QJumpInst&& jumpInst)
    {   
        curBlock->SetTerminator(std::move(jumpInst));
    }

    QValue_Named NewValue();

    size_t GetExpTypeSize(MExp* exp);

};

} // namespace IR0IR1Translator


} // namespace Citron