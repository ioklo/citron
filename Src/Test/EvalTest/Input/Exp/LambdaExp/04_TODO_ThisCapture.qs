// 5

class X
{
    int x;

    // this만 쓸거면 box 안해도 된다.
    some func<int, int> Func()
    {
        // 여기서 x는 this.x이고, capture에는 this가 들어가야 한다
        return y => this.x + y;
    }
}

void Main() 
{
    var x = new X(4);
    var f = x.Func();
    var v = f(1);

    @$v
}