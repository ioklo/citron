// $Error
enum E
{
    First,
    Second(int x)
}

struct S
{
    E e;
    int y;

    int* GetX()
    {
        if (e is E.Second s)
            return &s.x;  // error, enum element는 레퍼런스의 대상이 될 수 없다.
        else 
            return &y;
    }

    void Mutate()
    {
        e = E.First;
    }
}

void Main()
{
    var s = S(Second(3), 7);
    var x = s.GetX();
    x.Mutate(); // e의 레이아웃을 흐트러 놓으셨다
    x = 3;
}