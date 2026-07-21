# Current Decisions

Status: current snapshot

이 문서는 agent가 현재 큰 결정을 빠르게 파악하기 위한 요약이다. 자세한 내용은 각 topic page를 본다.

## Documentation
- 현재 기준 지식은 `ai/wiki/`에 둔다.
- `ai/notes/`는 history로 유지한다.
- 기존 specs/implementations 내용은 wiki로 옮긴 뒤 active tree에는 유지하지 않는다. 필요하면 `git history: ai/specs/`와 `git history: ai/implementations/`를 확인한다.

## Language
- `some Trait`는 함수 return position 전용 opaque result marker다. 일반 type expression이 아니며 변수/인자/generic argument 위치에는 쓰지 않는다.
- opaque return 표기는 `some T`를 사용하며 `some<T>`는 허용하지 않는다.
- `some`은 일반 type parser가 아니라 함수 declaration의 return 자리에서만 특별히 읽는 쪽을 선호한다.
- `some Trait` 결과는 `var`로만 받으며, source-level에서는 declared trait surface만 사용할 수 있다.
- `trait`는 static contract다. `interface`는 dynamic/runtime dispatch contract로 따로 둔다.
- `func<R, Params...>`는 callable static contract type expression이다. `lambda<>` type expression은 두지 않는다.
- 일반 `dyn trait`는 도입하지 않는다. dynamic dispatch가 필요하면 명시적 `interface`를 설계한다.
- associated type inference는 v1에서 제외한다. trait 구현체는 `type Item = T;` 또는 nested type으로 associated type requirement를 명시적으로 충족한다.
- `concept`는 초기에는 반복되는 `where` constraint 묶음으로 본다.
- `T&`는 일반 first-class type constructor가 아니라 parameter/return/local alias/implicit this 같은 제한된 surface slot의 reference 표기다.
- Nested nullable은 flatten하지 않는다. `C?`는 compressed nullable representation, 일반 `T?`는 tagged nullable representation으로 본다.
- nullable binding pattern은 `if (x is not_null v)` 형태를 사용한다.
- 초기 `foreach`는 `RefEnumerable` / `RefEnumerator` 위의 `foreach(var& x in e)`를 우선한다. `foreach(var x in e)`의 `RefEnumerable` fallback은 두지 않는다.
- `void`는 `tuple<>`와 별도 타입이다. `T = void` generic argument는 허용하되 내부에는 `__VoidSubst`를 사용할 수 있다.
- BC/NBC value semantics는 observable behavior 기준으로 구분한다. NBC lifetime operation은 보존해야 한다.
- Return은 기본적으로 RVO path를 요구하고, NBC return call은 dest-passing / caller-provided storage를 기본 lowering으로 본다.
- Binding을 만드는 `is`는 일반 expression context에서 금지하고, `if` condition의 top-level에서만 허용한다.
- import된 module의 모든 declaration name은 lookup candidate가 될 수 있으며, `public`/`private`/`protected`는 resolution 뒤 accessibility checker가 use permission을 판정한다. inaccessible candidate도 lexical shadowing에 참여하고 access failure 때문에 outer candidate로 fallback하지 않는다. declaration metadata reachability는 public API/ABI 또는 linker export와 별개다.
- Nested declaration은 논리적으로 허용 가능하지만, v1 허용 범위는 implementation scope와 design stability에 따라 결정한다.
- Nested generic declaration identity는 outer type arguments를 포함한다. 예: `C<int>.Trait`와 `C<string>.Trait`는 다르다.
- Struct는 concrete struct를 상속하지 않는다. Struct의 `:` 뒤에는 trait conformance만 올 수 있고 struct member에는 `protected`를 허용하지 않는다.
- 원본 module은 `struct S : Trait`로 canonical conformance를 선언하고 `impl S : Trait {}`로 구현한다.
- generic struct의 canonical impl header는 `impl S<U> : Trait<U> {}`처럼 target pattern에 parameter를 드러낸다. `U`는 impl header가 도입하며, `struct S<T> : Trait<T>`의 `T`와 alpha-equivalent하다. `struct S<T> : Trait<T>`는 모든 well-formed `S<T>`에 대한 conformance를 선언하고 impl header는 그 전체 범위를 구현한다.
- `where` constraint가 있는 generic impl도 `impl S<U> : Trait<U> where U : OtherTrait {}`처럼 target-pattern parameter를 사용한다.
- specialization/conditional conformance는 direct canonical impl이 아니라 `extension Bundle for S<int> : Trait;`와 대응 `impl Bundle for S<int> : Trait {}` 같은 named bundle로 선언한다. bundle conformance는 소비 file의 `extend`로 활성화한다.
- v1에서 `impl` target은 `struct`로 한정한다. `class`, `enum`, structural type 등 다른 target category는 후속 설계에서 단계적으로 검토한다.
- 외부 module은 `extension Bundle for S : Trait;`로 이름 있는 conformance bundle을 선언하고 `impl Bundle for S : Trait {}`로 구현한다.
- generic bundle은 `extension Bundle<T> for S<T> : Trait<T>;`로 선언한다. declaration-name slot의 `T`는 bundle parameter이고, 대응 `impl Bundle<U> for S<U> : Trait<U>`의 `U`는 impl target-pattern parameter다.
- 외부 bundle은 자동 활성화하지 않는다. 소비 file에서 `import Provider;`로 declaration world를 연 뒤 `extend Bundle;`로 bundle family 전체를 활성화한다. `extend Bundle<int>;`는 concrete instantiation만 활성화하고, generic 또는 trait-selective activation은 `extend<U> Bundle<U> : Trait<U>;`처럼 explicit parameter clause와 trait selector를 사용한다. nested declaration에서는 parameter clause 없이 lexical generic context의 parameter를 capture할 수 있다. bundle header가 target mapping을 가지므로 `extend`에서 `for` target은 생략한다.
- bundle의 trait entry별 activation selection은 유지한다. 활성화된 bundle entry들의 conformance pattern이 겹치면 같은 file에서 동시 활성화를 금지하며, overlap은 conformance 사용 지점이 아니라 activation 시점에 진단한다. 이 규칙은 사용 편의보다 explicit activation을 우선한다.
- `extension`은 bundle declaration, `impl`은 witness implementation, `extend`는 file-local activation 역할로 구분한다.
- 외부 extension은 target의 private member에 접근 가능한 trusted augmentation이다. ordinary consumer와 extension compiler 모두 private declaration을 lookup candidate로 얻을 수 있지만, extension context만 target private member access를 통과한다.
- 별도 export modifier는 두지 않는다. accessibility는 name lookup surface가 아니라 use permission을 정하며, declaration metadata reachability와 public API/ABI export는 분리한다.

## Modules And CTI
- `cti`는 declaration/import boundary다.
- Swift식 opaque result metadata 모델을 택하면 consumer-facing `rcti`는 없어질 수 있다.
- `cti`는 `some` opaque result identity, metadata accessor symbol, opaque sret ABI contract를 담을 수 있어야 한다.
- 외부 module은 `import A;`로 열고, alias가 필요하면 `import A as X;`를 사용한다.
- Module qualification은 import를 대신하지 않으며, `A.Name`이나 `global::A.Name`을 사용해도 `import A;`가 먼저 필요하다.
- `import`와 `using` alias는 unit-local이며 export되지 않는다. `using X = T;`는 unit-local convenience alias다.
- `type X = T;`는 type-decl-space에 들어가는 정식 transparent type alias declaration이며 accessibility에 따라 다른 unit/module에서 접근할 수 있다.
- `import unit` / `using unit`은 두지 않는다. 같은 module의 모든 unit declaration을 body보다 먼저 자동 수집해 forward reference를 해결한다.
- public extension bundle의 target/trait 목록과 witness identity는 module declaration surface 및 dependency metadata에 포함한다.

## Compiler
- `some` opaque result call은 metadata accessor, value witness, trait witness, opaque sret로 낮춘다.
- value witness는 size/align/copy/move/destroy 같은 값 기본 연산 테이블이다.
- trait witness는 trait requirement를 backing type 구현으로 연결하는 테이블이다.
- global identifier는 canonical string을 사용한다. `RName`은 lookup-only string이 아니라 local/parameter/reserved compiler name까지 표현하는 구조화 semantic name value이며, lookup에서는 overload-family key로 쓸 수 있다. `RIdentifier`는 same-outer exact declaration key, `GlobalDeclIdentifier`는 `ModuleName::` prefix와 `RIdentifier` path, `GlobalTypeIdentifier`는 canonical type expression이다. identifier는 live symbol/type으로 역직렬화할 필요 없이 equality/hash와 external symbol lookup에 우선 사용한다. normal name은 `$`, `,`, `.`, `:`, `(`, `)`, `<`, `>`를 escape한다. tag의 one argument는 바로 붙이고, multiple RName argument는 `(...)`, multiple type argument는 `<...>`를 쓴다.
- generic conformance header는 generic signature, self type-argument pattern, trait type arguments, constraint를 포함한다. canonical/bundle conformance의 implementation은 lexical symbol tree의 internal `RImplTraitDecl` subtree로 나타낸다. `RImplTraitDecl`은 source lookup member가 아니며 `$I(TargetRIdentifier,TraitGlobalTypeIdentifier)` 형식의 internal `RName_ImplTrait`를 사용한다. target은 current lexical outer의 direct type member로 제한하고, trait type은 module prefix를 생략하지 않는다. public header는 RSymbol declaration surface에 남기고, CTI에는 conformance/witness identity를 남긴다.
- declaration/body resolution 주변의 공용 sum type은 raw public `std::variant` alias보다 얇은 wrapper class를 선호하고, 호출부에는 free helper보다 member API를 우선 둔다.
- declaration tree 축은 category view와 provenance payload에서 분리하는 쪽을 선호한다. 현재 leaning은 semantic tree node를 별도 `RNode` 모델로 세우고, `RTypeDecl` / `RFuncDecl`는 category view로 보는 것이다.
- `RNode`는 우선 `RName` 중심의 lightweight tree node로 두고, generic arity나 callable parameter identity 같은 richer declaration identity는 별도 metadata로 둔다.
- `RDecl`의 `GetTypeParam`, `GetTypeMember`, `GetMember`는 현재 declaration scope만 조회한다. 각각 generic binder, qualified type child, type parameter를 제외한 named member를 찾는다.
- `GetMember`의 결과는 single declaration과 function overload group을 표현할 수 있는 `RMember`로 둔다. `RMember`는 type parameter를 포함하지 않으며 `RDeclRes`로 변환할 수 있다.
- `RDecl`의 `ResolveTypeIdentifier`, `ResolveTypeIdentifierInHeader`, `ResolveIdentifier`는 현재 scope의 `Get*` 결과를 확인한 뒤 miss일 때만 outer lexical scope로 재귀한다. type parameter는 `ResolveTypeIdentifier`와 `ResolveIdentifier`에서 lexical binder로 검색되지만 qualified member surface에는 참여하지 않는다.
- `ResolveTypeIdentifierInHeader`는 현재 declaration의 type parameter만 확인하고, outer에는 일반 `ResolveTypeIdentifier`로 진행한다. 따라서 header는 자기 member를 보지 않되 outer declaration의 정상 type lookup은 사용한다.
- class의 inherited lookup은 `ResolveInheritedTypeMember`/`ResolveInheritedMember`가 담당한다. 이 hook은 base chain만 탐색하며 outer lexical scope로 진행하지 않는다. base generic arguments는 derived/current arguments에 apply한 뒤 찾은 result에 보존한다.
- symbol/declaration 문맥에서는 containing tree edge를 `outer`, inheritance edge를 `base`로 부르고, `parent`는 쓰지 않는 쪽을 선호한다.
- `Get`/`Resolve`의 이름은 result 형식이 아니라 탐색 범위로 정한다. `GetMember`/`GetVar`는 현재 object의 direct table만 조회하고 applied result를 만들 수 있다. `ResolveTypeIdentifier`/`ResolveIdentifier`는 generic binder, inherited member, outer lexical scope 등 lookup policy를 적용한다.
- `ROuterAppliedDecl<T>`은 outer type arguments만 적용된 declaration이고, `RAppliedDecl<T>`은 필요한 type arguments가 모두 적용된 declaration이다. `RDeclRes`/`RTypeRes`의 nested type variants는 전자를, direct variable variants는 후자를 사용한다.
- 함수 overload lookup result는 `ROuterAppliedFuncDeclGroup<TFuncDecl>`으로 표현한다. source의 explicit function type arguments는 RSymbol lookup result에 결합하지 않고 SmTranslator의 `SmPartiallyAppliedFuncDeclGroup<TFuncDecl>::memberTypeArgs`에 둔다. candidate matching은 이 prefix와 outer arguments를 합쳐 남은 function type parameters를 inference한 뒤 full type arguments를 만든다.
- Citron에서 generic definition은 `RTypeDecl`이고 unbound `RType`은 만들지 않는다. `RType`은 arguments가 적용된 constructed type만 나타낸다. `S<T>`는 open constructed type, `S<int>`는 closed constructed type이다.
- type declaration은 C++처럼 same-name generic arity overloading을 허용하지 않는 쪽을 선호하고, C#류 arity distinction은 interop/import layer에서 해소하는 방향을 선호한다.
- semantic tree는 먼저 tree 모델로 안정화하고, path map 중심 모델은 그 뒤에 재검토한다.
- accessibility는 단일 tree-only `CanAccess`보다, module/namespace member 정책과 type-member/inheritance 정책을 분리한 별도 checker/policy layer로 두는 쪽을 선호한다.
- MIR 값 모델은 BC/NBC, read/create/init destination을 분리하는 방향을 유지한다.
- `MCreate`는 MIR surface에 유지하되, lowering 중심 primitive는 `TranslateMExp`, `TranslateMLoc`, `TranslateMInitExp`로 분리한다.
- QIR call lowering은 logical slot model과 passing mode를 사용하고, physical ABI 선택은 후속 lowering에 맡긴다.
- QIR slot index만으로 meaning을 추론하지 않고 role metadata를 둔다. Indirect return은 hidden first slot `HiddenReturnDestPtr` shape를 current preferred로 본다.
- QEvaluator는 backend physical behavior가 아니라 observable event trace를 보존하는 semantic reference executor다.

## Process
- 기능/버그 수정에는 관련 테스트 추가/갱신을 동반한다.
- Windows 빌드/테스트는 `ai/process/build-test-and-generation-windows.md`를 따른다.
- `.g.cpp`, `.g.h`는 직접 수정하지 않고 generator를 통해 갱신한다.
