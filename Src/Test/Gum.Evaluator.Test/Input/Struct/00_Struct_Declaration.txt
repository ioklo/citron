public struct B<T> // default inherit from Object
{
    protected T a;
    protected T GetA() { return a; }
}

struct S<T> : B<T>
    where T has int operator+(T, int) // constraint
{
    int x; // default public
    protected int y;
    private int z;

    // No custom constructor allowed    
    // using static function
    static S<T> Make()
    {
    }

    int Sum() // default public
    {
        return GetA() + x + y + z;
    }

    int GetZ() 
    {
        return z;
    }
}

// operator definition, 
public S<T> operator+<T>(S<T>& x, S<T>& y)
{
    return new S<T>(x.a + y.a, x.x + y.x, x.y + y.y, x.z + y.z); // can access private members
}

// 기본 기능
var s = new S(2, 3);

// 1. 
Assert(s.x == 2);

// 2. 
Assert(s.y == 2);


// 일단 다 적고 나중에 분리
{
    var s = new S<int>(1, 2, 3, 4); // a, x, y, z
    Assert(s.x == 2);
    Assert(s.GetZ() == 4);
    Assert(s.Sum() == 10);
}

// 생성
{
    S s1 = new S(2, 3, 4); // 생성, 오버라이드 불가능
    S s2 = s1;             // 복사 생성, 오버라이드 불가능
}


// 고급, box, boxed 타입 S*
var s = box S(2, 3, 4);

// 힙에 생성
S* s3 = box S(2, 3, 4);

// 힙에서 참조 ->가 아님
s3.x
s2 = s3; // S <- S* 복사

// Scoped or Heap 참조
S& s10 = s1; // Scoped, 가능
S& s11 = s3; // Heap, 가능 
S& s12 = S(2, 3, 4); // 

// 힙참조
S* p = box S(2, 3);
S* s = p;
S* e = s1; // S* <- S 불가능, box s1;
B* b = p; // B* <- S* 가능 

// casting, emulating boxing
Object o = s3; // Object <- S* 가능
Object o = s1; // Object <- S 불가능, box S(s);
Object o = new S(s1); // 복사생성자

s.Func();
