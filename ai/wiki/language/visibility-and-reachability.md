# Visibility And Reachability

Status: draft current
Area: language, name lookup, module
Keywords: visibility, reachability, private, internal, public, var, cti

## Current Direction
- `visibility`는 source code에서 이름을 직접 쓸 수 있는지에 대한 lookup rule로 좁게 본다.
- `reachability`는 compiler가 typecheck, ABI, conformance, lowering을 위해 semantic information을 알 수 있는지로 분리한다.
- Private / not-visible type도 public API를 통해 값으로 흐를 수 있다.
- 사용자는 not-visible concrete type의 값을 `var`로 받을 수 있다.
- namespace 수준 accessibility가 외부 접근과 export 여부를 함께 결정하며 별도 export modifier는 두지 않는다.

## Accessibility By Outer Kind

Accessibility는 declaration의 immediate outer 종류에 따라 해석한다.

| outer | 허용 accessibility |
|---|---|
| namespace/module | `public`, `private` |
| class | `public`, `protected`, `private` |
| struct | `public`, `private` |

- Struct는 concrete struct를 상속하지 않으므로 struct member에 `protected`를 허용하지 않는다.
- Nested type은 자신의 종류와 무관하게 containing outer의 member accessibility 규칙을 따른다.
- `protected`는 symbol containment뿐 아니라 class inheritance와 receiver type을 함께 검사해야 한다.
- Effective accessibility는 declaration path에 있는 enclosing declaration의 accessibility와 member accessibility를 모두 만족해야 한다.

외부 `extension` implementation은 예외적으로 target의 private member에 접근할 수 있는 trusted augmentation이다. Private semantic/ABI 정보는 extension compilation에 reachable할 수 있지만 일반 source name lookup에는 visible하지 않다.

## Visibility
Visibility는 name lookup rule이다.

```citron
class List<T>
{
private:
    struct Enumerator
    {
    }

public:
    Enumerator GetEnumerator();
}
```

외부:
```citron
var e = list.GetEnumerator();                   // ok
List<int>.Enumerator e = list.GetEnumerator();  // error: name not visible
```

## Using Not-Visible Values
`var`로 얻은 값의 실제 type name이 visible하지 않아도, 값 자체는 사용할 수 있다.

```citron
var e = list.GetEnumerator(); // ok
e.Next();                     // ok if Next is public
```

반면 source code에 private/not-visible type name을 직접 쓰면 막는다.

```citron
List<int>.Enumerator e; // error if Enumerator is not visible
```

Generic constraint나 explicit type annotation도 name을 직접 쓰는 경우로 본다.

```citron
void F<T>()
    where T == List<int>.Enumerator // error if Enumerator is not visible
{
}
```

## Public API And Inferred Return
Not-visible type을 반환하는 function을 forwarding하려면 return type을 직접 적기 어렵다.

```citron
func GetEnumerator()
{
    return list.GetEnumerator();
}
```

이런 form은 return type inference 또는 opaque result와 연결될 수 있다. 현재 `some Trait`는 opaque result metadata model로 정리 중이며, 일반 inferred return은 별도 open point다.

## Open Points
- Public API에 not-visible type이 흐르는 것을 문서/API 표시에서 어떻게 보여줄지
- `private` nested declaration이 외부 surface에 존재 정보까지 실릴지
- Inferred public signature를 `cti`에 어떻게 표현할지

## History
- `ai/notes/2026-05-22-nested-decl-visibility-and-resolved-cti.md`
- `ai/notes/2026-05-12-module-visibility-and-internal-fdecl-direction.md`
- `ai/notes/2026-06-29-accessibility-struct-trait-extension-direction.md`
