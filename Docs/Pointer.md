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
# Local Pointer

## Local Pointer Type

기존 타입에 \*를 붙이면 local pointer 타입이 됩니다. 여러개를 붙여서 포인터의 포인터 타입을 만들 수 있습니다.
```cs
int* a;
int***** b;
```

var\*로 포인터 타입을 선언할 수 있습니다. 포인터 타입인데 `var`로 선언했다면 에러를 냅니다.

```csharp

var a = 0;  // a: int   int a = 0;
var* x = &a; // x: int&  int* x = local &a;

```

## Local Reference Value
local variable에 &를 붙여서 local pointer를 만들 수 있습니다. local variable의 수명만큼 유효합니다.
```csharp

int x = 0;
int* a = &x;

```

## Dereference Local Pointer
C와 비슷하게 \*을 붙여서 포인터를 값으로 바꿀 수 있습니다. Dereference 한 결과는 location입니다. 대입의 왼쪽에 사용할 수 있습니다. 

```csharp

int a = 0, b = 1;
int* x = &a;
*x = *x + 3; 

x = &b; // lifetime이 같으므로 변경 가능합니다

int* y = x; // x가 b를 가리키고 있었으므로, y는 b를 가리킵니다
```

  
# 레퍼런스 기본값


  

## conversion from box pointer
local 레퍼런스 변수에는 box 레퍼런스값을 넣을 수 있습니다. local과 box의 차이는 유효기간입니다. box 레퍼런스 값의 유효기간이 local보다 항상 길기 때문에 대입해도 문제가 생기지 않습니다.

  

box 레퍼런스 변수에 local 레퍼런스를 넣으면, 레퍼런스가 가리키는 지역 변수의 유효범위를 넘어서 사용할 수 있게 되므로, 대입할 수 없습니다.

  

```jsx

int* a = box 5;

  

int x = 0;

box int& b = x; // 에러, b는 x의 생명주기보다 깁니다.

```

  

# 함수에서의 레퍼런스

  

함수의 인자로 레퍼런스를 사용할 수 있습니다. 레퍼런스를 사용하면 함수 호출시 인자를 복사하는 오버헤드를 줄일 수 있습니다. 또한 함수를 호출하는 곳으로 값을 전달할 수도 있습니다.

  

```csharp

void Func(int& x)

{

x = 2; // 

}

  

int a = 0;

Func(ref a); // 지역변수 x를 가리키는 레퍼런스를 인자로 넘깁니다

  

print(a); // 2를 출력합니다

```

  

# 레퍼런스 타입의 멤버변수

  

`struct`, `enum`, `class`는 box 레퍼런스 타입의 멤버변수를 가질 수 있습니다. 

  

```jsx

struct S { box int& i; }

enum E { First(box int& i), Second }

class C { box int& i; }

```

  

local 레퍼런스를 넣기 위해서는 `struct`, `enum` 앞에 `local` 키워드를 붙여서 `local struct`, `local enum`으로 만들어 주어야 합니다. `local`이 앞에 붙은 값 타입은 지역에만 할당 가능하고, `box`가 불가능합니다.

  

```jsx

local struct S { int& i; }

  

var x = 0;

var s = S(x);

```

  

# 인터페이스류의 local선언

  

값 타입 구조체가 interface를 구현할 수 있고, 때때로 interface 타입 변수가 이 구조체를 가리키게 해야 할 수 있습니다. interface는 기본적으로 box(힙) 레퍼런스를 함유하는 값 타입입니다. 따라서, 지역에 할당된 값을 가리키게 하기 위해서는 따로 표시가 필요합니다. 변수 선언시 타입에 local 키워드를 붙여서 interface가 local하게 쓰일수 있다는 표시를 해줍니다.

  

```csharp

interface I { }

struct S : I { }

  

void F(I i) { }

void G(local I i) { } // i는 함수 안에서만 쓰이도록 보장

  

// 1. 지역변수 s에 대하여

var s = S(); // S

F(ref s); // 에러: I는 box 레퍼런스만 가리킬 수 있습니다

G(ref s); // 가능: 지역변수의 레퍼런스이므로 캐스팅이 가능합니다.

  

// 2. heap boxedS

var boxedS = box S(); // box S& boxedS = box S();

F(ref boxedS); // 가능:

G(ref boxedS); // 가능:

```

  

이 규칙은 func<>, seq<>에도 적용할 수 있습니다

  

```csharp

void Run1(func<string, int> f) {}

void Run2(local func<string, int> f)

  

var f = (string x) => 0; // f는 func<string, int>를 따르는 anonymous lambda타입

Run1(ref f); // 에러

Run2(ref f); // 가능

  

// 혹은 boxing해야 합니다

var boxedF = box (string x) => 0; 

Run1(ref boxedF); // 가능

Run2(ref boxedF); // 가능

  

// 혹은 자동

Run1((string x) => 0); // 자동 boxing

Run2((string x) => 0); // 자동 ref

```

  

# this타입에 따른 멤버함수 호출 오버라이드

  

box타입에서의 동작을 따로 정의하고 싶으면, 함수 매개변수 리스트 다음에 box키워드를 추가합니다

  

```csharp

struct S

{

  int a;

  

  void F() // 1번 함수

  { 

     var& x = a; // a는 this.a이고, this는 S&이므로, x는 int& 

  }

  

  void F() box // 2번 함수

  { 

    var& x = a; // a는 this.a이고, this는 box S&이므로, x는 box int&

  }

}

  

var s = S();      // s의 타입은 S

var bs = box S(); // bs의 타입은 box S&

  

s.F();  // 1번 함수가 호출됩니다

bs.F(); // 2번 함수가 호출됩니다

  

```

  

제너레이터를 사용할때 사용하면 코드에서 참조가 어떻게 잘못되었는지 확인이 수월합니다.

  

```csharp

struct S

{

  int x;

local seq<int> F(int a) { yield x + a; } // 가능: x는 this.x이고, this는 S& 입니다.

  seq<int> F(int a) { yield x + a; }       // 에러: this는 로컬 S&인데, boxing seq<>로 참조하려고 했습니다

  seq<int> F(int a) box { yield x + a; }   // 가능: x는 this.x이고, this는 box S&입니다

}

  

var s = S(3);

var sq = s.F(4); // sq의 타입은 local seq<int>

  

var bs = box S(4);

var bsq = s.F(4); // bsq는 seq<int>

```

  

# 레퍼런스 중심 타입의 레퍼런스

  

레퍼런스 중심 타입의 변수는 값 타입입니다. 클래스는 레퍼런스 타입이지만, 클래스 타입의 지역변수는 인스턴스를 가리키는 값 타입입니다. 따라서 레퍼런스를 적용할 수 있습니다.

  

```csharp

class C { public int x; }

  

void Func(C& c)

{

c = new C(2);

}

  

C c = new C(1);

Func(ref c); // 지역변수 c의 값이 변합니다

  

print(c.x); // 2가 출력됩니다

```

  

# 지역 레퍼런스 함유 분석 (생명주기분석)

  

지역변수의 레퍼런스는 지역변수의 범위가 벗어나면 더 이상 유효하지 않게됩니다. 따라서 가리키는 값의 생명주기를 따져서 대입이 가능한지, 리턴이 가능한지 등을 컴파일러가 확인해줘야 합니다.

  

```csharp

int& Func(int &a, int &b)

{

  int x = 0;

  

  // return &x; // 에러, 

return &a; // 함수 인자는 함수 호출과 생명주기가 같으므로 허용됩니다.

}

```

  

레퍼런스 변수가 직접 무엇을 가리키는지 보다는, 값이 생명주기가 얼마인 local 레퍼런스를 함유하고 있는지가 중요합니다.

  

```csharp

local struct S { local int& i; }

  

var x1 = 0;

var s1 = S(ref x1); // 가능: 최상위 '변수'에, 최상위 local 레퍼런스를 함유하는 '값' 대입

{

    var x2 = 1; 

    var s2 = S(ref x2); // 가능: 안쪽 스코프 범위의 '변수'에, 안쪽 스코프 범위의 local 레퍼런스를 함유하는 '값' 대입

  

    s1 = s2; // 에러: 최상위 '변수'에, 안쪽 스코프 범위의 local레퍼런스를 함유한 '값'을 대입하려고 합니다

}

```

  

함수 범위 안쪽으로 따라 들어가서 context분석을 하기에는 한계가 있기 때문에, 보수적으로 접근합니다. 일반적인 경우는 아닐 것 같아서, 이런 경우에는 box 레퍼런스를 사용하도록 합니다.

  

```csharp

local struct S { local int& i; }

S Func(int& x, int& y) { return S(inout x); }

  

int a = 0;

S s1 = S(ref a);

{

   int b = 0;

   S s2 = Func(ref a, ref b); // a가 선택될 것이 확실하지만, 함수에 가려서 보이지 않습니다. 대신 입력으로 들어간 a, b중 생명주기가 짧은 것을 선택합니다.

  

   s1 = s2; // 에러, 생명주기가 짧은 값을 함유하고 있어서 더 긴 생명주기의 변수에 값을 대입할 수 없습니다.

}

```

  

# 타입 변환

  

T와 T&는 서로 암시적으로 변환됩니다.

  

```csharp

int a = 0;  // a는 int

var& x = a; // x는 int&, a는 int에서 int&로 타입변환(local ref값을 얻어오는 작업)

int y = x;  // y는 int, x는 int&인데, int로 타입 변환(local ref에서 deref 하는 작업)

  

```

  

# box reference internal

  

box reference가 가리키는 storage가 가비지 컬렉션대상에서 제외되기 위해서는 box reference가 유지되어야 할 instance를 갖고 있어야 합니다.

  

```csharp

C c;

box int& x = c.a; // x안에 c레퍼런스가 포함되어 있어야 합니다. (c, &c.a);

                  // box-ref-struct-member-exp(make-box-ref-value (local-loc("c")), "a")

  

var& s = box S(); // box-ref-value(holderS, <address>)

box int& x = s.a; // box-ref-struct-member-exp(load-local-exp(local-loc("s")), "a")

  

// c, d는 클래스, t, s는 구조체

box int& x = d.s.c.t.a; // x안에 c가 포함되어야 합니다. (d.s.c, t.a);

  

// box-struct-member(box-struct-member(box-loc(d.s.c), "t"), "a")

  

box S& s;

box int& x = s.t.a; // box-struct-member(box-struct-member(box-loc(local-loc(s)), "t"), "a")

  

```


# Pointer of RefType
포인터를 사용해서 value type(struct, enum 등)의 value가 저장된 위치를 주로 가리키게 됩니다. 하지만 레ref type(class, interface)의 value도 instance의 위치를 가리키는 value이기 때문에 pointer가 가리킬 수 있는 대상이 될 수 있습니다. 함수의 결과를 리턴 값 이외에도 설정하고 싶은 경우에 쓸 수 있습니다.

```
class C { public int x; }

void F(out C* c)
{
    c = new C(3)

}

void Main()
{
    var c = new C(2);
    F(out &c); // c가 3인 값으로 덮어씌워진다

    @ ${c.x}
}

```


