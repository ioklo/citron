# 글로벌 함수 호출

```csharp

namespace MyNamespace
{
    int F(int n, ref<int> i, int t1, string t2, bool b)
    {
        ...
        return n;
    }

    int F(int x, params arglist<int> ints)
    {
        
    }
}


var i = 0;
var t = (1, "string");

// 호출
var r1 = MyNamespace.F(2, ref i, params t, false); 

var r2 = MyNamespace.F(params (2, 3, 4));  // 

```

