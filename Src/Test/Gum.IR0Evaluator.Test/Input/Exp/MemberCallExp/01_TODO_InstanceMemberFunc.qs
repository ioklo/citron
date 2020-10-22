// 2 4

// 5가지
// x.F(); X.F(); x.f(); X.f(); NS.f();

class X
{
    int x;

    public void F(int i)
    {
        @$x $i
    }

    public X(int x) { this.x = x; }
}

X x = new X(2);
x.F(4);