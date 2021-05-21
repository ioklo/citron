// 1. 타입모으기 -> 2. Func(타입도 포함) 시그니처 모으기 -> 3. 최상위 stmt 분석 (글로별 변수 타입정보추출) -> 4. 각 함수 분석

// TODO: 추후에 가능해야 할 것들, 다른 stmt를 보면서 x1의 타입을 알아내간다
// var x1;
// x1 = b ? X.First : X.Second(2);   

var x1 = X.First;  // 타입 검사를 먼저 하는지 테스트, enum이 밑에 있지만 여기서 사용 가능하다

void Func(X x2) // 타입 검사를 먼저 하는지 테스트, 함수 선언도 X가 밑에있어도 여기서 할 수 있다
{
    x1 = First; // x1이 X타입인게 먼저 밝혀져야 한다
}

// class C // 타입은 먼저 모으고,
// {
//     X Func() { return First; } // Func 시그니처는 타입을 모으고 나서, 나중에 모은다
// }

enum X // 타입 모으기 먼저
{
    First,
    Second (int i)
}

F(Second(2));  // 함수 파라미터에 X가 있으면 네임스페이스가 열려서 Second를 그냥 쓸 수 있도록 한다