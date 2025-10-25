#include "QFactory.h"
#include "QInsts.h"
#include "QValues.h"

using namespace std;

namespace Citron {

QInst_Load* QFactory::MakeQInst_Load(QValue* v, QValue* lv)
{
    auto inst = make_unique<QInst_Load>(v, lv);
    auto* pInst = inst.get();
    insts.push_back(move(inst));
    return pInst;
}

QInst_Store* QFactory::MakeQInst_Store(QValue* lv, QValue* v)
{
    auto inst = make_unique<QInst_Store>(lv, v);
    auto* pInst = inst.get();
    insts.push_back(move(inst));
    return pInst;
}

QValue_ConstBool* QFactory::MakeQValue_ConstBool(bool b)
{
    auto val = make_unique<QValue_ConstBool>(b);
    auto* pVal = val.get();
    values.push_back(move(val));
    return pVal;
}

QValue_ConstInteger* QFactory::MakeQValue_ConstInteger(int i)
{
    auto val = make_unique<QValue_ConstInteger>(i);
    auto* pVal = val.get();
    values.push_back(move(val));
    return pVal;
}
} // namespace Citron