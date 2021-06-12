// 2

enum E
{
    One,
    Two,
    Three
}

E e = E.One;
e = E.Two;

if (e is E.Two)
    @2