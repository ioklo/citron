#pragma once
#include "InfraConfig.h"
#include "IWriter.h"

#include <sstream>

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
    INFRA_API virtual void Write(const std::string& str) override;
    INFRA_API virtual void WriteLine() override;

    std::string ToString() { return oss.str(); }
};

}
