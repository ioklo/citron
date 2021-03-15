# 튜플 멤버 변수 참조

```csharp
var t0 = (2, "hi", false);
tuple<int number, string text, bool flag> t1 = (x: 2, y: "hi", z: false);

// 1. unnamed 튜플일 경우 Item*로 참조
t0.Item0 = 7;
t0.Item1 = "hello";
t0.Item2 = true;

// 2. 튜플 타입 이름을 사용함 (다른 이름의 튜플이 들어와도 타입 이름)
t1.number = 11;
t1.text = "hi";
t1.flag = false;

t1.x = 7 // 에러

// 3. Head는 첫번째 원소, Rest는 나머지 부분 튜플, 없으면 (), 
// Rest에는 대입 불가능 참조만 가능
t0.Head = 7;
t0.Rest.Head = "hello";
t0.Rest.Rest.Head = true;
assert(t0.Rest.Rest.Rest == ());


```