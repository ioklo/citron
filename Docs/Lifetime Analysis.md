%%TEST(ReturnLocalPointer, 4)%%
```
int* F(int* i)
{
    return i;
}

int x = 3;
var* y = F(&x);
*y = 4;

@$x
```

%%TEST(StructThisLifeTime, 4)%%
```
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
var* x = s.GetX();
*x = 4;

@${s.x}
```

%%TEST(CantReferenceEnumElemMemberVaraible, $Error)%%
```
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
```

%%TEST(StructMemberSequenceFunction, $Error)%%
```cs
// $Error()

struct S
{
    int x;

    // 이 함수의 리턴 값에는 this를 함유하게 되는데, lifetime이 같으므로 컴파일 가능하다
    seq int F()
    {
        yield this->x; // capture this(S*)
    }
}

void Main()
{
    local seq<int> sq;
    {
        S s = S(3);
        sq = s.F(); // 에러, local variable의 seq call, 상위 스코프로 assign 불가
                    // s.F()의 lifetime은 s와 같고, sq의 lifetime은 그보다 길어지므로 대입이 불가능하다
    }
}

```

%%TEST(PreventBox1, $Error)%%
```cs
struct S
{
    int x;

    seq int F()
    {
        yield this->x;
    }
}

void Main()
{
    S s = S(3);
    seq<int> sq = box s.F(); // 에러, box 불가
}
```

%%TEST(PreventBox2, $Error)%%
```
struct S
{
    seq int F(int* i)
    {
        yield *i;        
    }
}

void Main()
{
    int i = 6;
    S s = S();

    var sq = box s.F(&i); // 에러, ptr이 들어가는 seq call 값은 스코프를 벗어날 수 없다
}
```

%%TEST(PreventBox3, $Error)%%
```cs
struct S
{
    int x;

    int* F()
    {
        return &x;
    }
}

void Main()
{
    S s = S(3);
    var x = box s.F(); // 에러, s를 캡쳐해서 x에 넣을수 없으므로 불가
}
```

%%TEST(LocalPtrContainedLambda, $Error)%%
```cs
void Main()
{
    int i = 3;
    int* x = &i;

    var l = () => *x; // l은 local-ptr-contained
    var p = l;        // p는 local-ptr-contained, 전파

    var s = box p;   // 에러, p는 local을 갖고 있으므로 boxing 불가
}
```

%%TODO%%
```cs
// 무슨 변수를 contained했냐보다 lifetime을 갖고 가는것이 맞을 듯

void Main()
{
    lambda_value<int, int, 100> p;
    {
        int x = 3;
        {
            int y = 2;
            lambda_value<int, int, 100> q;

            if (true)
            {
                q = () => x;     // q: local-ptr-contained(x)
            }
            else
            {
                q = () => y;     // q: local-ptr-contained(y)
            }

            // q: local-ptr-contained(x, y)
            p = q; // 에러, p의 범위가 x, y 범위보다 넓음 local-ptr-contained(x, y)
        }
    }
}
```