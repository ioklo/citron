- ```std::expected```는 접두사 ```e_```를, ```std::optional```은 접두사 ```o_```를 붙인다
사용할때 ```*```로 참조해야할거라는걸 알게된다
```cs

std::expected<MLoc*, DiagPtr> GetMLoc();
std::optional<size_t> GetIndex();

auto e_mLoc = GetMLoc();
auto o_index = GetIndex(); 
```

- 접두어만 다른 리턴값을 가지는 interface는 Get다음에 접두어를 붙인다

```
GetBaseStruct는

NStructDecl* GetNBaseStruct(); // GetBaseNStruct하면 N을 어디에 붙일지 헷갈리기 시작한다 
RStructDecl* GetRBaseStruct(); // 무조건 Get다음에 붙이는 것으로
```

- ```std::variant```는 직접 노출하지 않고 class로 감싸서 사용한다
  - wrapper class 내부에 ```using Variant = std::variant<...>;```를 둔다.
  - forwarding constructor를 두되 자기 자신 타입은 제외한다.
  - 필요하면 ```Visit``` member function으로 ```std::visit```을 감싼다.

```cpp
class RName
{
    using Variant = std::variant<RName_Normal, RName_Reserved>;

    Variant v;

public:
    template<typename T> requires (!std::same_as<std::remove_cvref_t<T>, RName>) && std::constructible_from<Variant, T&&>
    RName(T&& t) : v{std::forward<T>(t)} {}

    template<typename... TArgs>
    auto Visit(TArgs&&... args) { return std::visit(std::forward<TArgs>(args)..., v); }
};
```

- class member 순서는 대략 다음 순서를 따른다
  - nested type 정의
  - member variable
    - 생성자에서 바로 세팅되는 변수
    - 동적으로 채워지는 변수
    - component
    - 외부에서 주입한 dependency
  - class의 일반 함수
  - class의 interface 함수
    - 상속 순서대로 배치한다.
    - interface section은 ```public: // from Interface``` 형태로 표시한다.

- override하는 interface 함수는 특별한 이유가 없으면 cpp에 구현한다

- 다중 interface 구현 중 이름이 충돌하면 interface 이름을 함수 이름 앞에 붙인다
  - 예: ```RTypeDecl_GetDecl()```, ```RFuncDecl_GetDecl()```
  - 되도록 대표 함수 하나만 노출하고, 중복되는 함수는 제거한다.
  - 다른 interface의 같은 개념이 필요하면 해당 interface를 통해 대표 declaration을 얻은 뒤 호출한다.

- reference parameter와 reference/value return helper를 사용한다
  - ```const T&``` parameter는 ```InRef<T>```로 쓴다.
  - ```T&&```만 단독으로 필요하면 ```T&&```를 쓴다.
  - ```const T&```와 ```T&&```를 모두 받을 수 있어야 하면 ```TakeRef<T>```를 쓴다.
  - ```TakeRef<T>```를 변수에 대입할 때는 ```Take()```를 쓴다.
  - 함수 return에서 ```T&```와 ```T```를 모두 허용해야 하면 ```RefOrOwn<T>```를 쓴다.

- component는 private inheritance보다 member variable로 포함한다

- constructor만으로 완성하기 어려운 객체는 constructor와 ```Init...``` 함수로 단계 초기화를 분리할 수 있다
  - 순환 참조, factory 구성, type parameter와 body 정보의 지연 주입처럼 한 번에 완성하기 어려운 경우에만 사용한다.
  - 현재는 ```Init...``` 호출 누락을 강제 확인하는 공통 장치가 없으므로, 새로 도입할 때는 호출 순서와 소유자를 명확히 둔다.
