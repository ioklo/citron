# General
지역 범위에 할당된 값을 가리키는 값을 지역 포인터라고 합니다.
# 지역 포인터 타입 만들기

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

# Local Pointer Value
local variable에 &를 붙여서 local pointer를 만들 수 있습니다. local variable의 수명만큼 유효합니다.
```csharp

int x = 0;
int* a = &x;

```

# Dereference Local Pointer
C와 비슷하게 \*을 붙여서 포인터를 값으로 바꿀 수 있습니다. Dereference 한 결과는 location입니다. 대입의 왼쪽에 사용할 수 있습니다. 

```csharp

int a = 0, b = 1;
int* x = &a;
*x = *x + 3; 

x = &b; // lifetime이 같으므로 변경 가능합니다

int* y = x; // x가 b를 가리키고 있었으므로, y는 b를 가리킵니다
```

# converting box pointer to local pointer
함수 호출시, local pointer 인자에 box pointer를 사용할 수 있습니다. 함수 호출의 인자로만 사용되어야 하는 이유는 local pointer의 유효기간보다 box pointer의 유효기간이 길어야 한다는 전제가 있어야 하기 때문입니다.. local pointer 유효기간 안에 box pointer가 가리키는 대상이 변경되어서도 안됩니다. 

```
void Func(int* x) { *x = 4; }
box int* a = box 3;

Func(a); // implicit conversion (box int* -> int*)
```
# converting local pointer to box pointer
local pointer를 box pointer로 만들게 되면, local pointer가 가리키는 대상의 범위를 벗어나서 사용할 수 있게 되므로 불가능합니다
```
class C { public box int* p; }
int a = 3;
int* x = &a;

var c = new C(x); // 오류.
```
