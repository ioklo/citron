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

## pattern matching

`if (expr is pattern)` 및 `switch(expr) { case pattern { ... } }` 형태로 패턴 매칭을 제공한다.

```
if (e is not_null(v)) ... 
if (e is null)
if (e is C)
if (e is C c)
if (e is E.First(v, _))
if (e is E.First) // tag만 보기
if (e is E.Second) // standalone인 경우

switch(e) {
    case not_null(v) { ... }
    case null { ... }
    case C { ... }
    case C c { ... }
    case E.First(v, _) { }
    case E.First { }
    case E.Second { }
}
```

### pattern 형태
- `_` : wildcard (항상 매칭, 바인딩 없음)
- `null` : nullable이 null인지 검사
- `not_null(p)` : nullable이 null이 아님을 검사하고 내부 값을 `p`로 바인딩  
  - `p`는 보통 식별자(예: `v`) 또는 `_`를 사용한다.
- 타입 패턴: `C` / `C c`
  - `C`는 타입 테스트만, `C c`는 타입 테스트 + 전체 값을 `c`로 바인딩
- enum/variant 패턴:
  - `E.First` : tag만 검사 (payload는 무시)
  - `E.First(p1, p2, ...)` : tag 검사 + payload를 각 sub-pattern으로 매칭/바인딩
  - standalone(unit) variant는 `E.Second`처럼 tag 검사만으로 충분하다

`E.Tag`(tag-only)는 `E.Tag(_, _, ...)`의 축약으로 취급한다.

### 바인딩(식별자)의 의미
- 패턴에서 생성되는 식별자 바인딩은 기본적으로 **talias**로 취급한다.  
  즉, 원본 값의 “일부/뷰”를 이름으로 지칭하는 것이다.
- `if`에서는 바인딩이 then/else의 조건 계산에 섞이지 않는다.  
  예: `if (e is C c && c.x > 0)` 형태는 지원하지 않으며, 중첩 `if`로 표현한다.

### 평가/스코프 규칙
- `expr`(scrutinee)은 정확히 1회 평가된다.
- `if (expr is pattern)`에서 바인딩 변수는 then 블록 내부에서만 유효하다.
- `switch(expr)`는 위에서 아래 순서대로 case를 시도하며, 최초로 매칭된 case 블록이 실행된다. (fallthrough 없음)
- guard/when(추가 조건)은 제공하지 않는다. 필요한 조건은 case 블록 내부에서 `if`로 처리한다.

