# 예외 잡기

```csharp

// 1. 실행중 발생하는 모든 예외 잡기, 안 잡을 경우 컴파일 워닝, 실행중 바로 종료
enum FuncError1 { NotImplemented }
void Func1() throw FuncError1;

enum FuncError2 { NotFound }
void Func2() throw FuncError2;

try
{
    Func1();
    Func2();

    ...
}
catch(FuncError1 fe1)
{
    ...
}
catch(FuncError2 fe2)
{
    ...
}
catch // 나머지 에러
{

}
```

```csharp
class E { }
class E1 : E { }
class E2 : E2 { }

void Func1() throw E1 {}
void Func2() throw E2 {}

try
{
    Func1();    
    Func2();
}
catch(E2 e2) // 순서대로.. e2에 매칭이 먼저되면 여기서 처리
{

}
catch(E) // e 에 매칭이 되면 여기서 처리
{

}
catch // 나머지 처리
{

}

```
