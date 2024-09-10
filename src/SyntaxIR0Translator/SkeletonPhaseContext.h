#pragma once

#include <functional>

namespace Citron {

class MemberDeclPhaseContext;

class SkeletonPhaseContext
{
public:
    void AddMemberDeclPhaseTask(std::function<void (MemberDeclPhaseContext&)> f);
};


}