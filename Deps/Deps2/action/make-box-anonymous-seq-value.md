# seq 함수로 box 값 만들기

```csharp
struct S
{
    int x;

    seq int MakeSeq()
    {
        yield x;
        yield x + 4;
    }
}

var s = new S();
var ss = box s.MakeSeq(); // anonymous_seq 리턴후 박싱이 아닌, 직접 box<anonymous_seq> 리턴
```