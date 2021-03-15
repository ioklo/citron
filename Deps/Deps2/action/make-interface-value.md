# 인터페이스 값 생성

`box<Struct>`, `class` 타입으로 캐스팅을 통해 인터페이스 값을 생성할 수 있습니다

```csharp
interface I { }
struct S : I { }

// 1. 암시적 캐스팅 box<S> -> I
I i = box S();
```

이 부분 cast-class-value 랑 겹친다.
cast-box-value 까지 만들어서 이 문서를 없애자
