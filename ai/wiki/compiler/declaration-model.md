# Declaration Model

Status: implemented current
Area: compiler, symbol
Keywords: RDecl, RNode, NDecl, EDecl, REDecl, declaration, skeleton, fdecl, symbol tree

## Current Rules
- Declaration/symbol tree ownership is per `RModule`: all units of one module contribute to its one canonical tree.  A translation unit has no independent symbol tree; its syntax declaration pointers, imports/aliases, task state and incremental contribution are held in a separate unit context.  A compiler-wide module registry contains multiple module trees rather than one merged declaration tree.
- `RNode` is the common declaration-space tree base. `RDecl : RNode` represents an actual declaration; `RNamespace : RNode` is a canonical named namespace scope and is not an actual declaration. `RModule` owns root `RNamespace` but is not an `RNode`. Namespace syntax gets or creates the one node for its module/path and adds children to it; `RNamespaceDeclGroup` is not used.
- Runtime/intermediate declaration interface is unified as `RDecl`.
- Source declarations and externally exposed declarations remain distinct as `NDecl` and `EDecl`.
- Compilation phases use `RDecl` as the common interface.
- Common declaration-related sum types such as `RFuncDecl`, `NFuncDecl`, `RDeclRes`, and `BodyRes` use thin wrapper classes over `std::variant` rather than public type aliases.
- Prefer domain methods on those wrappers (`GetRDecl()`, `GetRFuncDecl()`, `GetFuncDeclWithOuterTypeArgs()`, etc.) over scattering free helper functions at call sites.
- The semantic declaration tree reconfiguration around `RNode` is complete; it is no longer an active migration task.
- `RTypeDecl`, `RFuncDecl`, `RTypeDeclOuter`, and `RFuncDeclOuter` are better understood as category views over semantic declarations than as the tree node abstraction itself.
- Terminology: use `outer` for the containing element in the symbol tree, `base` for inheritance relationships, and avoid `parent` because it is ambiguous between those axes.
- Current leaning: `RNode` should be a lightweight tree/name carrier first. Richer declaration identity such as generic arity and callable signature belongs in declaration/category metadata rather than the node name key itself.
- Type declarations follow a C++-like rule: do not allow same-name generic type families distinguished only by generic arity in the same scope.

## Declaration Kinds
- `NDecl` is created directly from source.
- `EDecl` is an externally exposed declaration.
- `REDecl` wraps `EDecl` and implements the `RDecl` interface.
- `RDecl` is the common declaration interface used by translation and intermediate representation phases.
- Current leaning is that `NDecl` is weaker as an independent tree abstraction and stronger as a source-origin contract/payload surface.

## Pipeline Implications
- Later phases, including `SyntaxIR0Translator`, create/register required declarations through `RDecl`.
- External declarations and source declarations can be handled through one runtime-facing interface.
- Skeleton/fdecl collection can establish stable declaration identity before complete surface/body is known.
- Wrapper-based sum types keep declaration-specific operations close to the type while still allowing internal `Visit(...)` dispatch where a real sum-type branch is needed.
- `RDecl`의 `GetTypeParam`, `GetTypeMember`, `GetMember`는 outer recursion 없이 현재 declaration scope만 조회하는 공통 API다. type parameter는 `GetTypeParam`으로만 얻고, `GetMember`에는 넣지 않는다.
- `GetMember`는 single declaration과 function overload group을 나타낼 수 있는 `RMember`를 반환한다. `RMember`는 현재 scope의 named member lookup result이며 declaration tree node나 generic binder가 아니다.
- `RName`은 normal source name, type parameter, local variable, function parameter, reserved compiler name 등 RSymbol 전반에서 쓰이는 구조화 lookup key다. `RName_CtorParam`처럼 source-spellable하지 않은 compiler-generated name도 body lookup에 참여할 수 있다. `RDecl`은 lookup surface가 필요한 경우에만 `RName`을 가지며 별도 `RDeclName` abstraction은 현재 두지 않는다. `RNodeKey`는 same-outer에서 tree child 하나를 가리키는 exact key로 `RName`과 분리한다. module부터 node path를 따라 tree node 하나를 가리키는 global identity는 `RIdentifier`다.
- `RNode`의 `ResolveTypeIdentifier`, `ResolveTypeIdentifierInHeader`, `ResolveIdentifier`는 `Get*`을 사용해 current scope를 조회한 뒤 outer lexical scope로 재귀한다. `ResolveIdentifier`은 type parameter를 `RDeclRes_TypeVar`로 반환할 수 있다. class base-chain only hook인 `ResolveInheritedTypeMember`/`ResolveInheritedMember`는 RNode의 protected virtual default이며 RClassDecl이 override한다.
- type parameter는 lexical type-name/identifier lookup에는 참여하지만 qualified member surface에는 참여하지 않는다. 따라서 `S<int>.T` 같은 projection은 허용하지 않는다.
- Accessibility policy is likely to split between module/namespace member rules and type-member/inheritance rules, so `RNode` should not assume a single tree-only access algorithm.

## Impl Trait Declaration Direction

- `impl`은 ordinary source name을 바인딩하지 않지만, body/generic scope/callable identity를 표현하기 위해 lexical symbol tree의 internal `RImplTraitDecl` subtree로 둔다.
- `RImplTraitDecl`은 canonical internal node key를 사용한다. 이 key와 declaration은 ordinary `GetMember(RName_Normal)` lookup과 ordinary function overload group에는 노출하지 않는다. impl key는 `$I(TargetRNodeKey,TraitRTypeIdentifier)`로 encode한다. target은 current lexical outer의 direct type member로 제한하므로 local `RNodeKey`만 쓰고, trait type은 같은 module인 경우에도 module prefix를 포함한 `RTypeIdentifier`를 쓴다.
- `RImplTraitDecl`은 syntax의 lexical outer를 tree outer로 두고, target struct 및 matched conformance header를 typed field로 둔다. target struct member lookup과 `this`는 impl-specific body lookup policy로 처리한다. target struct를 lexical outer로 사용하지 않는다.
- `RImplTraitDecl`은 ordinary `GetMember(RName)` lookup scope가 아니다. trait requirement implementation은 `RTraitFuncDecl`과 대응 `RImplTraitFuncDecl`의 typed relation으로 찾는다. future extension/private-helper scope가 unqualified helper call을 지원할 때만 별도 member name index를 검토한다.
- trait requirement implementation member는 `RImplTraitMemberDecl`으로 나타내며, 함수 requirement의 구현은 `RImplTraitFuncDecl : RImplTraitMemberDecl + RFuncDecl`로 둔다. 따라서 일반 함수와 같은 `RFuncDecl`/`MFuncBody`/ABI/QIR 경로를 사용할 수 있다.
- `NStructInfo`, `NImplTrait`, `NImplTraitFunc`는 witness implementation의 authoritative owner가 아니다. 현재 `NStructInfo`가 `implTraits`만 보관하므로, 이 설계로 이행하면 제거한다.
- canonical conformance header는 `RStructDecl`의 conformance entry가 소유하고, bind 후 해당 entry가 `RImplTraitDecl*`를 연결한다. public header와 witness identity는 declaration surface에 남기되 CTI에는 synthetic tree name을 기록하지 않는다.
- generic conformance header는 단순 `(trait, traitTypeArgs)`가 아니라 generic signature, owner struct의 formal parameter에 적용하는 self type-argument pattern, trait type arguments, constraint를 함께 나타낸다. 예를 들어 `impl<T> S<T> : Trait`의 self pattern은 `[T]`이고, `impl Bundle for S<int> : Trait`의 self pattern은 `[int]`다.
- extension bundle도 trait requirement implementation 자체는 동일한 `RImplTraitDecl` / `RImplTraitMemberDecl` / `RImplTraitFuncDecl` 계열을 재사용하는 방향이다. `RExtensionFuncDecl`은 bundle-private 공용 helper로 별도 계열이며 trait requirement member가 아니다.

## Generic Applied Declaration Context

- `RAppliedDecl`의 type argument는 concrete type만이 아니라 open type
  variable일 수 있다. 따라서 type argument가 모두 전달됐다는 것과 결과가
  closed type이라는 것은 구분한다.
- 모든 generic analysis는 type arguments와 별도로, 적용 위치의 ordered lexical
  type-variable environment(`tenv`)를 이용할 수 있어야 한다. tenv를
  `RAppliedDecl`이 직접 보관할지, declaration/body context가 제공할지는 아직
  미확정이다. `tenv`는 application 안에 실제로 등장하는 자유 type variable만의
  목록이 아니라 그 위치에서 보이는 전체 type variable 환경이다. 예를 들어 다음은
  서로 다른 정보다.

  ```text
  tenv:      [T1, T5, T3]
  application: X<T1>.Tr<list<T5>>.F<T3>
  full args: [T1, list<T5>, T3]
  ```

  `full args`는 `F`의 formal parameter `[T1, T2, T3]`에 대한 적용이고,
  `tenv`는 적용 위치의 lexical type-variable order다.
- 그러므로 closed application도 tenv를 가진다. 예를 들어
  `struct S<X> { struct T<Y> { void F() { new List<int>(); } } }`에서
  `List<int>`는 closed type이지만 `tenv [X, Y] => List<int>`로 표현한다.
  정규화할 type variable이 없으므로 결과 type은 그대로 `List<int>`다.
- trait requirement implementation을 검사할 때 requirement와 impl의 반환형,
  인자형, 제약은 각각 자신이 속한 `tenv`와 함께 비교한다. 단, 먼저 두 generic
  signature의 binder 위치가 대응된다는 것을 확인해야 한다. 그 뒤 각 tenv의
  위치를 `$0`, `$1`, ...로 정규화한 결과가 같으면 type parameter의 source name이
  달라도 alpha-equivalent하다.
- 예를 들어 `X<T1>.Tr<list<T5>>.F<T3>`의 반환형
  `tenv [T1, T5, T3] => list<T5>`와 impl 함수의 반환형
  `tenv [T1, T5, T6] => list<T5>`는 모두 `list<$1>`로 정규화된다. 인자형
  `T3`와 `T6`은 모두 `$2`로 정규화된다.
- `tenv`는 `std::vector`를 값마다 복사해 보관하지 않는다. 빈 environment
  singleton 및 persistent outer link를 가진 immutable `RTypeEnv` 같은 공유
  표현을 사용한다. `RAppliedDecl`이 그 handle/pointer를 직접 보관할지,
  decl-space/body-space가 공유해 제공할지는 구현 시 확정한다.
- exact application equality는 declaration과 applied type arguments의 origin을
  비교하며 tenv를 보지 않는다. 예를 들어 `List<A.X>`와 `List<F.Y>`는 다르지만,
  서로 다른 lexical context에서 얻은 두 `List<int>`는 같다. alpha-equivalence는
  명시적인 binder 대응을 전제로 하는 별도 API다. 현재 context에서 사용할 수
  있는지의 검사는 equality가 아니라 별도 validity API로 둔다.

## Related Open Points
- Exact fields filled at fdecl / decl / impl states for each declaration kind.
- `RImplTraitDecl`의 target struct member lookup, impl generic binder, lexical outer lookup을 body context에서 어떤 우선순위로 결합할지.
- global identifier의 `$T0` binder-slot numbering, alias normalization, future extension/specialized target pattern identity.
- shared `RTypeEnv`의 ownership/interning, tenv의 저장 위치(`RAppliedDecl`
  대 decl-space/body-space), alpha-equivalence용 binder correspondence의 API.
- extension declaration과 separate `impl Bundle ...` syntax가 있을 때 `RImplTraitDecl`의 tree outer 및 `RExtensionDecl` conformance entry 연결 방식.
- How `cti` generated declaration surface maps into `EDecl` / `REDecl`.
- How opaque result identity for `some` return attaches to declaration identity.

## History
- `git history: ai/implementations/decl-model.md`
- `ai/notes/2026-06-27-rdecl-rnode-and-lookup-direction.md`
- `ai/notes/2026-05-12-module-visibility-and-internal-fdecl-direction.md`
- `ai/notes/2026-07-15-generic-impl-and-specialized-conformance.md`
