#include "MData.h"

using namespace std;

namespace Citron {

MData::MData(vector<MFuncBody>&& funcBodies)
    : funcBodies{move(funcBodies)}
{
}

} // namespace Citron