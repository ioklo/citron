// 2

enum X
{
    First,
    Second (int i)
}

var x = X.First;
x = X.Second (2);

if (x is X.Second)
    @${x.i}
