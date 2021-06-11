// $Error

{
    int x = 3;
    var l = () => x; // l은 local-ref-contained(x)
    var p = l;       // p는 local-ref-contained(x), 전파

    var s = box p;   // 에러, p는 local-ref-contained(x)
}