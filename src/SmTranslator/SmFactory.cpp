#include "SmFactory.h"
#include <deque>
#include <optional>
#include "ImExp.h"
#include "IrExp.h"
#include "ReExp.h"
#include "SmType.h"

using namespace std;

namespace Citron {

struct SmFactoryPrivateData
{
    deque<SmType> types;
};

SmFactory::SmFactory()
    : privateData{make_unique<SmFactoryPrivateData>()}
{
}

SmFactory::~SmFactory()
{
}

SmType* SmFactory::MakeSmType(SmType&& type)
{
    return &privateData->types.emplace_back(move(type));
}


} // namespace Citron