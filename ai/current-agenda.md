# Current Agenda

## Topic
Type alias와 associated type 구현

## Active Work
- trait/impl parsing 완료.
- trait/impl syntax의 RSymbol 변환 완료.
- impl이 trait requirement를 만족하는지 검사하는 단계 완료.
- `some Trait` return 구현 진행 중:
  - opaque return type 함수 호출
  - 결과를 받을 저장소 생성
  - 생성된 변수에서 declared trait 함수 호출
- MIR에서 QIR로의 변환과 전체 테스트 확인은 후속 단계다.
- `foreach`의 `RefEnumerable` / `RefEnumerator` contract를 위해 associated type 구현을 다음 core 작업으로 검토 중이다.

## Associated Type Work Checklist
- [ ] trait에 associated type requirement를 추가한다: `type TItem;`.
- [ ] 정식 transparent type alias declaration을 추가한다: `type TItem = int;`. Declaration 등록, alias target 해석, lookup, normalization을 포함한다.
- [ ] 선언 외부 계약의 접근 범위 포함 검사를 구현한다. private alias 사용, protected owner 차이, bundle/header 및 associated type witness 노출을 포함한다.
- [ ] impl의 same-name type alias 또는 concrete nested type declaration을 associated type requirement의 type witness로 연결하고 만족 여부를 검사한다.
- [ ] generic trait constraint를 반영한 qualified type projection lookup을 추가한다: `T.TItem`은 내부적으로 `<T as MyTrait>::TItem`을 뜻한다.
- [ ] generic function의 typecheck와 shared runtime implementation 경로를 추가한다. Type metadata, trait witness, unknown-layout parameter/result 처리를 포함한다.
- [ ] function generic type argument inference를 추가한다. 이 단계는 유예할 수 있으며, 초기 end-to-end test에서는 explicit type arguments를 사용한다.
- [ ] `MyTrait::TItem` / `S` / generic `Func<T>` core 예제를 parser, symbol/conformance, typecheck 및 가능한 범위의 evaluation test로 추가한다.
- [ ] 후속 TODO: `type Item<T>;` 형태의 generic type member requirement를 지원한다. 이번 구현에서는 parameter 없는 requirement만 허용하되 semantic declaration은 향후 own type parameters를 추가할 수 있게 설계한다.

세부 순서와 acceptance test는 `ai/notes/2026-08-29-associated-type-implementation-plan.md`를 본다.

## Current Direction
- 선언 D의 외부 계약에 노출되는 symbol S는 `Access(D) ⊆ Access(S)`를 만족해야 한다. enclosing declaration을 반영한 실제 접근 범위로 비교하며, 서로 다른 owner의 `protected`를 같은 등급으로 취급하지 않는다. 이전 C++식 private type 노출 허용 규칙을 대체한다.
- public signature에 private type/alias를 쓰거나 public alias로 private target을 노출하는 것은 금지한다. private alias의 target이 `int`여도 public signature에는 그 alias를 쓸 수 없다. `some Trait`의 backing type과 함수 body/private 구현 멤버는 외부 계약과 구분한다.
- public struct의 header에 private trait를 직접 넣지 않는다. 내부용 conformance는 접근 범위가 맞는 bundle로 분리한다. 공개 conformance의 associated type으로 private 타입을 노출하는 것은 거부한다.
- trait 구현 전용 extension은 trait/header의 접근성, trait 인자·associated type 노출, impl signature 일치 검사로 계약을 보장한다. trusted private 접근 권한을 별도의 공개 권한으로 보지 않는다. 세부 규칙은 `ai/wiki/language/visibility-and-reachability.md`, 변경 이력은 `ai/notes/2026-08-31-declaration-contract-accessibility.md`를 본다.
- 소멸자에는 접근 지정자를 두지 않는다. 항상 public으로 취급하며 소멸자 accessibility check를 수행하지 않는다.
- 일반 generic은 type별 monomorphization을 기본 구현으로 삼지 않는다. 하나의 shared generic code가 type metadata와 trait witness를 hidden argument로 받는 Swift식 runtime generic ABI를 사용한다.
- generic specialization은 선택적 최적화다. 장래의 명시적 instantiation은 일반 generic과 분리된 `template` 또는 macro 기능에서 처리한다.
- associated type은 semantic에서는 `<T as Trait>::Member` projection으로 유지하고, runtime에서는 trait witness의 associated type metadata/accessor와 associated conformance witness를 통해 다룬다.
- trait requirement의 `some Trait`는 지금 구현하지 않는다. `foreach`에는 명시적 associated type을 사용하며, 나중에 필요하면 anonymous associated type 문법으로 별도 도입한다.
- associated type core 예제는 `Producer.Output : Value`와 `Output Produce()` 형태로 잡아 parsing, explicit type witness, bound 검사, signature substitution, generic projection을 `foreach`와 독립적으로 검증한다.
- `RAppliedDecl`은 `decl + complete RTypeArguments`만 보관한다. 적용 위치의
  `RTypeEnv`/`tenv`를 semantic value에 붙여 다니지 않는다.
- `RAppliedDecl`의 complete type arguments는 declaration의 flattened formal slots에
  대한 substitution이다. 뒤 substitution은 기존 arguments 각각에 적용하여 합성하고,
  항상 `decl + complete arguments` 한 겹의 정규형을 유지한다.
- declaration-backed가 아닌 type constructor는 substitution을 내부 type에 eager
  apply한다. declaration body는 instantiate하지 않고 applied declaration의 arguments만
  변환하므로 별도의 `RType_LazyApply`/`SmType_LazyApply` node는 두지 않는다.
- `SmType`, `SmAppliedDecl`, application용 `SmTypeEnv`는 제거하고 SmTranslator도
  `RType`, `RAppliedDecl`, `RTypeArguments`를 직접 사용한다.
- formal type environment는 substitution의 source/target과 합성 법칙을 설명하는 데
  사용하되, 구현 representation은 flattened complete `RTypeArguments` 배열로 erase한다.
- `RTypeParam::GetGlobalIndex()`는 outer부터 현재 declaration까지 평탄화한 type
  argument slot이다. `RType_TypeVar::Apply`는 이 index로 complete
  `RTypeArguments`를 조회한다.
- exact type equality는 `RTypeParam*` binder identity를 비교한다. 서로 대응하는
  generic signature의 alpha-equivalence는 함수-local binder를 양쪽의 canonical
  slot index로 대응시켜 비교한다. 한쪽 binder를 다른 signature의 binder로 바꾼
  임시 `RType`을 만들지 않는다.
- trait requirement와 impl signature 비교는 양쪽에 outer `RTypeArguments`와
  function-local `RTypeParam* -> size_t` correspondence를 둔 read-only 비교 context를
  사용한다. type variable을 만나면 먼저 정확한 substitution domain의 binder인지
  확인해 outer argument를 lazy하게 비교하고, 아니면 local canonical slot 또는
  exact binder identity를 비교한다.
- `RTypeArguments`의 배열 범위만으로 substitution 가능 여부를 판단하지 않는다.
  unrelated binder는 같은 flattened index를 가질 수 있으므로 source formal binder
  identity를 함께 확인한다. substitution RHS는 target context의 type이므로 원래
  outer substitution을 다시 적용하지 않는다.
- source type-name lookup에는 현재 보이는 type parameter 문맥이 필요할 수 있지만,
  이는 translator의 임시 lookup context이며 `RType`/`RAppliedDecl`의 영구 상태가
  아니다.
- `STypeExp`는 value expression 번역과 같은 단계 구조를 따른다:
  `STypeExp -> ImTypeExp -> ReTypeExp`.
- `ImTypeExp`는 type identifier/member chain을 계속 해석하는 중간 상태다. 현재
  variant는 namespace group, canonical `RType*`, applied trait declaration으로 제한한다.
  class/struct/enum처럼 nominal type을 별도 `ImTypeExp_*` variant로 중복 표현하지 않는다.
- qualified type member lookup은 `ImTypeExp_Type`의 `RType*` visitor가 맡는다. nominal
  `RType`은 자신이 보관한 applied declaration/arguments로 `GetTypeMember`를 수행하고,
  primitive·void·type constructor 등은 `TypeCantHaveTypeMember` 진단을 낸다.
- nested trait type의 명시적 type arguments도 outer arguments와 결합해
  `ImTypeExp_Trait`에 보관한다.
- `ReTypeExp`는 최종 type-expression 결과로 `RType*` 또는
  `RAppliedDecl<RTraitDecl>`을 담는다. `ReExp`와 동등한 번역 단계임을 드러내기 위해
  `ReTypeRes`보다 이 이름을 사용한다.
- `TranslateSTypeExpToRType` 및 `TranslateSTypeExpToRTrait`는 공통
  `ReTypeExp`를 만든 뒤 각각 해당 variant만 요구하는 consumer로 둔다.
- `Nullable`/`Shared`/`Box`/`Ptr`/`Local` 같은 type constructor는 type-name/member
  chain과 분리해 재귀적으로 `RType*`를 만든 뒤 적용한다. trait 결과에는 적용할 수 없다.
- `S<int>.T` 같은 qualified type-parameter projection은 계속 허용하지 않는다.
- SmTranslator 소유 intermediate 이름의 장기 명명은 `SmIntermediateExp` /
  `SmResolvedExp` 방향이다. 다만 이번 변경에서는 기존 `ImExp`/`ReExp` rename을 함께
  수행하지 않는다.
- `RDecl`/`RNode` 정리는 완료했다. semantic tree 재구성은 현재 작업 주제가 아니다.
- 첫 trait 구현 범위는 원본 module의 canonical conformance로 제한한다: `struct S : Trait`와 대응 `impl S : Trait`.
- generic canonical impl은 `impl S<U> : Trait<U>`처럼 target pattern에 type parameter를 드러내는 표기로 구현한다. `U`는 header가 도입하며 `struct S<T> : Trait<T>`와 alpha-equivalent한 universal target으로 정규화한다.
- v1의 `impl` target은 struct로 한정하므로 semantic target은 일반 `RType*`가 아니라 `RAppliedDecl<RStructDecl>`로 표현한다. nested target의 complete arguments에는 lexical outer slots와 struct 자신의 slots가 모두 들어간다.
- `where`를 가진 full generic impl과 direct specialization은 초기 범위에서 제외한다. specialization/conditional conformance는 named extension bundle과 `extend` activation 경로로 둔다.
- 먼저 trait declaration/type, struct trait 목록, witness `impl` declaration을 RSymbol과 SmTranslator skeleton 단계에 연결한다.
- 이름 있는 외부 `extension` bundle, 소비자 `extend` activation, overlap/ambiguity 처리는 후속 단계다.
- `impl`은 ordinary source name을 바인딩하지 않지만, lexical symbol tree의 internal `RImplTraitDecl` subtree로 둔다. internal tree name은 canonical `RName_ImplTrait`이며 ordinary lookup/overload group에는 노출하지 않는다.
- external symbol lookup을 위해 `RName`(common structured lookup key), `RNodeKey`(same-outer exact tree-child key), `RIdentifier`(module prefix + node-key path), `RTypeIdentifier`(canonical type expression)를 구분한다. identifier는 parser 없이 equality/hash에 쓸 canonical string으로 우선 구현한다.
- `some Trait`는 surface에서는 return-position marker지만 semantic에서는 `RType_Opaque`로 표현한다. opaque type identity는 Swift식으로 return owner declaration과 outer/function의 complete applied generic arguments에 묶는다. `RType_Opaque`는 `appliedTrait`와 `appliedOwnerFunc`를 보관하며, `RTypeIdentifier`는 `$O(TraitRTypeIdentifier,OwnerFuncRIdentifier<FullAppliedArgs...>)`로 encode한다. trait constraint만의 `$O<TraitTypeId>`로 intern하지 않는다.
- `RName`은 normal source name뿐 아니라 type parameter, local/parameter, reserved/compiler name을 포함하는 lookup key다. `RName_CtorParam`처럼 source-spellable하지 않은 generated name도 body lookup에 참여한다. 따라서 현재 `RDeclName` 별도 abstraction은 두지 않으며, exact tree identity는 `RNodeKey`가 담당한다.
- `RNode`를 declaration-space symbol tree의 공통 base로 두고, actual declaration은 `RDecl : RNode`, canonical namespace scope는 `RNamespace : RNode`로 분리한다. `RModule`은 root namespace를 소유하지만 RNode는 아니다. `RNodeKey`/`GetNodeKey()`는 기존 `RDeclKey`를 일반화하며 `RIdentifier`는 global tree-node path가 된다.
- `RNode::GetIdentifier()`는 최종 value API이고, internal virtual append가 하나의 string buffer에 module prefix와 node path를 직접 쓴다. root namespace만 module boundary를 처리한다. RNode는 declaration-space lookup API도 직접 제공하고, class-only inherited lookup은 protected virtual hook으로 둔다.
- symbol tree는 `RModule`마다 하나의 canonical tree를 둔다. compiler-wide module registry는 여러 module tree를 찾기만 하며 declaration을 하나의 global tree에 병합하지 않는다. unit은 tree를 갖지 않고 syntax decl pointer, import/alias overlay, task/cache contribution을 가진 별도 context로 둔다. namespace syntax는 module/path당 하나인 canonical `RNamespace`를 GetOrAdd하여 child declaration을 추가한다. `RNamespaceDeclGroup`은 제거한다.
- `RImplTraitDecl`의 child는 `RImplTraitMemberDecl`, 함수 requirement 구현은 `RImplTraitFuncDecl`이다. 이 함수는 `RFuncDecl`이므로 existing MIR body/ABI/QIR direct-call 경로를 사용한다.
- `NStructInfo`/`NImplTrait*`는 기존 witness payload 모델의 잔재가 되며, conformance header entry와 `RImplTraitDecl*` 연결로 대체하는 방향이다.
- SmTranslator는 unit 간에는 global phase barrier를 유지하고, unit 내부의 세밀한 선행 조건은 order가 있는 task dependency로 표현하는 방향을 검토한다.

## Current Refactoring State
- declaration 구현과 주 번역 경로는 `NSymbol`에서 `RSymbol`로 이행됐다. `RFactory`가 `RDecl`을 소유·생성한다.
- `NSymbol`에는 현재 `NFactory` wrapper와 일부 비주력 target/test의 old API 참조가 남아 있다.
- trait/impl parsing, RSymbol 변환, trait requirement 만족 검사는 구현됐다.
- 현재 `some Trait` opaque return의 호출, 결과 저장소, trait 함수 호출 경로를 구현 중이다.
- MIR→QIR 변환과 전체 테스트 확인은 아직 남아 있다.
- 현재 SmTranslator에 남아 있는 실험적 `SmType`/`SmAppliedDecl`/`SmTypeEnv` 경로는
  제거 대상이며, 이 기록 시점에는 아직 소스에서 제거하지 않았다.
- 자세한 이행 범위와 잔재는 `ai/wiki/compiler/nsymbol-rsymbol-migration.md`를 본다.

## Recently Discussed Points
- namespace 수준의 `public/private`는 source-level use permission을 정한다. 별도 export keyword는 두지 않으며, CTI declaration 노출과 native DLL export table은 구분한다.
- class member는 `public/protected/private`를 갖고, struct member는 `public/private`만 갖는다.
- struct는 현재 상속 불가 방향으로 정리되어 있으므로 struct `protected`는 두지 않는다.
- accessor는 namespace/class/struct에서 이름이 겹치더라도 의미 공간이 다르므로, 단일 universal accessor보다 context별 accessor 분리가 더 자연스럽다는 쪽으로 기울어 있다.
- 다만 이 accessor를 `RNode` 본체에 직접 넣기보다, declaration payload나 별도 accessibility policy 계층에 두는 쪽이 현재 `RNode` 방향과 더 잘 맞는다.
- generic type parameter는 declaration이 소유하는 lexical binder이며 qualified member tree의 child가 아니다. 따라서 member body의 `T` substitution은 지원하되 `S<int>.T` projection은 제공하지 않는 방향을 검토 중이다.
- type lookup은 current header의 binder만 보는 경우와 normal member lookup을 구분한다. inheritance lookup은 outer lookup과 별개이며, `ResolveInheritedTypeMember`/`ResolveInheritedMember`처럼 applied base type arguments를 유지하는 좁은 hook 후보를 검토 중이다.
- Citron은 generic definition을 `RTypeDecl`로 두고 unbound `RType`은 만들지 않는다. `RType`은 `S<T>`(open) 또는 `S<int>`(closed)처럼 arguments가 적용된 type만 나타낸다. `RDeclRes`의 outer-applied 상태는 type이 아니라 lookup declaration context다.
- `RDeclRes` 반환 여부가 아니라 탐색 범위로 `Get`/`Resolve`를 구분하도록 적용했다. direct lookup 결과는 `ROuterAppliedDecl`/`RAppliedDecl`로 표현하고, 함수의 explicit type argument prefix는 RSymbol result가 아니라 SmTranslator의 `SmPartiallyAppliedFuncDeclGroup`에 둔다.
- `RAppliedDecl`은 declaration의 모든 formal type parameter에 argument가 전달된
  relation이다. argument 자체는 open type variable일 수 있으므로 fully-applied와
  closed를 같은 뜻으로 쓰지 않는다. 자체 type parameter가 없는
  lambda/variable/enum element도 lexical outer arguments까지 적용됐다면
  `RAppliedDecl`을 사용한다. member 자신의 type arguments를 아직 받지 않은
  nominal type/function lookup은 `ROuterAppliedDecl` 또는 partial-application
  결과로 유지하되, requirement signature 비교는 새 type variable을 해당 member
  argument로 적용해 `RAppliedDecl`을 만든다.
- trait call은 call-site에서 source-local `NImplTraitFunc`를 보관하지 않는다. semantic `Trait` call은 `RTraitFuncDecl`과 applied trait identity를 보관하고, lowering이 concrete conformance에는 direct impl call을, generic constraint와 `some Trait` opaque value에는 trait table call을 선택한다.
- `RImplTraitDecl` subtree는 impl syntax의 lexical outer에 둔다. target struct와 matched conformance header는 typed relation으로 따로 둔다.
- `RImplTraitDecl`은 ordinary `RName` member lookup scope가 아니다. trait requirement implementation은 `RTraitFuncDecl -> RImplTraitFuncDecl` relation으로 찾으며, future extension-private helper scope에만 별도 name index 필요성을 재검토한다.
- extension은 trait impl subtree에 `RImplTrait*` 계열을 재사용하는 쪽을 우선 검토한다. `RExtensionFuncDecl`은 bundle-private helper로 별도 계열이다.

## Open Questions
- 선언 접근성 검사 구현: `Access(D) ⊆ Access(S)`의 protected/outer 비교 API, alias normalization 전에 이름의 접근성을 검증하거나 provenance를 유지하는 방식, 검사 시점은 미정이다. 소스 구현은 아직 변경하지 않았다.
- 후보 (2026-08-31): type alias target과 class direct base를 BuildTypeHierarchy 내부의 demand-driven resolver로 함께 해석하는 안. 미완료 inherited lookup의 outer fallback 금지, resolution/inheritance cycle 구분, 기존 unit-local/lower-order-only scheduling 안과의 충돌은 미확정이다. `ai/notes/2026-08-31-type-alias-and-base-resolution.md` 참고.
- 보류 (2026-08-31): Windows DLL export 방식과 한도 대응은 지금 결정하지 않는다. 초과 진단, 선택적 export, 자체 module table, DLL 자동 분할은 후보로만 유지한다. 현재 type alias/associated type 작업의 선행 조건으로 삼지 않으며, native module linking 설계 시 재검토한다.
- accessor를 정확히 어느 계층에 둘지: declaration payload, category view, 별도 metadata 중 어디가 가장 자연스러운지
- lookup / resolver 책임과 `RNode` 책임의 경계를 어디까지 나눌지
- nested type의 accessibility를 tree membership과 declaration accessibility 사이에서 어떻게 모델링할지
- generic conformance header의 generic signature, trait type arguments, constraint를 어느 RSymbol API로 노출할지. Self target은 `RAppliedDecl<RStructDecl>`로 확정했다. Source header에서는 `impl S<U>`/`impl Bundle<U>`의 target pattern parameter와 `extend<U>` activation parameter를 구분해 표현한다.
- generic bundle pattern의 overlap을 `where` constraint까지 포함해 activation 시점에 어떻게 판정할지
- trait별 independent witness/conformance identity를 유지하면서 관련 trait 구현의 공용 helper/member를 bundle-private scope나 별도 mechanism으로 어떻게 제공할지. impl block에 trait requirement 밖 member를 허용할지 여부
- unit-local task graph의 freeze 시점, task order enum, failure propagation 규칙
- type-parameter binder/type member의 namespace collision 및 shadowing 규칙, header/body/inherited/outer type lookup의 정확한 우선순위
- `RTypeParam`의 `RDecl`/`RTypeDecl` 분리 뒤 type-name lookup result를 어떤 union/result shape로 나타낼지
- `MCallable`을 semantic `Trait` call로 유지할지, 어느 IR stage에서 `Direct`/`TraitTable`/`Virtual` call target으로 분해할지
- trait table call의 ABI shape: generic constraint dictionary, opaque metadata의 witness entry, direct conformance symbol의 관계
- `$T0` binder slot numbering, passing kind/function generic signature의 overload identity, alias normalization API
- complete type-argument 배열의 source declaration/signature를 debug build에서
  검증할지, slot count와 construction invariant만으로 둘지
- `GetGlobalIndex()`를 `GetTypeArgumentIndex()`처럼 실제 의미가 드러나는 이름으로
  바꿀지
- `RNodeKey`의 structured form 대 string form, category 간 same-name collision, exact key seal/register 시점
- `RNode` API 추출 순서와 existing `RDecl` users의 migration 범위

## Update Rule
- 현재 주제가 바뀌면 이 파일을 먼저 갱신한다.
- 완전히 확정된 규칙만 `ai/wiki/current-decisions.md`로 옮긴다.
- 세부 대화나 실험 결과는 `ai/notes/`에 남기고, 이 파일에는 현재 작업을 이어가기 위한 최소 맥락만 유지한다.
