# 글로벌 함수 정의

```csharp 
RetType FuncName<covariant_modifier TypeArg0...>(ArgType0 arg0, ...)
    where constraints
{
    Body Statements...
}

void F<T, U, V>(T t, ref<U>, u, params arglist<V> vs)
    where T : I
{
}
```