#pragma once
#include <string>

namespace Citron {

class QValue;

class QInst
{
public:
    virtual ~QInst() { }
};

class QJumpInst
{
public:
    virtual ~QJumpInst() { }
};

// QLocalVar(lv, name)
class QInst_LocalVarDecl : public QInst
{
    QValue* loc;
    std::string name;
};

// QInst_Store(lv, v)
class QInst_Store : public QInst
{
    QValue* loc;
    QValue* value;

public:
    QInst_Store(QValue* loc, QValue* value)
        : loc{loc}, value{value} { }
};

// QInst_Load(v, lv)
class QInst_Load : public QInst
{
    QValue* value;
    QValue* loc;

public:
    QInst_Load(QValue* value, QValue* loc)
        : value{value}, loc{loc} {}
};

class QJumpInst_CondJump : public QInst, public QJumpInst
{

};

class QJumpInst_Jump : public QInst, public QJumpInst
{

};


} // namespace Citron