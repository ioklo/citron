struct S
{
    int x;

    ref int F()
    {
        return ref x;
    }
}

{
    S s = S(3);
    var x = box s.F(); // 에러, s를 캡쳐해서 x에 넣을수 없으므로 불가
}