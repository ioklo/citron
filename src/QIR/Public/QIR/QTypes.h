#pragma once
namespace Citron {

struct QType {};

struct QType_Ptr : public QType
{
private:
    QType_Ptr() = default;
    friend class QFactory;
};

struct QType_Void : public QType 
{
private:
    QType_Void() = default;
    friend class QFactory;
};

struct QType_Primitive : public QType
{
private:
    QType_Primitive() = default;
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
