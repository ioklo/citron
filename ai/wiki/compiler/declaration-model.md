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

## Generic Applied Declaration And Type Parameters

- `RAppliedDecl`은 `decl`과 outer부터 member 자신까지 모두 적용된 complete
  `RTypeArguments`만 보관한다. 적용 위치의 `RTypeEnv`/`tenv`를 `RAppliedDecl`이나
  중첩 `RType`에 넣지 않는다.
- `RTypeParam::GetGlobalIndex()`는 compiler 전체에서 유일한 번호가 아니라, 해당
  declaration의 lexical outer chain을 평탄화한 complete type-argument index다.
  새 declaration의 local type parameter index는
  `outer->GetAllTypeParamCount() + localIndex`로 정한다.
- `RType_TypeVar::Apply`는 자신의 global index로 complete `RTypeArguments`를
  조회한다. 따라서 별도의 환경 배열 없이도 nested declaration, base application,
  trait requirement substitution을 처리할 수 있다.
- type parameter를 인자로 적용했다는 것과 application이 closed라는 것은
  구분한다. 예를 들어 `X<T>`는 모든 formal parameter에 argument가 전달된
  `RAppliedDecl`이지만 argument `T`가 open type variable이다.
- exact type/application equality는 `RTypeParam*` binder identity를 포함한
  declaration/type-argument identity로 판정한다. `A.X`와 `F.Y`가 모두 global
  index 0이어도 exact type으로는 다르다.
- alpha-equivalence는 두 generic signature가 대응한다는 전제에서만 검사한다.
  `GetGlobalIndex()`를 `$0`, `$1`, ... canonical slot으로 사용할 수 있지만,
  requirement binder를 impl binder로 치환한 뒤 exact equality를 사용하는 방법을
  우선 검토한다.
- source type-name lookup에서 현재 보이는 type parameter scope가 필요하면
  translator의 임시 lookup context로 둔다. 이는 semantic `RType` 또는
  `RAppliedDecl`의 identity/state가 아니다.

## Related Open Points
- Exact fields filled at fdecl / decl / impl states for each declaration kind.
- `RImplTraitDecl`의 target struct member lookup, impl generic binder, lexical outer lookup을 body context에서 어떤 우선순위로 결합할지.
- global identifier의 `$T0` binder-slot numbering, alias normalization, future extension/specialized target pattern identity.
- alpha-equivalence를 global-index canonical comparison과 binder substitution 중
  어느 API로 통일할지.
- extension declaration과 separate `impl Bundle ...` syntax가 있을 때 `RImplTraitDecl`의 tree outer 및 `RExtensionDecl` conformance entry 연결 방식.
- How `cti` generated declaration surface maps into `EDecl` / `REDecl`.
- How opaque result identity for `some` return attaches to declaration identity.

## History
- `git history: ai/implementations/decl-model.md`
- `ai/notes/2026-06-27-rdecl-rnode-and-lookup-direction.md`
- `ai/notes/2026-05-12-module-visibility-and-internal-fdecl-direction.md`
- `ai/notes/2026-07-15-generic-impl-and-specialized-conformance.md`
