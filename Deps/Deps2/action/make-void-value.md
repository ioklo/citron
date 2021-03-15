# void 값 생성

금지, 하지만 void가 값이므로 Generic에서 쓰기 좋다

```csharp
T PassThru<TFunc, T>(TFunc func) where TFunc : func<T>
{
    return func(); // constraint가 func<T>이므로 호출된다
}

void F() { }

// x는 void 값,, 쓸데도 없다
var x = PassThru(F); // PassThru<anonymous_seq0> where anonymous_seq : func<void> 호출

```