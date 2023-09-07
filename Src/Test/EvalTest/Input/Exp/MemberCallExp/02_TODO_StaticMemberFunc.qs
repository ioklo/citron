// 23

class C
{
    public static void F(int i)
    {
        @$i
    }
}

void Main()
{
    C.F(2);

    var c = new C();
    c.F(3);
}