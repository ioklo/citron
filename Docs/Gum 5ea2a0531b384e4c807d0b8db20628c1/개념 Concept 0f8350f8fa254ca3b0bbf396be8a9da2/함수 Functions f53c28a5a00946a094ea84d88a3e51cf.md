# 함수 Functions

- 선언
    - 전역 함수
    - 멤버 함수 (Class, Struct, Enum)
- 호출

# 함수 선언

- 리턴 타입
- 함수 이름
- 타입 인자
- 인자
    - out
    - params

## 기본형

```csharp
int Func<T>(string x)
{
}
```

## 멤버함수

멤버함수에는 access modifier가 붙는다

```csharp
struct S
{
    private int Func<T>(string x)
    {
    }
}
```