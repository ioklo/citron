// 2

enum X
{
    First(int i),
    Second(string s)
}

X x = First(1);
x = Second("2");

if (x is X.Second second)
    @${second.s}
