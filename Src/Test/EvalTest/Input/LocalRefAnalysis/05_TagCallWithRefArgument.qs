// $Error

struct S
{
    static ref int F(ref int x)
    {
        return x; // x자체는 ref 이므로, 바로 리턴 가능
    }
}

{
    var i = 3;
    var x = box S.F(ref i); // 에러, Box불가(이건 그냥 ref<> 는 box불가;)
}
