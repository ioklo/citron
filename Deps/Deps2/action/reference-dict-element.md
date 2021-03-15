# dict<,> 아이템 참조

```csharp
var d = { "a": 34, "b": 44 }; 

assert(d["a"] == 34);    // 없으면 exception

var v1 = d.GetValue("b"); // v1은 nullable<int>, 44
var v2 = d.GetValue("c"); // v2은 nullable<int>, null

```

