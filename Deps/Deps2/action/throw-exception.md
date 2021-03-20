# 예외 던지기

```csharp
enum FuncError { NotImplemented, NotFound }

void Func() throw FuncError
{
    ...
    throw NotImplemented; // 1. 예외 던지기 함수 시그니쳐와 맞아야 한다
}

// 2. throw void
void Func() throw
{
    throw;
}

// 3. throw disallowed
void Func()
{
    
}

```