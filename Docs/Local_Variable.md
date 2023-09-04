# General
현재 stack frame위의 위치를 가리키고 있는 variable을 local variable이라고 합니다.

# Declare Local Variable 
함수 본문에서 local variable을 정의하려면, 시작부분에는 정의할 변수들의 타입을 지정합니다. 그리고 정의할 변수들의 이름들을 적습니다. 변수 이름 옆에 `=`을 붙이고 식을 적어주면, 해당이름의 변수는 적어준 식으로 초기화를 합니다.
```
<LOCALVARDECL> = <TYPEEXP> <VARLIST> ';'

<VARLIST> = <VARITEM>
          | <VARITEM> ',' <VARLIST>

<VARITEM> = <ID>               // uninitialized
          | <ID> '=' <EXP>     // with initiailization

```
Type Expression관련은 [TypeExpression](Type.md) 을 참조하세요. 

```
void Main()
{
    int x = 0;
    @$x
}
```
## 초기화 구문 생략
초기화 구문은 생략 가능합니다. 다만 모든 경로에서 대입이 일어나기 전까지는 사용이 불가능합니다 자세한 사항은 uninitialized value analysis 부분을 참조하세요

```c
bool F()
{
    return true;
}

void Main()
{
    int x;
    
    if (F()) { x = 3; } // 심지어 F()가 항상 true를 리턴해도

    // 에러, x가 초기화 되지 않았습니다
    @$x 
}
```

## Local Variable Type Inference
타입 부분에 `var`를 사용해서 타입을 직접 명시 하지 않을 수 있습니다. 해당 변수의 타입은 초기화 식의 타입을 그대로 사용합니다. 
```c
void Main()
{
    var x = 3; // x는 int 타입입니다
    @$x
}
```

따라서 초기화 식이 없으면 에러가 납니다.
```
void Main()
{
    var x; // 에러
}
```

실수를 방지하기 위해 최종 타입이 local pointer, box pointer, nullable인 타입은 var 단독으로 사용해서 유추할 수 없습니다. 대신 `var*`, `box var*`, `var?` 를 사용합니다
```
void Main()
{
    var i = 3;
    var* x = &i;
    box var* y = box 3;
    var? optI = (int?)null;
}
```

```
void Main()
{
    var i = 3;
    var x = &i; // 에러
    var y = box 3; // 에러
    var optI = (int?)null; // 에러
}
```

여러 변수를 `var`타입으로 선언하면, 각각 타입을 유추하게 됩니다.
```
void Main()
{
    var x = 0, y = "hi";
    @$x, $y
}
```

local pointer나 box pointer와 함께 var를 쓴 경우, 여러 변수들은 각각 local pointer나 box pointer로 유추하게 됩니다.
```
void Main()
{
    int a = 0
    string b = "hi"
    var* x = &a, y = &b;
    @${*x}, ${*y}
}
```

```
void Main()
{
    int a = 0;
    var* x = &a, y = 3; // 에러
}
```

%%TEST(Decl, 21)%%
```cs
void Func()
{
    int x = 2;
    @$x
}

void Main()
{
    int x = 1;
    Func();
    @$x
}
```

# Temp Variable
복잡한 expression을 계산하기 위해서는 최종 결과 하나로는 부족할 수 있습니다. sub expression이 value로 평가되고 어딘가에 잠시 저장되어야 할 때, 컴파일러는 이름없는 temp variable을 스택에 만들어서 그 곳에 값을 저장합니다.

temp variable은 현 stack frame 위의 위치를 가리키기 때문에 이름없는 local variable이라고 할 수 있습니다.
temp variable은 해당 expression이 속한 statement가 끝나면 소멸됩니다. 따라서 temp variable을 참조하는 local pointer는 만들 수 없습니다. 단, 함수 호출시 인자로 넘어가는 local pointer는 만들 수 있습니다.

```
int x = F() + G() + H();
```
`F() + G() + H()`를 계산하기 위해서 컴파일러는 네개의 temp variable(t0 ~ t3)을 만들고 다음 순서로 실행합니다
```
int t0 = F();
int t1 = G();
int t2 = t0 + t1;
int t3 = H();
int x = t2 + t3;
```
마지막 부분은 `x`에 직접 대입할 것이므로 temp variable이 따로 필요하지 않습니다