# 개념 Concept

개념을 초급/중급/고급으로 나눠보자, 근데 그 전에 전체 레퍼런스는 있어야 할 것 같다

요약

1. 모듈
    1. 소스 파일 여러 개를 묶어서 한 프로그램으로 컴파일 할 수 있다
    2. 프로그램은 {네임스페이스, 타입, 함수} 정의, 탑레벨 절을 포함할 수 있다
2. 식별자(Identifier) 
    - 네임스페이스, 타입, 함수, 변수를 식별하기 위한 용도의 것
    - 네임스페이스 Namespace
        - 전역
        - 네임스페이스는 타입, 함수, 변수
        - using으로 네임스페이스 열기
    - 식별자의 접근성 Accessibility
    - 타입 식별자 (식별자 중 타입만 관련 있는 것들 TypeExp)
    - 참조 범위 (Scope)
    - 식별자 검색 순서
3. 커맨드
4. Storage
    1. 전역 변수
        1. 정의 (PrivateGlobalVarDecl)
        2. 사용 (PrivateGlobalVarExp)
    2. 지역 변수
        1. 선언 (LocalVarDecl)
        2. 사용 (LocalVarExp)
    3. 스태틱 변수
        1. StaticMemberExp
    4. 멤버 변수
5. 분기
    1. 일반 분기
    2. 캐스트 시도 분기 if test
        1. Class
        2. Enum
6. 반복
    1. For
    2. Continue
    3. Break
    4. Foreach
7. 함수
    1. 함수 선언 (Func)
    2. 호출(CallFuncExp)
    3. 함수 빠져 나가기(ReturnStmt)
8. 순회 Iteration
    1. List 순회
    2. Sequence 함수 (SeqFunc), Yield
9. 범위 만들기 BlockStmt
10. 동시, 비동기, 대기 (Task, Async, Await)
11. 외부 모듈과 통신 (ExternalGlobalVarExp, CallExternalFuncExp)
12. 타입 변수
    1. Type Alias
    2. Generic Type Argument
13. 변수에 값 대입 (AssignExp)
14. 값
    1. 문자열
        1. 만들기
    2. 32비트 정수
        1. 만들기
        2. operations
    3. Bool
        1. 만들기 BoolLiteralExp
        2. operations
    4. 람다 
        1. 생성 (LambdaExp)
        2. 람다 호출 (CallLambdaExp)
    5. 리스트
        1. 리스트 값 생성 (ListExp)
        2. 리스트 요소 참조 (ListIndexExp)
    6. Struct
        1. Struct 선언 
        2. 값 생성
        3. 멤버 변수 참조 (StructMemberExp)
        4. 멤버 함수 참조
        5. Struct 오브젝트 생성
    7. Class
        1. Class 선언
        2. 멤버 변수 참조 (ClassMemberExp)
        3. 멤버 함수 참조
        4. Class 오브젝트 생성
    8. Enum
        1. Enum 선언
        2. 값 생성
        3. Enum 오브젝트 생성
    9. Interface
        1. 오브젝트의 인터페이스
    10. 레퍼런스 값
        1. Heap Object 레퍼런스 *
        2. Scoped Value 레퍼런스 &

이름 공간

[Module](Modules.md)

[식별자 Identifier](Identifiers.md)

 

[타입 Types](Types.md)

[함수 Functions](Functions.md)

[변수 Variables](Variables.md)

[Statements](Statements.md)

[Expressions](Expressions.md)

[Values](Values.md)