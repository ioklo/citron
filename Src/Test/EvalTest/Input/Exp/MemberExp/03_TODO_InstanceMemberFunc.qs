// 2

class C
{
    public int x;

    public void F()
    {
        @$x
    }

    public C(int x)
    {
        this.x = x;
    }
}

void Main()
{
    var c = new C(2);
    var f = c.F;
    f();
}