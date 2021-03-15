# 튜플 정의

```csharp
// 1. using type alias
type t = tuple<int x, string S>;

// 2. inline
tuple<int x, int y> pair;

tuple<int x, int y> F()
{
    return (1, "hi");
}

```