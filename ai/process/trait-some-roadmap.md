# This Week

목표:
- 한 파일 안에서 `trait`와 `some` 리턴을 시험할 수 있는 최소 바탕 만들기

수요일:
- `trait` 문법과 최소 형태 정리
- `some T`는 리턴에만 붙는다는 규칙 코드에 넣기
- `some<T>` 금지

금요일:
- `some T` 함수 선언 파싱
- 함수가 `some Trait`를 리턴할 때, 실제 리턴값이 그 trait를 만족하는지 검사

토요일:
- 아래 코드가 되는지 확인

```citron
trait Printable
{
    void Print();
}

struct S
{
    void Print() { @S; }
}

some Printable Make()
{
    return S();
}

void Main()
{
    var x = Make();
    x.Print();
}
```

이번 주에 안 하는 것:
- 다중 파일 컴파일
- `cti` 저장/불러오기
- `foreach`
- generics
- `struct S`의 기존 메서드와 `extend S : Trait` 메서드가 같은 이름/인자를 가질 때의 호출 규칙

다음 주는:
- 이번 주 끝나고, 어디까지 됐는지 보고 다시 정한다
