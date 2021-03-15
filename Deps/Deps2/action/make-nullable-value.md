# nullable 값 생성

```csharp
// 타입 힌트를 사용한 nullable값 생성
nullable<int> i = null;  // 타입 힌트 null type (적기가 불가능) -> nullable<int>
nullable<int> i = 1;     // 타입 힌트 int -> nullable<int>
```
