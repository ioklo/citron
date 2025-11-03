#include "QBodyContext.h"
#include "QIR/QInsts.h"
#include "QIR/QValues.h"

using namespace std;

namespace Citron::IR0IR1Translator {

QValue QBodyContext::AddIntrinsic(QInst_IntrinsicKind kind, vector<QValue>&& args)
{
    auto lv = NewValue();
    curBlock->AddInst(QInst_Intrinsic{kind, lv, std::move(args)});

    return lv;
}

} // Citron::IR0IR1Translator
