## 실행 환경
  - 용어와 저장공간 
    - 용어 (값, 인스턴스, 함수 프레임, task) <- 타입에 해당
    - 저장공간 (현재 함수 프레임, 현재 네임스페이스, 값 저장소, 인스턴스 저장소) <- 인스턴스에 해당

  - component
    - value-id, value
      
    - instance-id, instance

    - variable-scope
      - prev-variable-scope: variable-scope

    - task-scope
      - prev-task-scope: task-scope

    - function-context
      - this-value?, 
      - outer-type?
      - type-variables: typevar-id => typeid
      - local-variable-scope: variable-scope
      - local-task-scope: task-scope
      - flow-control: flow-control
      - saved-function-context: function-context
      - saved-sequence-caller-context: function-context

    - task
      + get-result

  - item-path  
  
  - global-path : item-path
    - namespace-path
    - string

  - member-item-path : item-path
    - item-path
    - string

  - context  
    - type-query-service: item-path => type-decl      // 전역타입선언, 멤버타입선언
    - item-query-service: item-path => item           // 전역타입선언, 전역함수선언,  멤버타입선언, 멤버함수선언, 멤버변수선언    

    - value-repo: value-id => value                   // 
    - instance-repo: instance-id => instance
    - function-context: function-context

  - 값 [[value]]
    - class 값 [[class-value]]
    - struct 값 [[struct-value]]
    - enum 값 [[enum-value]]
    - interface 값 [[interface-value]]
    - tuple 값 [[tuple-value]]
    - box 값 [[box-value]] <- 이거 클래스로 퉁 쳐질듯
    - ref 값 [[ref-value]]
    - nullable 값 [[nullable-value]]
    - void 값 [[void-value]]
    - null 값 [[null-value]]

    - seq 값 [[seq-value]]
      - Next 실행시 함수 프레임 <- 값이 아니다

    - lambda 값 [[lambda-value]]
      - Invoke 실행시 함수 프레임 <- 값이 아니다

  - 인스턴스
    - member-var-id => value
    - virtual-member-func-id => 함수 선언

    - class 인스턴스 <- 값이 아니다 class 값으로 부터 참조된다
    - box 인스턴스

  - task

  * 함수 프레임 [[function-frame]]
    * this 값 [[this-value]]

    * 함수 선언 타입(This) [[this-type]]
    * 지역 변수 스코프 [[local-variable-scope]]
      - local-var-id => value-id
      - 이전 지역 변수 스코프
    * 지역 task 스코프 [[local-task-scope]]
      - 지금은 task가 값이 아니다
      - 이전 지역 task 스코프
    * flow-control flag (다음 실행위치) [[flow-control]]
    * return 값 // 은 필요없을듯? [[return-value]]
    * 타입 변수 컨텍스트 [[type-variables]]
    * 이전 함수 프레임 (return 시 돌아갈) [[saved-function-frame]]
    * yield 로 돌아갈 프레임 [[saved-sequence-frame]]

  - 현재 네임스페이스 [[current-namespace]]

  - 전역 이름 검색 공간 (선언으로 여기 등록) [[global-id-space]]
    - (네임스페이스, 이름) -> {value-id, 함수선언, 타입선언, 타입Alias} // 전역
    - (타입, 이름) -> {value-id, 함수선언, 타입선언, 타입Alias}         // 멤버
    - 레퍼런스

  - 전역 타입 이름 검색공간 (선언으로 여기 등록) [[global-typeid-space]]
    - (네임스페이스, 타입) -> {타입, 타입Alias}
    - (타입, 이름) -> {타입}
    - 레퍼런스

  - 값 저장소 [[value-repository]]
    - value-id => value
    
  - 인스턴스 저장소 (가비지컬렉션 대상) [[instance-repository]]
    - instance-id => instance

## 행동
  - 전역 변수 
    * 전역 변수 선언 [[declare-global-variable]] (루트 네임스페이스에서만 가능)
    * 전역 변수 참조 [[reference-global-variable]]

  - 전역 함수
    * 전역 함수 선언 [[declare-global-function]]
    * 전역 함수 호출 [[call-global-function]]

  - 지역 변수
  * 지역 변수 선언 [[declare-local-variable]]
    * 지역 변수 참조 [[reference-local-variable]]
    * 지역 변수 새 스코프 열기 [[open-new-local-variable-scope]]

  - 대입
    * assign [[assign-value]]

  - 예외
    * throw [[throw-exception]]  
    * try ... catch [[catch-exception]]
    * pass-thru exception [[passthru-exception]]

  - flow control
    - test
      * if (분기문(값에 따라서 실행할 명령어를 다르게 하기)) [[branch-by-bool-value]]
      * if class (클래스 타입 테스트를 하고, 변수의 타입을 테스트한 타입으로 바꾸기) [[branch-by-class-value]]
      * if enum (enum 멤버 테스트를 하고, 변수의 타입을 테스트한 타입으로 바꾸기) [[branch-by-enum-value]]
      * if nullable
      [[branch-by-nullable-value]]

    - loop
      * for (그 자체로 설명) [[loop-using-for]]
      * foreach (seq<>.Next constraint 함수 호출) [[loop-using-foreach]]
      * break [[exit-loop]]
      * continue [[restart-loop]]
      
    * return [[exit-function]]
    * yield [[yield-seq-function-context]] (현재 seq값에 실행 컨텍스트를 저장하고, 저장해놨던 컨텍스트로 복귀하며 리턴값 설정하기)

  - tasks
    * task [[run-parallel]] (동시 실행하기)
    * await [[wait-tasks]] (지역 작업 새 스코프를 생성하고, 모아서 기다리기)
    * async [[run-asynchrounouly]] (비동기적으로 실행하기)

  - sequence
    * seq 값 만들기 [[make-anoymous-seq-value]] (seq 함수와 yield를 사용하여)
    * seq<> 값 만들기 [[make-seq-interface-value]] (seq 함수에 box를 붙여서)
      - seq<> 인터페이스로 여러 종류의 seq 값을 한 변수에 대입하기    

  - lambda 
    * lambda 값 만들기 (lambda 식) [[make-lambda-value]]
    * lambda 식 앞에 box를 붙여서 func<> 만들기 [[make-func-interface-value]]
      - func<> 인터페이스로 여러 종류의 lambda 값을 한 변수에 대입하기
    * lambda 식 앞에 box를 붙여서 func_throw<> 만들기 [[make-func_throw-interface-value]]
    * lambda 값 호출 [[call-lambda-value]](func<>.Invoke constraint 함수 호출)
    * func<>를 func_throw<> 로 캐스팅 [[cast-func-interface-value]]

  - 리터럴
    * bool: true, false [[make-bool]]
    * int [[make-int]]    
    * string [[make-string]] ("text $x ${x}" 생성자)

  - 자료 구조
    - [] list 생성자 [[make-list]]
    - [] list 참조 [[reference-list-element]]
    - { } dict<,> 생성자 [[make-dict]]
    - [] dict 참조 [[reference-dict-element]]

  - 사용자 정의 가능 타입

    - 타입 가리키기 [[alias-other-type]]

    
    - class (9)
      - class 선언 [[declare-class]]
      - class 값 생성 (생성자 호출) [[make-class-value]]
      - class 값 멤버 변수 참조 [[reference-instance-class-member-variable]]
      - class 값 인스턴스 함수 호출 (식타입사용) [[call-instance-class-member-function]]
      - class 값 인스턴스 함수 가상 호출 (값타입사용) [[call-virtual-class-member-function]]
      - class 정적 변수 참조 [[reference-static-class-member-variable]]
      - class 정적 함수 호출 [[call-static-class-member-function]]
      - class 타입 캐스트 [[cast-class-value]]

    - struct (6)
      - struct 선언 [[declare-struct]]
      - struct 값 생성 (생성자 호출) [[make-struct-value]]
      - struct 값 멤버 변수 참조 [[reference-instance-struct-member-variable]]
      - struct 값 인스턴스 함수 호출 [[call-instance-struct-member-function]]
      - struct 정적 변수 참조 [[reference-static-struct-member-variable]]
      - struct 정적 함수 호출 [[call-static-struct-member-function]]

    - interface (7)
      - interface 선언 [[declare-interface]]
      - interface 값 생성 (다른 값으로부터) [[make-interface-value]]
      - interface 값 인스턴스 함수 가상 호출 [[call-virtual-interface-member-function]]  
      - interface 타입 캐스트 [[cast-interface-value]]
      
      - constraint로부터 생성 [[make-value-from-constraint]]
      - constraint로부터 함수 호출 (가상 호출이 아닌) [[call-instance-function-from-constraint]]
      - constraint로부터 정적 함수 호출 [[call-static-function-from-constraint]]

    - enum (4)
      - enum 선언 [[declare-enum]]
      - enum 값 생성 (enum 생성자로부터) [[make-enum-value]]
      - enum 값 멤버의 변수 참조 [[reference-enum-element-member-variable]]
      - enum 값 캐스트 [[cast-enum-value]]

    - tuple (3)
      - tuple 인라인 선언 [[declare-tuple]]
      - tuple 값 생성 ((...) 식) [[make-tuple-value]]
      - tuple 값 멤버 변수 참조 [[reference-tuple-member-variable]]

  - box
    - box 값 생성 (다른 값으로부터) [[make-box-value]]
    - box가 가리키는 값 참조 [[reference-box-member-varable]]

  - ref  
    - ref 값 생성 (다른 지역 변수로부터, 호출에서만 가능) [[make-ref-value]]
    - ref가 가리키는 값 참조 [[reference-ref-member-variable]]

  - nullable
    - nullable 값 생성 (다른 값으로부터, null로 부터) [[make-nullable-value]]
    - nullable이 가리키는 값 참조 [[reference-nullable-member-variable]]

  - void
    - void 값 생성 () [[make-void-value]]

  - null
    - null 값 생성 [[make-null-value]]
  
  - this
    - this 참조 [[reference-this]]

  - 네임스페이스
    - 네임스페이스 열기 namespace X { }, 간편 네임스페이스 열기 #namespace X; [[open-namespace]]
    - 검색할 네임스페이스 설정하기 (using) [[add-using-namespace]]



  


