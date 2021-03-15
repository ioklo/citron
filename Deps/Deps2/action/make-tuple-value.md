# 튜플 값 생성

```csharp
var t1 = (1, "hi", bool); // unnamed-tuple, x.Item0, x.Item1, x.Item2로 접근 가능
var t2 = (Message: "Hello", Code: 41);
tuple<string Message, int Code> t3 = ("Hello", 44); // 타입 선언
```

