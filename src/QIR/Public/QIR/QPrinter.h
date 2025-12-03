#pragma once
#include "QIRConfig.h"

namespace Citron {

class QData;
class IWriter;
class QFactory;

QIR_API void PrintQData(QData* data, IWriter& writer, QFactory& qFactory);


} // namespace Citron