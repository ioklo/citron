export module Citron.SyntaxIR0Translator:DesignatedDiagnostic;

import <memory>;
import <functional>;

import Citron.Logger;
import Citron.Diag;

namespace Citron::SyntaxIR0Translator {

export class IDesignatedDiagnostic
{
public:
    virtual DiagPtr MakeDiag() = 0;
};

export template<typename TDiag>
struct DesignatedDiagnostic : public IDesignatedDiagnostic
{   
    std::function<DiagPtr ()> ctor;

    template<typename... TArgs>
    DesignatedDiagnostic(TArgs&&... args)
    {
        // stack에 들고 있도록 한다
        ctor = [&]() {
            return MakePtr<TDiag>(std::forward<TArgs>(args)...);
        };
    }

    DiagPtr MakeDiag() { return ctor(); }
};

} // namespace Citron::SyntaxIR0Translator