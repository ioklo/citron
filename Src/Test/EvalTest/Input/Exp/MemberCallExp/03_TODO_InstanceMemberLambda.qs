// 2

class C
{
    public func<int, void> F;

    public C(func<int, void> F)
    {
        this.F = F;
    }
}

void Main()
{
    C c = new C(i => {
        @$i
    });

    c.F(2);
}
