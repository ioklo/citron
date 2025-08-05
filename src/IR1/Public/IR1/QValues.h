#pragma once

namespace Citron {

// 값을 지칭하는 구조
class QValue
{

};

class QValue_ConstBool : public QValue
{
    bool value;
public:
    QValue_ConstBool(bool value): value{value} { }
};

class QValue_ConstInteger : public QValue
{
    int value;
public:
    QValue_ConstInteger(int value) : value{value} { }
};

} // Citron