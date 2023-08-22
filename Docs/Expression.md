# Value
value는 데이터입니다. value는 type으로 대표됩니다. 같은 type의 value는 value를 저장하기 위해 필요한 메모리의 크기가 같습니다. value는 자체로 저장될 위치를 갖고 있지 않습니다. value를 만들어내는 expression이 쓰이는 위치에 따라 value이 저장되는 행동이 결정됩니다.
```
int i;
i = 3;

var c = new C();
c.x = 3;
```
value `3`은 저장될 위치를 갖고 있지 않습니다. `3`이 `int i = ` 옆에 온 경우에는 stack에,  `c.x = ` 옆에 온 경우에는 heap에 저장됩니다.
# Location
value가 저장될 위치(storage)를 나타냅니다. 대입 대상으로 쓸 수 있습니다. 최종 위치는 stack, heap, static 중 하나가 됩니다.
# Expression
expression은 syntax는 똑같아도, 쓰이는 위치에 따라 value가 될 수도, location이 될 수도 있습니다.
```
int i = 0; // 지역 변수 i에 대하여 
i = i;     // 대입
```
위의 두번째 줄 대입 구문에서 첫번째 `i`는 location을 나타내고, 두번째 `i`는 value를 나타냅니다.

# Local Scope
## Local Variable
### General
현재 stack frame위의 위치를 가리키고 있는 variable을 local variable이라고 합니다.

### Declare Local Variable 
다음과 같은 방식으로 local variable을 정의할 수 있습니다
```
<LOCALVARDECL> = <TYPEEXP> <VARLIST> ';'

<VARLIST> = <VARITEM>
          | <VARITEM> ',' <VARLIST>

<VARITEM> = <ID>               // uninitialized
          | <ID> '=' <EXP>     // with initiailization

```

Type Expression관련은 [TypeExpression](Type.md) 을 참조하세요. 아래는 예시입니다.
```
void Main()
{
    int x = 0;
    @$x
}
```
### 초기화 구문 생략
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

### Local Variable Type Inference
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



## Temp Variable
복잡한 expression을 계산하기 위해서는 variable 하나로는 부족할 수 있습니다. sub expression이 value로 평가되고 어딘가에 잠시 저장되어야 할 때, 컴파일러는 이름없는 temp variable을 스택에 만들어서 그 곳에 값을 저장합니다.

temp variable은 현 stack frame 위의 위치를 가리키기 때문에 이름없는 local variable이라고 할 수 있습니다.
temp variable은 해당 expression이 속한 statement가 끝나면 소멸됩니다. 따라서 temp variable을 참조하는 local pointer는 만들 수 없습니다. 단, 함수 호출시 인자로 넘어가는 local pointer는 만들 수 있습니다.

```
int x = F() + G() + H();
```
`F() + G() + H()`를 계산하기 위해서 컴파일러는 네개의 temp variable을 만들고 다음 순서로 실행합니다
```
int t0 = F();
int t1 = G();
int t2 = t0 + t2;
int t3 = H();
int x = t2 + t3;
```
마지막 부분은 `x`에 직접 대입할 것이므로 temp variable이 따로 필요하지 않습니다

## Nested Local Scope
`{ }` 를 사용해서 Local Scope안에 임의로 Local Scope를 추가할 수 있습니다 
```cs
void Main()
{
    int x = 0;
    {
        int x = 0;
        x = 3;
    }
    @$x
}
```
위의 코드는 0을 출력합니다.

scope를 추가하고 같은 이름의 변수를 만들면, 상위 범위에 있는 이름이 가려집니다. 이름이 같은 두 변수는 다른 위치를 가리키고 있습니다. 

`{ }` 뿐 아니라, `if`, `for`,  `while` , `foreach` 등은 본문에 Local Scope를 추가합니다.
