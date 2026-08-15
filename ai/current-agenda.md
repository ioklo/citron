# Current Agenda

## Topic
Lazy generic type substitution and trait requirement signature matching

## Current Direction
- `RType`은 canonical semantic type expression으로 유지한다. SmTranslator의 generic
  계산 상태는 mirrored `SmType` hierarchy가 아니라
  `SmTypeView { RType*, SmTypeSubstitution* }`로 표현하는 방향이다.
- `SmTypeSubstitution`은 `RTypeParam* formal -> SmTypeView actual` mapping을 persistent
  chain으로 보관한다. actual도 caller의 substitution 아래에 있을 수 있으므로 bare
  `RType*`가 아니라 contextual view여야 한다.
- generic member projection과 signature comparison에서는 치환된 `RType` tree를
  즉시 만들지 않는다. 원본 `RType`과 substitution을 함께 전달하고, parameter
  projection, member lookup, type equality처럼 관찰이 필요한 지점에서만 resolve한다.
- lexical/binder context와 substitution을 구분한다. context는 lookup과 constraint에
  사용하고, substitution은 callee formal을 actual type view에 대응시킨다. 적용
  위치의 full `RTypeEnv`를 `RType`/`RAppliedDecl` identity에 넣지 않는다.
- exact type equality는 substitution을 필요한 만큼 resolve한 뒤 `RTypeParam*` binder
  identity를 비교한다. 대응하는 generic callable의 alpha-equivalence는 requirement
  binder를 impl binder로 ordinal mapping한 뒤 exact view comparison으로 환원한다.
- trait requirement와 impl 비교는 applied trait의 outer substitution에
  `trait.F.Tn -> impl.F.Tm` binder mapping을 결합해 lazy signature view끼리 비교한다.
  fresh binder나 완전히 instantiate된 signature tree를 만들지 않는다.
- `RType::Apply`는 interning, serialization, lowering contract처럼 canonical type
  materialization이 필요한 경계의 helper로 남을 수 있으나 generic 분석의 기본
  계산 모델로 사용하지 않는다.
- `ROuterAppliedDecl`/`RAppliedDecl`의 canonical RSymbol 역할은 유지하되,
  `SmAppliedDecl`/`SmTypeView`와의 정확한 책임 경계는 구현 설계가 남아 있다.
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
- trait/impl은 parser/AST까지만 연결돼 있으며, `RTraitDecl`, trait type/factory, SmTranslator visitor/task는 아직 구현 대상이다.
- merge된 `e84dc27c`의 mirrored `SmType`/index-based `SmType_TypeVar`/
  `RTypeToSmType` 구현은 이전 WIP prototype이다. 현재 설계는 이를 `SmTypeView`와
  lazy substitution으로 대체하는 방향이며 source migration은 아직 시작하지 않았다.
- 자세한 이행 범위와 잔재는 `ai/wiki/compiler/nsymbol-rsymbol-migration.md`를 본다.

## Recently Discussed Points
- namespace 수준의 `public/private`는 별도 export 키워드가 아니라 accessibility를 통해 export 의미를 포함한다.
- class member는 `public/protected/private`를 갖고, struct member는 `public/private`만 갖는다.
- struct는 현재 상속 불가 방향으로 정리되어 있으므로 struct `protected`는 두지 않는다.
- accessor는 namespace/class/struct에서 이름이 겹치더라도 의미 공간이 다르므로, 단일 universal accessor보다 context별 accessor 분리가 더 자연스럽다는 쪽으로 기울어 있다.
- 다만 이 accessor를 `RNode` 본체에 직접 넣기보다, declaration payload나 별도 accessibility policy 계층에 두는 쪽이 현재 `RNode` 방향과 더 잘 맞는다.
- generic type parameter는 declaration이 소유하는 lexical binder이며 qualified member tree의 child가 아니다. 따라서 member body의 `T` substitution은 지원하되 `S<int>.T` projection은 제공하지 않는 방향을 검토 중이다.
- type lookup은 current header의 binder만 보는 경우와 normal member lookup을 구분한다. inheritance lookup은 outer lookup과 별개이며, `ResolveInheritedTypeMember`/`ResolveInheritedMember`처럼 applied base type arguments를 유지하는 좁은 hook 후보를 검토 중이다.
- Citron은 generic definition을 `RTypeDecl`로 두고 unbound `RType`은 만들지 않는다. `RType`은 `S<T>`(open) 또는 `S<int>`(closed)처럼 arguments가 적용된 type만 나타낸다. `RDeclRes`의 outer-applied 상태는 type이 아니라 lookup declaration context다.
- `RDeclRes` 반환 여부가 아니라 탐색 범위로 `Get`/`Resolve`를 구분하도록 적용했다. direct lookup 결과는 `ROuterAppliedDecl`/`RAppliedDecl`로 표현하고, 함수의 explicit type argument prefix는 RSymbol result가 아니라 SmTranslator의 `SmPartiallyAppliedFuncDeclGroup`에 둔다.
- `RAppliedDecl`은 declaration의 모든 formal slot에 argument가 전달된 canonical
  relation이며, applied와 closed는 구분한다. member 자신의 type arguments가 아직
  없는 nominal type/function lookup은 `ROuterAppliedDecl` 또는 partial application
  상태로 유지한다. SmTranslator가 parameter type이나 requirement signature를
  관찰할 때는 새 type tree를 만들지 않고 `SmTypeView`에 lazy substitution을 붙인다.
- trait call은 call-site에서 source-local `NImplTraitFunc`를 보관하지 않는다. semantic `Trait` call은 `RTraitFuncDecl`과 applied trait identity를 보관하고, lowering이 concrete conformance에는 direct impl call을, generic constraint와 `some Trait` opaque value에는 trait table call을 선택한다.
- `RImplTraitDecl` subtree는 impl syntax의 lexical outer에 둔다. target struct와 matched conformance header는 typed relation으로 따로 둔다.
- `RImplTraitDecl`은 ordinary `RName` member lookup scope가 아니다. trait requirement implementation은 `RTraitFuncDecl -> RImplTraitFuncDecl` relation으로 찾으며, future extension-private helper scope에만 별도 name index 필요성을 재검토한다.
- extension은 trait impl subtree에 `RImplTrait*` 계열을 재사용하는 쪽을 우선 검토한다. `RExtensionFuncDecl`은 bundle-private helper로 별도 계열이다.

## Open Questions
- accessor를 정확히 어느 계층에 둘지: declaration payload, category view, 별도 metadata 중 어디가 가장 자연스러운지
- lookup / resolver 책임과 `RNode` 책임의 경계를 어디까지 나눌지
- nested type의 accessibility를 tree membership과 declaration accessibility 사이에서 어떻게 모델링할지
- generic conformance header의 generic signature, self type-argument pattern, trait type arguments, constraint를 어느 RSymbol API로 노출할지. Source header에서는 `impl S<U>`/`impl Bundle<U>`의 target pattern parameter와 `extend<U>` activation parameter를 구분해 표현한다.
- generic bundle pattern의 overlap을 `where` constraint까지 포함해 activation 시점에 어떻게 판정할지
- trait별 independent witness/conformance identity를 유지하면서 관련 trait 구현의 공용 helper/member를 bundle-private scope나 별도 mechanism으로 어떻게 제공할지. impl block에 trait requirement 밖 member를 허용할지 여부
- unit-local task graph의 freeze 시점, task order enum, failure propagation 규칙
- type-parameter binder/type member의 namespace collision 및 shadowing 규칙, header/body/inherited/outer type lookup의 정확한 우선순위
- `RTypeParam`의 `RDecl`/`RTypeDecl` 분리 뒤 type-name lookup result를 어떤 union/result shape로 나타낼지
- `MCallable`을 semantic `Trait` call로 유지할지, 어느 IR stage에서 `Direct`/`TraitTable`/`Virtual` call target으로 분해할지
- trait table call의 ABI shape: generic constraint dictionary, opaque metadata의 witness entry, direct conformance symbol의 관계
- `$T0` binder slot numbering, passing kind/function generic signature의 overload identity, alias normalization API
- `SmTypeSubstitution`의 owner/lifetime을 `SmFactory` arena와 persistent immutable
  object 중 어느 방식으로 둘지
- canonical `RAppliedDecl`/`RTypeArguments`와 transient
  `SmAppliedDecl`/`SmTypeView`의 정확한 책임 경계
- lazy `ResolveTypeVar`, member lookup, equality, materialization API와 chain
  flattening/memoization 정책
- declaration-owned bound `RTypeParam`과 inference/skolem variable의 표현을 어떻게
  분리할지
- `RNodeKey`의 structured form 대 string form, category 간 same-name collision, exact key seal/register 시점
- `RNode` API 추출 순서와 existing `RDecl` users의 migration 범위

## Update Rule
- 현재 주제가 바뀌면 이 파일을 먼저 갱신한다.
- 완전히 확정된 규칙만 `ai/wiki/current-decisions.md`로 옮긴다.
- 세부 대화나 실험 결과는 `ai/notes/`에 남기고, 이 파일에는 현재 작업을 이어가기 위한 최소 맥락만 유지한다.
