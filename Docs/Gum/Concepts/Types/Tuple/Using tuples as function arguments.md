# Tuple을 함수 인자로 사용하기

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

template<THead, TRest> 
void print(THead hd, params TRest rest) // 인자가 두개 이상 있을 때만 매칭
{
    print(hd);
    print(rest);
}

print(2, "hi", false); // template<int, (string, bool)> print(2, ("hi, false));
```