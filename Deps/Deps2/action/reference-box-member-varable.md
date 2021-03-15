# 박스 멤버 변수 참조

```csharp
box<int> i = box 3;
assert(i.Value == 3); // i.Value 단일 멤버
```

## 코멘트
box<>는 class처럼 보인다 개별 타입대신 class로 해도 될 것 같다