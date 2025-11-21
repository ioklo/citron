#pragma once
#include "QIRConfig.h"

namespace Citron {

class QData;
class IWriter;

QIR_API void PrintQData(QData* data, IWriter& writer);


} // namespace Citron