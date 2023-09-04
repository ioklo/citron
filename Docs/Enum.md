# General
Discriminated Union, Algebraic Data Type
기존 C/C++의 enum과 비슷하지만, 멤버변수와 함께 넣을 수 있습니다.

%%TEST(Enum_Complex, 1300)%%
```csharp
// 선언
enum Coord2D<T>
{
    Rect(T x, T y),            // 기본 C syntax와 비슷한 느낌을 주려고 콤마로 구분합니다
    Polar(T radius, T angle),
}

int GetLengthSq(Coord2D<int> m)
{
    if (m is .Rect)
        return m.x * m.x + m.y * m.y;

    else if (m is .Polar) 
        return m.radius * m.radius;
}

void Main()
{
    Coord2D<int> m = .Rect(20, 30); // 타입힌트가 있어서 Coord2D<int>.Rect로 쓰지 않아도 됩니다
    var lenSq = GetLengthSq(m);

    @$lenSq
}
```
`Rect`, `Polar`를 Enum Case라고 합니다

# Enum Case들의 생성, 타입
enum case는 멤버변수가 없는 standalone 형식, 멤버변수가 있는 형식 두가지 형식이 있습니다. 두 형식 모두 생성시 enum case 타입이 아닌 부모 enum타입으로 생성합니다. var를 이용한 local variable선언시 에도 variable의 타입은 부모 enum 타입입니다. 이렇게 해야 다른 case를 대입하기 수월합니다. enum case타입을 직접 쓰는 경우는 패턴매칭 등에서 각각의 멤버변수에 접근해야 할 때 입니다.

standalone을 생성할땐 괄호 없이 그냥 써주면 됩니다.
%%TEST(Enum_ConstructStandalone, )%%
```
enum E { First }

void Main()
{   
    var e = E.First; // e는 E 타입입니다
    
}
```

멤버변수가 있는 타입은 함수처럼 인자를 주어서 생성합니다.
%%TEST(Enum_ConstructWIthArgument, 2)%%
```
enum E { Second(int x) }

void Main()
{
    var e = E.Second(2); // e는 E 타입입니다
}
```

# Generics
%%TEST(Enum_Generics, Hi)%%
```
enum Option<T>
{
    None,
    Some(T value)
}

Option<int> i = None;
Option<string> s = Some("Hi");

if (s is Option<string>.Some some)
    @${some.value}

```

# Pattern Matching

## if test
`if (<exp> is <enum_case> <variable>?) { ... } ` 형식을 사용해서 패턴 매칭을 할 수 있습니다. `optional variable`은 테스트가 성공했을때 할당할 지역 변수입니다. 지역변수를 할당하지 않도록 생략 가능합니다

%%TEST(Enum_IfTest, true)%%
```
enum E { First, Second(int x) }

void Main()
{
    var e = E.First;

    if (e is E.First)
        @true
    else if (e is E.Second s)
        @{s.x}
}

```

## Switch test

다음과 같은 switch의 case 구문을 쓸 수 있습니다. 
%%NOTTEST%%
```
case <enum_case>:
case <enum_case>(var <case_member_var_name> | _, ...):
case <enum_case> <optional_var_name>:
```

첫번째 구문은 standalone일때거나, 멤버변수가 있지만 쓰지 않을경우 사용합니다. 

두번째는 enum case 멤버변수 각각에 대해서 이름을 붙여서 지역 변수를 생성하고 값을 복사해 올 수 있습니다. 필요하지 않은 멤버는 \_를 써서 생성하지 않게 할 수 있습니다.

세번째는 해당 enum case 전체를 지역 변수 하나로 복사해오고 싶을때 사용합니다.

로컬변수가 만들어지는 경우 모두 값을 복사 하게 됩니다.

%%TEST(Enum_SwitchTest, 2)%%
```
enum E { First, Second(int x, bool y), Third(string s) }
void Main()
{
    var e = E.Second(2);
    switch (e)
    {
        case E.First:
            @First
        
        case E.Second(var x, _):
            @$x
            
        case E.Third x:
            @${x.s}
    }
}
```

# Reference member variable (추후)
enum case의 멤버변수를 직접 참조하는 것은 안전하지 않습니다. 언제고 enum변수의 값이 다른 enum case로 설정될 수 있기 때문입니다. 

세가지 방식이 있을 수 있는데, 변경시 메모리는 살아있으므로, 내용물을 undefined라고 정의하는 것과, 특수한 reference를 써서, 같은 enum case인지 확인하고 값에 접근하도록 하는 방식, 아예 접근 불가능하도록 못박는 방식이 있을 것 같습니다.

%%TODO%%
```
enum E { First(int i), Second(string s) }

var e = First(3);
var* x = &((E.First)e).i;

e = Second("hi");

// undefined를 허용하면 3이 나올 가능성이 크지만 확실하진 않다
@ ${*x}

// Or, 컴파일러가 알아서 
var x = EnumCaseMemberPtr(lableof(E.First), e, E.First.x);
@ ${x.GetValue()} // 익셉션 발사 가능

```

# Aggregation (추후)

%%TODO%%
```csharp
enum Mammal
{
    Cat(int age),
    Dog,
    Invalid,
}

enum Bird
{
    Duck,
    Invalid,
}

// 이름이 겹치는 것이 없다면 상속리스트로 포함시킬 수 있습니다
enum Animal : Mammal, Bird // Invalid 때문에 에러가 납니다
{
}

enum Animal2 // Invalid는 쓸 수 있겠지만 조금 귀찮습니다
{
    M(Mammal m),
    B(Bird b)
}

enum Animal3
{
    Cat <= Mammal.Cat, // 같은 타입은 아니지만 변환이 가능
    Dog <= Mammal.Dog,
    Buck <= Bird.Duck
}

Animal2 animal2 = .B(.Duck);

Animal3 animal31 = .Cat(2);
Animal3 animal32 = Mammal.Cat(2); // 생성 후, conversion
```

# Enum Type Hint
expression의 타입이 enum타입인 것을 미리 알 수 있을 때, Enum의 case 중 하나를 사용하려고 하는 경우, Enum명을 생략하고 .부터 시작할 수 있습니다.

로컬변수 초기화, 대입의 값 부분, 함수 인자, 함수 리턴에서 사용할 수 있습니다
%%TEST(Enum_TypeHint, )%%
```
enum E 
{
    First,
    Second(int x)
}


// 함수 인자
void F1(E e)
{
}

E F2()
{
    return .First;
}

void Main()
{
    // 1. local variable declaration의 initialization 부분
    E e = .Second(2);

    // 2. 함수 인자
    F1(.First);
    
    // 3. 함수 리턴
    e = F2();
}

```
