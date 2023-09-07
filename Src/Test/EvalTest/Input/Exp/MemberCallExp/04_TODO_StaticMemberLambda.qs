// 23

class C
{
    public static func<int, void> F;
}

void Main()
{
    C.F = i => {
        @$i
    };


    C.F(2);

    var c = new C();
    c.F(3);
}