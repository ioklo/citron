# Type Aliases

Status: draft current
Area: language, name lookup, declarations
Keywords: import, using, type, alias, accessibility

## Three Distinct Roles

Module import, unit-local alias와 exported type alias declaration은 서로 다른 keyword를 사용한다.

```citron
import Collections as Col;
using Items = Col.Dictionary<string, Col.List<int>>;
public type UserId = int;
```

| syntax | role | lifetime / visibility |
|---|---|---|
| `import A as X;` | module world import와 module alias | declaring unit only |
| `using X = T;` | convenience alias | declaring unit only |
| `type X = T;` | transparent type alias declaration | declaration accessibility에 따름 |

## Unit-Local Using Alias

```citron
using Items = Dictionary<string, List<int>>;
```

- `using` alias는 symbol tree의 정식 declaration이 아니다.
- accessibility modifier를 붙이지 않는다.
- module declaration/export surface에 포함하지 않는다.
- 다른 unit에서는 같은 alias 이름을 직접 사용할 수 없다.

## Type Alias Declaration

```citron
public type UserId = int;

class C
{
    public type V = int;
    private type State = int;
}
```

- `type` alias는 type-decl-space에 들어가는 정식 declaration이다.
- namespace/class/struct member가 될 수 있다.
- enclosing declaration과 자신의 accessibility를 모두 만족하면 다른 unit/module에서 이름으로 접근할 수 있다.
- alias는 새로운 runtime type identity를 만들지 않는 transparent alias다.
- alias RHS의 이름 접근성은 alias 선언 문맥에서 검사한다. 추가로 target과 RHS에 쓰인 alias 등 외부 계약의 symbol은 alias 선언의 접근 범위를 포함해야 한다: `Access(alias) ⊆ Access(target/symbol)`.
- public alias가 private target type을 가리키는 것은 금지한다. alias chain과 generic type arguments로 private 타입을 간접 노출하는 경우에도 같은 규칙을 적용한다.
- public signature에 private type alias를 쓰는 것도 금지한다. private alias의 target이 `int` 같은 public 타입이어도 normalization 결과만 보고 허용하지 않는다. 선언에 사용한 정식 alias 자체의 접근성도 검사한다.
- unit-local `using`은 정식 type alias declaration과 구분한다. target 타입의 계약 접근성 검사는 여전히 필요하지만, `using`에 declaration accessibility modifier를 부여하지 않는다.

```citron
private class Hidden {}
public type Exposed = Hidden; // error: private target 노출

private type Count = int;
public Count GetCount();      // error: public target이어도 private alias 사용 금지
public int GetCountDirect();  // ok
```

일반 선언의 접근 범위와 `protected` 비교는 `visibility-and-reachability.md`를 본다.

Trait associated type도 같은 `type` declaration 계열을 사용한다.

```citron
trait Enumerable
{
    type Item;
}

impl S : Enumerable
{
    type Item = int;
}
```

공개 conformance를 통해 알 수 있는 associated type witness는 그 conformance의 접근 범위를 포함해야 한다. `impl`에 `public`을 직접 적지 않아도 private 타입을 witness로 노출하면 거부한다.

## Open Points

- generic type alias의 type parameter surface
- direct/indirect alias cycle 진단 시점
- alias normalization 이전의 계약 접근성 검사 또는 사용한 alias provenance 보존 방식

## History

- `ai/notes/2026-08-31-declaration-contract-accessibility.md` (이전 public alias의 private target 노출 허용 규칙을 대체)
- `ai/notes/2026-05-08-cti-import-and-static-interface-direction.md`
- `ai/notes/2026-06-29-accessibility-struct-trait-extension-direction.md`
