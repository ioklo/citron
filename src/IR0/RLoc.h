#pragma once
#include <variant>
#include <memory>

namespace Citron {

class RTempLoc;
class RLocalVarLoc;
class RLambdaMemberVarLoc;
class RListIndexerLoc;
class RStructMemberLoc;
class RClassMemberLoc;
class REnumElemMemberLoc;
class RThisLoc;
class RLocalDerefLoc;
class RBoxDerefLoc;
class RNullableValueLoc;

class RLoc
{
};


using RLoc = std::variant<

>;

}

