
# General  
값의 **위치**를 값으로 저장한 것을 포인터 라고 합니다. Citron에서는 두가지 형식의 pointer가 있습니다. 위치가 현재 함수의 실행 중에 유효한것이 보장된 pointer를 local pointer, 항상 살아있는(그 값을 가리키는 것이 전혀 없을때까지) 포인터를 box pointer라고 합니다. 의미상 box pointer는 local pointer로 변환할 수 있지만, 반대는 불가능합니다.

```csharp
int a = 0;
int* x = &a; // 1. local pointer
box int* y = box 5; // 2. box pointer

class C { public int x; }

var c = new C();

box int* z = &c.x; // 3. box pointer
```

## Notation
아무런 언급이 없다면 기본적으로 local pointer입니다. box pointer가 필요하면 앞에 box키워드를 붙여서 box pointer 임을 나타냅니다. 예외적으로, interface는 기본적으로 heap의 값들을 가리키고 있기 때문에 interface 앞에 local을 붙여서 interface가 가리키는 값이 local에 있음을 표시합니다.

# box pointer 타입의 멤버변수
`struct`, `enum`, `class`는 box pointer 타입의 멤버변수를 제약 없이 사용할 수 있습니다.

```cs

struct S { box int* i; }

enum E { First(box int* i), Second }

class C { box int* i; }
```

# local pointer 타입의 멤버변수

local pointer를 멤버변수로 사용하기 위해서는 `struct`,  `enum` 앞에 `local` 키워드를 붙여서 `local struct`, `local enum`으로 만들어 주어야 합니다. `local`이 앞에 붙은 값 타입은 지역에만 할당 가능하고, `box`가 불가능합니다.

```cs
local struct S { int* i; }

var x = 3;
var s = S(&x);

var bs = box S(&x); // 에러
```

# 인터페이스류의 local 선언

interface 타입 변수에 interface를 구현한 값타입 포인터를 가리키게 할 수 있습니다. interface는 기본적으로 힙에 있는 인스턴스를 가리키는 값 타입입니다. 따라서, 지역에 할당된 값을 가리키게 하기 위해서는 따로 표시가 필요합니다. 변수 선언시 타입에 local 키워드를 붙여서 interface가 local하게 쓰인다는 표시를 해줍니다.
  
```cs
interface I { }
struct S : I { }

void F(I i) { }
void G(local I i) { } // i는 함수 안에서만 쓰이도록 보장

// 1. 지역변수 s에 대하여

var s = S(); // S
F(&s); // 에러: I는 box 레퍼런스만 가리킬 수 있습니다
G(&s); // 가능: 지역변수의 레퍼런스이므로 캐스팅이 가능합니다.

// 2. heap bs
var bs = box S(); // box S* bs = box S();
F(bs); // 가능
G(bs); // 가능

```

  
이 규칙은 func<>, seq<>, task<>에도 적용할 수 있습니다

```csharp

void Run1(func<string, int> f) {}
void Run2(local func<string, int> f) {}
  

var f = (string x) => 0; // f는 func<string, int>를 따르는 anonymous lambda타입

Run1(&f); // 에러
Run2(&f); // 가능


Run2((string x) => 0) 

// 혹은 boxing해야 합니다
var bf = box (string x) => 0; 

Run1(bf); // 가능
Run2(bf); // 가능
  

// 편의성 측면에서 다음이 가능하도록 합니다

Run2((string x) => 0); // temporary value를 local func<>로 캐스팅 시켜줍니다.

// 다음은 불가합니다.
Run1((string x) => 0); // 힙할당은 명시적이어야 합니다. box (string x) => 0
```

# this타입에 따른 멤버함수 호출 오버라이드

box타입에서의 동작을 따로 정의하고 싶으면, 함수 매개변수 리스트 다음에 box키워드를 추가합니다

```cs

struct S
{
  int a;

  void F() // 1번 함수
  {
     var* x = &a; // a는 this->a이고, this는 S*이므로, x는 int& 
  }
  
  void F() box // 2번 함수
  {
     box var* x = &a; // a는 this->a이고, this는 box S*이므로, x는 box int*
  }
} 

var s = S(3);      // s의 타입은 S
var bs = box S(8); // bs의 타입은 box S*

s.F();  // 1번 함수가 호출됩니다
bs->F(); // 2번 함수가 호출됩니다

```

```cs
struct S
{
    void F() box { }
}

var s = S();
s.F(); // 에러, box
```
  

제너레이터를 사용할때 사용하면 코드에서 참조가 어떻게 잘못되었는지 확인이 수월합니다.

```csharp

struct S
{
  int x;

  local seq<int> F(int a) { yield x + a; } // 가능: x는 this->x이고, this는 S* 입니다.

  seq<int> F(int a) { yield x + a; }       // 에러: this는 로컬 S&인데, boxing seq<>로 참조하려고 했습니다

  seq<int> F(int a) box { yield x + a; }   // 가능: x는 this->x이고, this는 box S*입니다

}

var s = S(3);
var sq = s.F(4); // sq의 타입은 local seq<int>

var bs = box S(4);
var bsq = bs->F(4); // bsq는 seq<int>
```

  

# local pointer of reference type
 the content of local variable which has reference type is also value. so we can apply reference operator to the reference type local variable.
 
포인터를 사용해서 value type(struct, enum 등)의 value가 저장된 위치를 주로 가리키게 됩니다. 하지만 레ref type(class, interface)의 value도 instance의 위치를 가리키는 value이기 때문에 pointer가 가리킬 수 있는 대상이 될 수 있습니다. 함수의 결과를 리턴 값 이외에도 설정하고 싶은 경우에 쓸 수 있습니다.

```cs
class C { public int x; }

void Func(out C* c)
{
    *c = new C(2);
}

C c = new C(1);
Func(out &c); // 지역변수 c의 값이 변합니다
print(c.x); // 2가 출력됩니다

```

# Lifetime Analysis of the value
  
the location where a local pointer points to become not valid when local variable is out of scope. compiler must check the value has local pointer inside and their lifetime.

```cs
int* Func(int *a, int *b)
{
    int x = 0;
    
    // return &x; // 에러, 
    return a; // 함수 인자는 함수 호출과 생명주기가 같으므로 허용됩니다.
}

```

this process is applied not only the pointer variable directly but also value that has local pointer in it.

```csharp
local struct S { int* i; }

var x1 = 0;          // x1(lifetime 0), 0(lifetime 0)
var s1 = S(&x1);     // s1(lifetime 0), S(&x1)(lifetime 0)

{
    var x2 = 1;      // x2(lifetime 1), 1(lifetime 1)
    var s2 = S(&x2); // s2(lifetime 1), S(&x2) (lifetime 1)
                     // possible

    s1 = s2; // s1(lifetime 0), s2(lifetime 1)
             // error: s1 lives longer than s2
}

```


함수 범위 안쪽으로 따라 들어가서 context분석을 하기에는 한계가 있기 때문에, 보수적으로 접근합니다. 일반적인 경우는 아닐 것 같아서, 이런 경우에는 box 레퍼런스를 사용하도록 합니다.

```cs
int* F(int* x, int* y) { return x; }

void Main()
{
    int a = 0;         
    int* p = &a;       // var p(lifetime 0)
    {
        int b = 0;     
        p = F(&a, &b); // 에러, &a(lifetime 0), &b(lifetime 1), F(...) (lifetime 1), p (lifetime 0)
    }
    @${*p}
}

```

추후에는 life time analysis를 적용시킬 수도 있습니다
```csharp
// [l]이면 l라이프타임, [l1, l2]하면 둘중 더 짧은것을 선택
[l] int* F([l] int* x, int* y) { return x; }

void Main()
{
    int a = 0;
    int* p = &a;
    {
        int b = 0; 
        p = F(&a, &b); // &a (lifetime 0)
                       // &b (lifetime 1)
                       // F(&a, &b) (lifetime 0), 라이프타임을 미리 정해줘서 0으로
                       // p (var lifetime 0)이므로 p에 대입 가능
    }
    @${*p}
}

```

```cs
local struct S { [l] int *x; }

void Main()
{
    S s; // uninitialized, s(var lifetime 0)
    {
        int b = 0;
        s = S(&b); // &b (lifetime 1), S(&b) (lifetime 1), s(var lifetime 0)
                   // 에러
    }
}

```

# box pointer internal
  
box pointer가 가리키는 value가 가비지 컬렉션대상에서 제외되기 위해서는 box pointer가 유지되어야 할 instance를 갖고 있어야 합니다

```csharp

C c;
box int* x = &c.a; // x안에 c레퍼런스가 포함되어 있어야 합니다. (c, &c.a);
                   // box-ref-struct-member-exp(make-box-ref-value (local-loc("c")), "a")


var* s = box S();   // box-ref-value(holderS, <address>)
box int* x = &s->a; // box-ref-struct-member-exp(load-local-exp(local-loc("s")), "a")

// c, d는 클래스, t, s는 구조체
box int* x = &d.s.c.t.a; // x안에 c가 포함되어야 합니다. (d.s.c, t.a);
// box-struct-member(box-struct-member(box-loc(d.s.c), "t"), "a")


box S* s = box S();
box int* x = &s.t.a; // box-struct-member(box-struct-member(box-loc(local-loc(s)), "t"), "a")

```


