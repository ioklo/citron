// 2

class C
{
    public (int => void) F;

    public C((int => void) F)
    {
        this.F = F;
    }
}

C c = new C(i => {
    @$i
});

c.F(2);