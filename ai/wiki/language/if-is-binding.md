# If / Is Binding

Status: draft current
Area: language, control flow, pattern
Keywords: if, is, binding, nullable, pattern, IfBind

## Current Rules
- Surface syntax에서는 `is`를 가능한 한 하나의 expression form으로 유지한다.
- Binding을 만드는 `is`는 일반 expression context에서 금지한다.
- Binding을 만드는 `is`는 `if` condition의 top-level에서만 허용한다.
- Binding 없는 `is`는 일반 expression context에서 허용한다.
- Body에서 보이는 binding은 condition이 true가 되는 모든 path에서 생성되는 binding만 허용한다.

## Is Expression Forms
아래 형태는 모두 `is` expression으로 본다.

```citron
c is null
c is D
c is D d
o_c is not_null c
e is E.Second(x, _)
```

## Binding Visibility
허용 가능한 방향:
```citron
if (a is D d && d.x > 0)
{
    use(d);
}
```

허용하지 않는 방향:
```citron
if (a is D d || cond)
{
    use(d); // error: d is not guaranteed on every true path
}
```

## Compiler Notes
- Parser는 `is` expression을 통합 형태로 유지한다.
- Semantic analysis에서 binding 포함 여부와 허용 context를 검사한다.
- MIR lowering에서는 conditional binding lifetime 관리가 필요한 경우 bind 성격 node로 내린다.
- 내부 node 이름은 `IfBind` 방향을 우선 검토한다.

Compiler-side lowering details are tracked in `ai/wiki/compiler/if-is-lowering.md`.

Hot areas:
- `src/Syntax/`
- `src/SyntaxIR0Translator/`
- `src/MIR/`

## History
- `git history: ai/specs/language/if-and-is.md`
- `git history: ai/implementations/if-is-binding-snapshot.md`
