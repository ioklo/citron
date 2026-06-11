# 회의 / 설계 노트

Date: 2026-05-22
Title: nested declaration, visibility/reachability, resolved cti 방향

Status
- draft

Summary
- nested `class` / `struct` / `trait`는 논리적으로 가능하며, 특히 generic outer type 안에서는 같은 계열의 문제를 공유한다.
- nested trait를 금지할 논리적 이유는 약하고, 금지한다면 구현 범위와 문법 안정성 때문이다.
- `visibility`는 소스 코드에서 이름을 직접 쓸 수 있는지에 대한 lookup 규칙으로 좁게 본다.
- `reachability`는 compiler가 타입체크, ABI, conformance, lowering을 위해 의미 정보를 알 수 있는지로 분리한다.
- private / not-visible 타입도 public API를 통해 값으로 흐를 수 있으며, 사용자는 `var`로 받을 수 있다.
- 이런 타입을 forwarding하려면 return type inference 또는 `func`류의 inferred return declaration이 필요하다.
- `cti`는 source surface 단계와 resolved artifact 단계로 나누는 방향을 검토한다.

Context
- trait/concept 설계에서 nested trait를 class/struct 안에 둘 수 있는지 논의했다.
- generic class/struct 안 nested trait는 outer generic parameter capture, specialized nested identity, conformance lookup 문제를 만든다.
- 하지만 이 문제는 nested class/struct에도 동일하게 적용되므로, nested trait만 논리적으로 불가능하다고 보기는 어렵다.
- private 구현 타입은 nested type이 아니라 unit/cti export control로 숨길 수도 있다.
- 반대로 `List<T>.Enumerator`처럼 generic parameter를 한 곳에 모아 관리하고 싶은 경우에는 nested type의 장점이 있다.

Decisions / Current Preferences
## 1) nested declaration은 논리적 가능성과 구현 범위를 구분한다
- nested `class` / `struct` / `trait`는 논리적으로 성립한다.
- 특히 generic outer type 안의 nested trait는 "generic environment를 capture한 nested declaration"으로 볼 수 있다.
- 금지하거나 미루는 이유가 있다면 "말이 안 된다"가 아니라 "v1 구현 범위와 설계 안정성"이다.

예:
```citron
class C<T, U>
{
    public trait R<V>
    {
    }

    public struct S<W>
    {
    }
}
```

이 경우 `C<int, short>.R<bool>`와 `C<long, short>.R<bool>`는 outer type argument가 다르므로 다른 trait identity로 볼 수 있다.

## 2) nested trait identity는 outer type arguments를 포함한다
- outer generic parameter capture를 허용한다면 nested trait identity는 다음 요소를 포함한다.
  - outer declaration identity
  - outer type arguments
  - nested trait declaration identity
  - nested trait type arguments

예:
```citron
C<int, short>.R<bool>
```

는 `C<T, U>`의 `T=int`, `U=short`, `R<V>`의 `V=bool` specialization으로 본다.

## 3) nested generic type conformance는 family-level conformance로 해석한다
예:
```citron
class C<T>
{
    public struct S<U>
    {
    }

    public trait R<V>
    {
    }

    public extend S<U> : R<T>
    {
    }
}
```

의미:
- 모든 `T`, `U`에 대해 `C<T>.S<U> : C<T>.R<T>` conformance를 제공한다.
- 외부에서 `C<int>.S<short>`를 보면 `C<int>.R<int>` conformance가 visible/reachable할 수 있다.

## 4) generic target extend에는 type parameter를 명시하는 쪽을 선호한다
- `extend S<U> : R<T>`처럼 target generic argument를 명시하는 편이 좋다.
- `extend S : R<T>`는 `S`의 type parameter를 암시적으로 여는지, non-generic `S`를 가리키는지 애매하다.

예:
```citron
public extend S<U> : R<T>
{
}
```

이 문장은 현재 outer `C<T>` 안에서 모든 `U`에 대해 `S<U>` conformance를 제공한다고 읽는다.

## 5) nested type의 주요 이점은 이름공간과 generic parameter 공유다
- private 구현 타입은 unit/cti export control로 숨길 수 있으므로 nested type만의 강한 이유는 아니다.
- 공개 API에 nested type이 자주 드러나면 이름이 길고 지저분해질 수 있다.
- 그래도 `List<T>.Enumerator`처럼 outer generic parameter를 한 곳에서 관리하고 싶은 경우에는 nested type이 의미가 있다.

비교:
```citron
class List<T>
{
    struct Enumerator
    {
    }
}
```

```citron
class List<T>
{
}

struct ListEnumerator<T>
{
}
```

첫 번째는 `T`를 outer에서 공유하고, 두 번째는 identity와 conformance가 단순하다.

## 6) visibility는 "소스에 이름을 직접 쓸 수 있는가"로 제한한다
- `visibility`는 name lookup 규칙이다.
- private/not-visible type 이름은 소스 코드에 직접 적을 수 없다.
- 하지만 그 타입의 값이 public API나 type inference를 통해 흐르는 것은 별도 문제다.

예:
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
var e = list.GetEnumerator();              // ok
List<int>.Enumerator e = list.GetEnumerator(); // error: name not visible
```

## 7) 추론으로 얻은 not-visible 타입 값은 사용할 수 있다
- `var`로 얻은 값의 실제 타입 이름이 visible하지 않아도, 값 자체는 사용할 수 있다.
- public member 접근도 허용한다.

예:
```citron
var e = list.GetEnumerator(); // ok
e.Next();                     // ok, if Next is public
```

반면 이름을 직접 적는 경우는 막는다.
```citron
List<int>.Enumerator e; // error if Enumerator is not visible
```

## 8) generic constraint나 explicit type annotation도 이름을 직접 쓰는 경우로 본다
- source code에 private/not-visible type name을 직접 쓰면 막는다.
- 이는 local variable type annotation, return type annotation, generic constraint, associated type equality 등에 동일하게 적용한다.

예:
```citron
void F<T>()
    where T == List<int>.Enumerator // error if Enumerator is not visible
{
}
```

## 9) forwarding에는 inferred return이 필요할 수 있다
- not-visible type을 반환하는 함수를 다시 forwarding하려면 반환 타입을 직접 적기 어렵다.
- 이 경우 return type inference를 허용하는 함수 declaration form이 필요할 수 있다.

예:
```citron
func GetEnumerator()
{
    return list.GetEnumerator();
}
```

여기서 `func`는 "return type inferred function declaration" 후보로 볼 수 있다.

## 10) public inferred return type은 cti 모델과 연결된다
- public function의 return type을 body에서 추론하면, import consumer는 그 resolved signature를 알아야 한다.
- 따라서 source-level cti만으로는 부족할 수 있고, resolved artifact가 필요하다.

## 11) cti는 source surface와 resolved artifact로 나누는 방향을 검토한다
- source cti:
  - 사용자가 작성하거나 `ct`에서 추출한 1차 declaration surface
  - `func`, `some`, inferred return placeholder 같은 unresolved signature가 남을 수 있다
- resolved cti / `cti.o`:
  - `ct` body compile 후 return type inference, private reachable type, associated type resolution 등을 확정한 artifact
  - downstream compile이 실제 타입 정보를 필요로 할 때 의존한다

개념:
```text
source cti can contain inferred/opaque signature placeholders.
resolved cti must contain canonical resolved signature metadata.
```

## 12) inferred signature dependency graph를 만든다
- 어떤 `ct`가 다른 unit의 inferred public signature를 필요로 하면, 해당 unit의 resolved cti가 먼저 필요하다.
- inferred signature dependency에 cycle이 생기면 compile을 중지한다.

예:
```citron
// A.ct
public func F()
{
    return B.G();
}

// B.ct
public func G()
{
    return A.F();
}
```

이 경우 `A.F`와 `B.G`의 return type inference가 서로 의존하므로 cycle error로 본다.

Rationale
- nested trait/type은 logical model 자체보다 identity, conformance, visibility, cti surface가 복잡하다.
- 그 복잡성은 금지의 논리적 근거가 아니라 implementation staging의 근거로 보는 편이 정확하다.
- visibility를 name lookup에 한정하면 private/reachable type을 public API를 통해 흘리는 모델이 가능해진다.
- `var`가 있는 언어에서는 not-visible concrete type을 직접 쓰지 않고도 값을 보유하고 public operation을 사용할 수 있다.
- public inferred return은 convenience를 주지만, resolved cti와 dependency ordering이 필요하다.

Open Points
- nested class/struct/trait를 v1에서 허용할지, namespace-level declaration만 먼저 둘지
- generic outer parameter capture를 모든 nested declaration에 허용할지
- specialized extend를 허용할지
  - 예: `extend S<int> : R<T>`
- generic conformance overlap을 어느 시점에 어떻게 진단할지
- public API에 not-visible type이 흐르는 것을 문서/API 표시에서 어떻게 보여줄지
- `func` 또는 inferred return function syntax를 실제로 둘지
- 수동 `cti`에서 inferred return placeholder를 허용할지
- source cti와 resolved cti의 파일 형식, cache key, incremental rebuild 기준

Action Items
- [ ] visibility와 reachability 용어를 별도 규칙으로 정리
- [ ] nested generic declaration identity 규칙 초안 작성
- [ ] conformance identity / overlap 검증 규칙 초안 작성
- [ ] inferred public signature와 resolved cti dependency graph 초안 작성
- [ ] `func` 또는 return type inference syntax 후보 검토
