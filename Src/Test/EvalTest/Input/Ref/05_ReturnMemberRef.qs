// 05 
struct S
{
    int x;

    ref<int> GetX()
    {
        return ref x;
    }
}

var s = S(3);
var x = GetX();

x = 4;

@${s.x}