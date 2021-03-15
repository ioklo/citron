# seq<> 인터페이스 값 생성

```csharp
// 1. anonymous seq를 boxing 한 다음 캐스팅하면 생성
seq int MakeSeq(int x) { yield x; }

var l = box MakeSeq(3);
seq<int> s = l; // seq<int> <- box<anonymous_seq0> where anonymous_seq0 : seq<int>

// 2. 직접 생성 (타입 힌트)
seq<int> F()
{
    return MakeSeq(); // 암시적 박싱
}

// 3. 변수 선언으로 부터 타입 힌트
seq<int> s = MakeSeq(); // 암시적 박싱

```

