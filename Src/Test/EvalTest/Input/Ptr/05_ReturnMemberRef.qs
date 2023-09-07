// 05 
struct S
{
    int x;

    int* GetX()
    {
        return &x; // this의 라이프 타임
    }
}

var s = S(3);
var* x = GetX();

*x = 4;

@${s.x}