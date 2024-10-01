#pragma once

#include <functional>

namespace Citron {

namespace SyntaxIR0Translator {

class MemberDeclPhaseContext;

class SkeletonPhaseContext
{
public:
    void AddMemberDeclPhaseTask(std::function<void(MemberDeclPhaseContext&)> f);
};

} // SyntaxIR0Translator

} // Citron
