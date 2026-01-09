# Citron 개념 모음

## lvalue
  값이 들어갈 위치를 지칭하는 표현식
  - 지역변수

## rvalue

본문의 표현식이 값 자체를 지칭할때 쓴다
  - 함수 표현의 결과 F()
  - move a (a는 지역변수)
  - 자체로 storage를 갖지 않고, rvalue를 사용하는 쪽에서 공간을 마련한다

## talias

Temporary Alias의 줄임말로, pattern matching등으로 내부의 값을 이름으로 지칭하는 것이다
외부 값이 변경되었을때 invalid되면서 접근 불가능해지도록 한다

```cpp
int? a = 3;
if (a is not_null(v)) // 여기서 v가 talias
{
    a = null;
    // 이후로 v를 쓸수 없다
}
```

## local reference 

scope내에서 lvalue를 지칭하는 객체


## Memberwise constructor

struct와 enum elem에 constructor를 별도로 만들지 않아도 멤버별로 값을 넣어서 생성할 수 있는 기능

```cpp
struct S
{
    int i;
    bool b;
}

var s = S(2, false);

enum E
{
    First,
    Second(int, bool)
}

var e = E.Second(2, false);
```

lvalue, rvalue, talias를 모두 받을 수 있다. 
함수를 거치지 않고, 직접 대입하는 효과가 있다

```
bool F() { return false; }

struct S
{
    int i;
    bool b;
}

var s = S(2, F()); 
// s.i = 2; (초기화)
// s.b = F(); (초기화) 와 같은 효과를 가진다
```

컴파일러 내부적으로는 직접 초기화를 하지만, 생성자 matching을 해야하기 때문에 memberwise-constructor 시그니처가 존재한다. [init] parameter kind를 사용한다

```
struct S
{
    int i;
    T t;
    // memberwise 생성자의 시그니처는 다음과 같다. [init]은 사용자가 직접적으로 쓸수 없다. 컴파일러 전용.
    // S([init] int& i, [init] T& t); 
}
```