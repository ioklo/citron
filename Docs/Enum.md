# General

Discriminated Union, Algebraic Data Type
기존 C/C++의 enum과 비슷하지만, 멤버변수와 함께 넣을 수 있습니다.

```csharp
// 선언
enum Coord2D<T>
{
    Invalid,
    Rect(T x, T y),            // 기본 C syntax와 비슷한 느낌을 주려고 콤마로 구분합니다
    Polar(T radius, T angle),
}

Coord2D<int> m = .Rect(20, 30); // 타입힌트가 있어서 Coord2D<int>.Rect로 쓰지 않아도 됩니다

int GetLengthSq(Coord2D<int> m)
{
    if (m is .Rect)
        return m.x * m.x + m.y * m.y;

    else if (m is .Polar) 
        return m.radius;

    else if (m is .Invalid)
        return -1;

    ...
}	
```
`Invalid`, `Rect`, `Polar`를 Enum Case라고 합니다

# Enum Case들의 생성, 타입
enum case는 멤버변수가 없는 standalone 형식, 멤버변수가 있는 형식 두가지 형식이 있습니다. 두 형식 모두 생성시 enum case 타입이 아닌 부모 enum타입으로 생성합니다. var를 이용한 local variable선언시 에도 variable의 타입은 부모 enum 타입입니다. 이렇게 해야 다른 case를 대입하기 수월합니다. enum case타입을 직접 쓰는 경우는 패턴매칭 등에서 각각의 멤버변수에 접근해야 할 때 입니다.

standalone을 생성할땐 괄호 없이 그냥 써주면 됩니다.
```
enum E { First }
var e = E.First; // e는 E 타입입니다
```

멤버변수가 있는 타입은 함수처럼 인자를 주어서 생성합니다.
```
enum E { Second(int x) }
var e = E.Second(2); // e는 E 타입입니다
```

# Pattern Matching
switch, if를 사용해서 enum값을 경우에 맞게 사용할 수 있습니다

# Reference member variable (추후)
enum case의 멤버변수를 직접 참조하는 것은 안전하지 않습니다. 언제고 enum변수의 값이 다른 enum case로 설정될 수 있기 때문입니다. 

세가지 방식이 있을 수 있는데, 변경시 메모리는 살아있으므로, 내용물을 undefined라고 정의하는 것과, 특수한 reference를 써서, 같은 enum case인지 확인하고 값에 접근하도록 하는 방식, 아예 접근 불가능하도록 못박는 방식이 있을 것 같습니다.

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
```
enum E 
{
    First,
    Second(int x)
}

// local variable declaration의 initialization 부분
E e = .First;
e = .Second(2);

// 함수 인자
void F1(E e)
{
}

F1(.Second(3))

// 함수 리턴
E F2()
{
    return .First;
}

```
