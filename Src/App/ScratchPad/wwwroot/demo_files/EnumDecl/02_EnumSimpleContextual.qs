// 2

enum E
{
    One,
    Two,
    Three
}

E Func()
{
    return Three;
}

E e = One;
e = Two;


if (e is E.Two) // TODO
    @2