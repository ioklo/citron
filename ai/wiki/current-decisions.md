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
- `visibility`는 source name lookup rule이고, `reachability`는 compiler가 semantic information을 알 수 있는지의 문제로 분리한다.
- Nested declaration은 논리적으로 허용 가능하지만, v1 허용 범위는 implementation scope와 design stability에 따라 결정한다.
- Nested generic declaration identity는 outer type arguments를 포함한다. 예: `C<int>.Trait`와 `C<string>.Trait`는 다르다.
- Struct는 concrete struct를 상속하지 않는다. Struct의 `:` 뒤에는 trait conformance만 올 수 있고 struct member에는 `protected`를 허용하지 않는다.
- 원본 module은 `struct S : Trait`로 canonical conformance를 선언하고 `impl S : Trait {}`로 구현한다.
- 외부 module은 `extension Bundle for S : Trait;`로 이름 있는 conformance bundle을 선언하고 `impl Bundle for S : Trait {}`로 구현한다.
- 외부 bundle은 자동 활성화하지 않는다. 소비 file에서 `import Provider;`로 declaration world를 연 뒤 `extend Bundle for S : Trait;`로 target과 trait를 명시해 활성화한다.
- `extension`은 bundle declaration, `impl`은 witness implementation, `extend`는 file-local activation 역할로 구분한다.
- 외부 extension은 target의 private member에 접근 가능한 trusted augmentation이다. Private 정보는 extension compiler에 reachable할 수 있지만 일반 lookup에는 visible하지 않다.
- namespace accessibility가 외부 접근과 export 여부를 함께 결정하며 별도 export modifier는 두지 않는다.

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
- declaration/body resolution 주변의 공용 sum type은 raw public `std::variant` alias보다 얇은 wrapper class를 선호하고, 호출부에는 free helper보다 member API를 우선 둔다.
- declaration tree 축은 category view와 provenance payload에서 분리하는 쪽을 선호한다. 현재 leaning은 semantic tree node를 별도 `RNode` 모델로 세우고, `RTypeDecl` / `RFuncDecl`는 category view로 보는 것이다.
- `RNode`는 우선 `RName` 중심의 lightweight tree node로 두고, generic arity나 callable parameter identity 같은 richer declaration identity는 별도 metadata로 둔다.
- `GetMember`는 "해당 scope에서 이름으로 접근 가능한 member" 전반을 다루는 넓은 API로 보고, type parameter도 필요하면 member로 노출할 수 있다.
- `GetTypeMember` / type-only resolution은 일반 member lookup과 shadowing 규칙이 다르므로 별도 resolver 단계에서 처리하는 쪽을 선호한다.
- `ResolveIdentifier`는 declaration node API보다 resolver / lexical scope algorithm 책임으로 보는 쪽을 선호한다.
- symbol/declaration 문맥에서는 containing tree edge를 `outer`, inheritance edge를 `base`로 부르고, `parent`는 쓰지 않는 쪽을 선호한다.
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
