#include "SmFactory.h"
#include <deque>
#include <optional>
#include "ImExp.h"
#include "IrExp.h"
#include "ReExp.h"

using namespace std;

namespace Citron {

struct SmFactoryPrivateData
{
};

SmFactory::SmFactory()
    : privateData{make_unique<SmFactoryPrivateData>()}
{
}

SmFactory::~SmFactory()
{
}

} // namespace Citron