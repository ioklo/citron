# constraint로부터 값 생성

```csharp
interface I 
{
    I(int x); // 생성자 constraint
}

T F<T>(int num) where T : I 
{
    return new T(num);
}
```

