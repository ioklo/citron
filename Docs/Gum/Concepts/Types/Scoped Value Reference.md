# Scoped Value Reference

Scoped Value를 가리키는 Value입니다.

scope를 벗어나면 사용할 수 없도록 분석 단계에서 검사합니다.

함수 호출에서만 사용할 수 있습니다

```csharp
void Func(out int& i, int& j)
{
    i = 5;
    j = 3;
}

var i = 0, j = 2;
Func(out i, j);

Assert(i == 5);
Assert(j == 3);
```