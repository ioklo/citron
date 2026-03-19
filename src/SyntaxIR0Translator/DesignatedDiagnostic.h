#pragma once

#include <memory>
#include <functional>

#include "Logging/Logger.h"
#include "Logging/Diag.h"

namespace Citron {

class IDesignatedDiagnostic
{
public:
    virtual DiagPtr MakeDiag() = 0;
};

template<typename TDiag>
struct DesignatedDiagnostic : public IDesignatedDiagnostic
{   
    std::function<DiagPtr ()> ctor;

    template<typename... TArgs>
    DesignatedDiagnostic(TArgs&&... args)
    {
        // stack에 들고 있도록 한다
        ctor = [&]() {
            return std::shared_ptr<TDiag>(new TDiag(std::forward<TArgs>(args)...));
        };
    }

    DiagPtr MakeDiag() { return ctor(); }
};

} // namespace Citron