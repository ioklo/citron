export module Citron.RLoc;

import <variant>;

namespace Citron {

export using RLoc = std::variant<
    class RTempLoc,
    class RLocalVarLoc,
    class RLambdaMemberVarLoc,    
    class ListIndexerLoc,
    class StructMemberLoc,
    class ClassMemberLoc,
    class EnumElemMemberLoc,
    class ThisLoc,
    class LocalDerefLoc,
    class BoxDerefLoc,
    class NullableValueLoc
>;

// 임시 값을 만들어서 Exp를 실행해서 대입해주는 역할
class RTempLoc
{
    RExp exp;
    MType type;
};

class RLocalVarLoc
{
    MName Name;
};

// only this member allowed, so no need this
class RLambdaMemberVarLoc
{
    MLambdaMemberVar memberVar;
};

// l[b], l is list    
class RListIndexerLoc
{
    RLoc list;
    RLoc index;
};

// Instance가 null이면 static
class RStructMemberLoc
{
    std::optional<RLoc> instance;
    MStructMemberVar memberVar;
};

class RClassMemberLoc
{
    std::optional<RLoc> instance;
    MClassMemberVar memberVar;
};

class REnumElemMemberLoc
{
    RLoc instance;
    MEnumElemMemberVar memberVar;
};

class RThisLoc
{
};

// dereference pointer, *
class RLocalDerefLoc
{
    RLoc innerLoc;
};

// dereference box pointer, *
class RBoxDerefLoc
{
    RLoc innerLoc;
};

// nullable value에서 value를 가져온다
class RNullableValueLoc
{
    RLoc loc;
};

}

