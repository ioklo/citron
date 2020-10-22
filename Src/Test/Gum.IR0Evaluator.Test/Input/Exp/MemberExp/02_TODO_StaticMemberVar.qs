// 11

class C
{
    public static int x = 0;
}

C.x = 1;
@${C.x}

var c = new C();
@${c.x}
