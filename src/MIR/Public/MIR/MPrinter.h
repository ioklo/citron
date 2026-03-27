#pragma once
#include "MIRConfig.h"

namespace Citron {

class MData;
class IWriter;
class RFactory;

MIR_API void PrintMData(MData* data, IWriter& writer, RFactory& rFactory);

} // namespace Citron
