// 5

class X
{
    int x;

    public X()
    {
        x = 4;
    }

    (int => int) Func()
    {
        // 여기서 x는 this.x이고, capture에는 this가 들어가야 한다
        return y => x + y;
    }
}

var f = Func();
var v = f(1);

@$v