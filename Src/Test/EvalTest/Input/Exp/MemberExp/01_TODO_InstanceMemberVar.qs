// 2

class X
{
    public int x;    
    public X(int x) { this.x = x; }
}

void Main()
{
    X x = new X(2);
    @${x.x}
}