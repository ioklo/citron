<!--BEGIN_EMBED(Struct_Complex)-->
```cs
//@ 2 3 6 1
public struct B
{
    int a;
}

struct S : B
{
    int x; // default public
    private int y;

    int Sum() // default public
    {
        return a + x + y;
    }

    int GetY() 
    {
        return y;
    }
}

void Main()
{	
	var s1 = S(1, 2, 3); // a, x, y
    
    // 2
	@${s1.x}

    // 3
	@${s1.GetY()}

    // 6
	@${s1.Sum()}
    
    S s2 = uninit; // 미초기화 상태
	
	s2 = s1;                  // 복사 생성
	
	// 고급, shared
	shared S s3 = shared S(1, 2, 3);
	
	@${*s3.a}
	s2 = *s3;                 // 복사 대입
}
```
<!--END_EMBED-->

<!--BEGIN_EMBED(Struct_AutoTrivialConstructor)-->
```cs
//@ 2 3
// 2, 3
struct S
{
    int x;
    int y;
}

var s = new S(2, 3);
@${s.x} ${s.y}
```
<!--END_EMBED-->

# 선언

```csharp
// accessor STRUCT ID<ID...> (COLON TypeExp (COMMA TypeExp)...)
public struct S<T...> : B, I...
{
    // member variable 
    // Accessor VarType Id1, ...;
    T x; 
    private T y;

    // 1. default constructor
    S() { this.x = T(); this.y = T(); }

    // 2. memberwise constructor
    // S(...) = delete;

    // 3. copy constructor (special)
    special S([in] S& s) { this.x = s.x; this.y = s.y; }
    // S([in] S& s) = default;

    // 4. move constructor (special)
    // S([move] S& s) = default;

    // 5. copy assign (special)
    special void copy_assign([in] S& s) {...}

    // 6. move assign (special)
    // special void move_assign([move] S& s) {...}

    // 7. destructor
    ~S() { ... };
 
    // member functions, virtual not allowed
    // [accessor] [static] [seq] ReturnType Id<TypeArgs...>(Args...) { Body }
    static seq T Func<U>(U u) { return y; }
}
```

## 멤버 변수

```
T x;
private T y, z;
```

- access modifier를 앞에 붙이고, 멤버 변수의 타입, 같은 타입의 변수 이름들을 적습니다.

- public인 경우 따로 access modifier를 적지 않습니다 (적으면 에러)

- 타입 부분에 var를 지원하지 않습니다

## 생성자

### 기본 생성자
 - 아무것도 적지 않았을 경우, 기본값으로 초기화 하는 생성자 입니다
 - 자동으로 생성합니다. 모든 멤버가 기본생성자를 지원하지 않으면 생성에 실패하면서 에러를 냅니다. 에러를 우회하고 싶으면 = delete를 사용해서 명시적으로 기본 생성자를 지워야 합니다
```
S() = delete;
```

### 멤버와이즈 생성자
 - 모든 멤버에 직접 초기화 구문을 넣을 수 있는 생성자입니다.

```cs
struct S { int x; string y; }
string z = "hello";
S s = S(3, move z);
```
이 구문에서 S(3, move z); 는 다음과 같이 처리되도록 합니다

```cs
s.x = 3;      // s.x의 복사 생성자 호출
s.y = move z; // s.y의 이동 생성자 호출
```

 - 기본적으로 멤버와이즈 생성자를 지원합니다. 멤버와이즈 생성자를 지원하고 싶지 않으면, 직접 소스에 적어 줘야 합니다.
```
S(...) = delete;
``` 

 - 생성 구문이 멤버와이즈 생성자와 일반 생성자 모두 가능한 상황이면, 에러가 납니다. 멤버와이즈 생성자를 꺼서 해당 에러를 없앨 수 있습니다.

### 복사 생성자
```cs
special S([in] S& s) { ... } // 또는
S([in] S& s) = default;
```

 - 복사 초기화를 할때 사용하는 생성자입니다. 자동생성 하지 않습니다.
 - 초기화 표현이 같은 타입의 lvalue, talias가 오는 경우 복사 생성을 합니다.
```
S a = S();
S s = a; // 복사 생성자
```
 - 함수의 시그니처는 ```[in] S& s```입니다. 
 - 시그니처를 잘못 표기해서 복사생성자로 간주되지 않을 경우를 대비해서 ```special``` 키워드를 앞에 무조건 붙입니다. 
 - ```special```키워드는 지원하는 시그니처 종류들 중 하나가 아니라면 에러를 냅니다.
 - ```= default```를 사용해서 자동으로 만들 수 있습니다. 자동생성의 경우에는 ```special```의미가 내포되어 있으므로 키워드를 붙이지 않습니다.

### 이동 생성자
```cs
special S([move] S& s) { ... } // 또는
S([move] S& s) = default;
```
 - 이동 초기화를 할때 사용하는 생성자입니다
 - 초기화 표현이 같은 타입의 rvalue가 오는 경우 이동 생성을 합니다
```cs
S F() { ... }
S a = S();
S s1 = move a; // 이동 생성자
S s2 = F(); // 이동 생성자
```
 - ```special``` 키워드는 복사 생성자의 경우와 같습니다.

### 생성자에서의 대입
 - 생성자로 처음 진입할때 멤버변수들은 uninitialized 상태에 있습니다. 생성자가 끝날때 모든 멤버변수가 initialized 상태가 되도록 만들어야 합니다
 - uninitialized상태에서의 대입은 생성자로 변환됩니다. 좀더 명시적으로 하고 싶으면 expression에 init 키워드를 추가합니다.

```cs
struct T { int a, int b;}

struct S
{
    T x;

    // 생성자가 모든 멤버를 초기화 하지 않았기 때문에 에러
    // S()
    // {
    //    // this.x는 uninitialized 상태
    // }

    S()
    {
        x = T(1, 2); // x의 생성자 호출
    }

    S(bool cond)
    {
        // this.x 는 uninitialized 상태
        while(cond) // cond가 뭐가 될지는 모른다
        {
            // x = T(1, 2); 에러, 두가지로 해석될 수 있습니다. 1) uninitialized 상태에서 initialize, 2) x 대입 
            if (x is uninit)
                x = T(1, 2); 
            else
                x.a = 1 + x.a;
        }

        `initialized(x); // cond가 확실히 한번은 거쳐가는게 확실하다면 적어준다. 안 적어주면 에러
        // 또는 
        // if (x is uninit)
        //    x = T(1, 2);
    }
}
```


## 대입 함수
### 복사 대입 연산자

 - 복사 대입 연산을 최적화하고 싶을 경우 작성합니다. 복사 대입이 일어날때 복사 대입 연산자가 없으면 소멸자를 호출하고 복사 생성자를 다시 부르는 방식으로 대체됩니다.
 - 복사 대입을 가지고 있는 리소스를 해제하지 않고 재사용하면서 만들 수 있습니다.
 - ```= default``` 자동생성이 의미가 없어서 지원하지 않습니다.
```cs
special void copy_assign([in] S& s) {...}
```
 - ```special```키워드는 복사 생성자의 경우와 같습니다.

```cs
struct T { int x; }
struct S 
{
    box T t;

    S() { t = box T(3); }

    special void copy_assign([in] S& s) 
    {
        *t = *s.t; // 기존 box T를 재사용해서 복사
    }
}
``` 

### 이동 대입
 - 이동 대입연산을 최적화하고 싶을 경우 작성합니다. 이동 대입이 일어날 때 이동 대입 연산자가 없으면 소멸자를 호출하고 이동 생성자를 다시 부르는 방식으로 대체됩니다.
 - 이동 대입을 가지고 있는 리소스를 해제하지 않고 재사용하면서 만들 수 있습니다.
 - ```= default``` 자동생성이 의미가 없어서 지원하지 않습니다.
 - ```special``` 키워드는 복사 생성자의 경우와 같습니다

# 사용

## 생성

```cs
var s1 = S(1, 2);     // S type, scoped
var s2 = box S(3, 4); // box S type
```

## 멤버 변수 읽기, 쓰기

```cs
var s1 = S(1, 2);
assert(s1.x == 1);

var s2 = box S(3, 4);
assert(s2->x == 3);
```

## 참조, 복사

```cs
var s1 = S(1, 2);               // S 타입, scoped
shared var s2 = shared S(3, 4); // shared S 타입

var s3 = s1;                    // S 타입, 복사
box var s4 = box s1;            // box S 타입, 복사
shared var s5 = s2;

S& s6 = s3;                     // scoped reference, var& s6 = s3;
S* s7 = &s3;                    // get unsafe pointer
S* s8 = s7;

s3.x = 6;
assert(s1.x == 1 && s3.x == 6);

s4->x = 7;
assert(s1.x == 1 && s4->x == 7);

s5->x = 8;
assert(s2->x == 8 && s5->x == 8);

s6.x = 9;
assert(s3.x == 9 && s6.x == 9);

s7->x = 10;
assert(s3.x == 10 && s6.x == 10 && s7->x == 10 && s8->x == 10);
```
