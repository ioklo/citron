#pragma once
namespace Citron {

struct QType {};

struct QType_Void : public QType 
{
private:
    QType_Void() = default;
    friend class QFactory;
};

struct QType_Struct : public QType
{
private:
    QType_Struct() = default;
    friend class QFactory;
};

struct QType_Class : public QType
{
private:
    QType_Class() = default;
    friend class QFactory;
};

} // namespace Citron
