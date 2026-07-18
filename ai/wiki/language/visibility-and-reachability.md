# Visibility And Reachability

Status: draft current
Area: language, name lookup, module
Keywords: visibility, reachability, private, internal, public, var, cti

## Current Direction
- module import는 provider의 모든 declaration name과 typecheck에 필요한 semantic metadata를 lookup surface에 올린다.
- accessibility는 name lookup filter가 아니라 resolution 뒤의 use-permission check다. inaccessible declaration도 lexical shadowing에 참여하며, access failure 때문에 outer candidate로 fallback하지 않는다.
- `public`/`private`/`protected`는 source context의 accessibility를 결정한다. declaration metadata reachability는 public API/ABI contract 또는 linker export와 별개다.
- inaccessible type도 public API를 통해 값으로 흐를 수 있고, 사용자는 그 값을 `var`로 받을 수 있다.

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

외부 `extension` implementation은 target의 private member에 접근할 수 있는 trusted augmentation이다. ordinary consumer와 extension compiler 모두 private declaration을 lookup candidate로 얻을 수 있지만, extension context의 access policy만 target private member 사용을 허용한다.

## Lookup And Accessibility

lookup은 declaration candidate를 찾고, accessibility checker는 그 candidate를 현재 context에서 사용할 수 있는지 판정한다.

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
List<int>.Enumerator e = list.GetEnumerator();  // error: Enumerator is inaccessible
```

## Using Not-Visible Values
`var`로 얻은 값의 실제 type name이 inaccessible해도, 값 자체는 사용할 수 있다.

```citron
var e = list.GetEnumerator(); // ok
e.Next();                     // ok if Next is public
```

반면 source code에 inaccessible type name을 직접 쓰면 accessibility check에서 막는다.

```citron
List<int>.Enumerator e; // error if Enumerator is inaccessible
```

Generic constraint나 explicit type annotation도 accessibility check 대상이다.

```citron
void F<T>()
    where T == List<int>.Enumerator // error if Enumerator is inaccessible
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
- Public API에 inaccessible type이 흐르는 것을 문서/API 표시에서 어떻게 보여줄지
- private declaration metadata의 ABI compatibility/versioning policy
- Inferred public signature를 `cti`에 어떻게 표현할지

## History
- `ai/notes/2026-05-22-nested-decl-visibility-and-resolved-cti.md`
- `ai/notes/2026-05-12-module-visibility-and-internal-fdecl-direction.md`
- `ai/notes/2026-06-29-accessibility-struct-trait-extension-direction.md`
