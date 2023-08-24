# Use tuple as function arguments

```csharp
void F1(int i, string s, bool b)
{
}

var s = (1, "hi", false);

F1(params s); // F1(s.Item1, s.Item2, s.Item3);
```

variadic arguments

```csharp
void print(int i) { ... }
void print(string s) { ... }
void print(bool b) { ... }

void print<THead, TRest>(THead hd, params TRest rest) // 인자가 두개 이상 있을 때만 매칭
{
    print(hd);
    print(rest);
}

print(2, "hi", false); // template<int, (string, bool)> print(2, ("hi, false));
```
## out Parameter
함수 호출 시에 넘긴 인자가 결과값 용도로 쓰인다는 것을  인자에 `out` 키워드를 쓰도록 강제할 수 있습니다. 함수 정의 시 파라미터에 `out`키워드를 붙이면, 함수를 사용하는 쪽에서 인자에 `out` 키워드를 붙이지 않으면 에러가 납니다. 본문을 파악하기 쉽게 하기 위한 용도 이고, 의미적인 측면에서 아무런 뜻은 없습니다.
```csharp
void Func(out int* x) { ... }

int a = 0;
Func(out &a);
```
