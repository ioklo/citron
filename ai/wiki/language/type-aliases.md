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

## Open Points

- generic type alias의 type parameter surface
- direct/indirect alias cycle 진단 시점
- public alias가 less-accessible target type을 가리킬 때의 accessibility consistency rule

## History

- `ai/notes/2026-05-08-cti-import-and-static-interface-direction.md`
- `ai/notes/2026-06-29-accessibility-struct-trait-extension-direction.md`
