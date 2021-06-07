// 2

enum X
{
    First,
    Second (int i)
}

var x = X.Second(2);

@${x.i}
