#pragma once
#include "QIRConfig.h"

namespace Citron {

class QData;
class IWriter;
class RFactory;

QIR_API void PrintQData(QData* data, IWriter& writer, RFactory& rFactory);


} // namespace Citron