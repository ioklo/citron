#include "QData.h"
#include "QFuncBody.h"

using namespace std;

namespace Citron {

QData::QData(vector<QFuncBody>&& bodies)
    : bodies(move(bodies))
{
}

span<QFuncBody> QData::GetAllBodies()
{
    return bodies;
}

} // namespace Citron