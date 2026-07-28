#pragma once
#include "InfraConfig.h"
#include <sstream>
#include "IWriter.h"

namespace Citron {

class StringWriter : public IWriter
{
    std::ostringstream oss;
    int indent;

public:
    INFRA_API StringWriter();
    INFRA_API virtual ~StringWriter() override;

    INFRA_API virtual void AddIndent() override;
    INFRA_API virtual void RemoveIndent() override;
    INFRA_API virtual void Write(std::string_view str) override;
    INFRA_API virtual void WriteLine() override;

    INFRA_API std::string ToString() { return oss.str(); }
};

}
