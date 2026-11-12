# Class Constructors

Status: current rules; implementation not verified
Area: language, construction
Keywords: class, constructor, init, direct, memberwise constructor

## Current Rules

- Class 객체는 `new`로 생성한다. 자동 memberwise constructor는 제공하지 않으며 생성자와 인자 목록을 명시한다.
- 생성자 인자 목록에서 `init <field-name>`으로 같은 이름의 instance field를 직접 초기화한다. `direct`와 `trivial`은 앞서 검토한 후보이며 이 기능의 확정 표기가 아니다.
- `init x`의 타입은 필드 `x`의 선언에서 가져온다. 호출 인자를 필드의 저장소에 직접 적용하며 일반 값 매개변수 객체를 거쳐 필드로 옮기는 단계를 만들지 않는다.
- `init`으로 지정한 필드는 생성자 본문 진입 전에 초기화된다. 나머지 필드는 생성자에서 별도로 초기화한다.
- 이는 참조를 전달하는 인자가 아니다. 기존 문서의 컴파일러 전용 `[init] T&` 표기로 이 기능을 설명하지 않는다.
- 직접 초기화는 호출 인자 자체에 필요한 복사·이동 생성까지 금지한다는 뜻은 아니다.

```citron
class C
{
    int x;
    int y;
    string s;

    public C(init x, init y)
    {
        s = "hi";
    }
}

var c = new C(1, 2);
```

모든 필드를 인자로 초기화하려면 `public C(init x, init y, init s) {}`처럼 각각 적는다.

## Struct와의 구분

- Struct는 전체 필드의 자동 memberwise constructor를 기본 제공한다. 비활성화에는 기존 `S(...) = delete;` 표기를 사용한다.
- `memberwise S() = delete;`는 대안 후보이며 확정하지 않았다.
- Class의 `init` 인자와 struct의 자동 생성 규칙을 하나의 선언 문법으로 통일하지 않는다.

## 미해결 세부 사항

- Base 생성자 호출과 필드 초기화 순서, 생성 실패 시 cleanup
- 중복 필드 지정, 상속 필드 지정, 일반 인자와의 이름 충돌에 대한 세부 제약
- Parser, semantic model, lowering의 구현 및 검증

## Decision

- 2026-11-10 사용자 확정: class 생성자의 필드 직접 초기화 인자는 `direct` 대신 `init`으로 표기한다.
