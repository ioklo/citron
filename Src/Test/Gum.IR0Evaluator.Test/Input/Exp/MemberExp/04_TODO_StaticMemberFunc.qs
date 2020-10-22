// 23

class C
{
    public static void F(int i)
    {
        @$i
    }
}

var f = C.F;

var c = new C();
var f2 = c.F;

f(2);
f2(3);
