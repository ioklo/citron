// 5

class X
{
    int x;

    func<int, int> Func()
    {
        // 여기서 x는 this.x이고, capture에는 this가 들어가야 한다
        return y => x + y;
    }
}

var x = new X(4);
var f = x.Func();
var v = f(1);

@$v