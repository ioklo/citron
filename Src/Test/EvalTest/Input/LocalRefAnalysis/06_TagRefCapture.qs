// $Error

void Main()
{
    int i = 3;
    int* x = &i;

    var l = () => *x; // l은 local-ptr-contained
    var p = l;        // p는 local-ptr-contained, 전파

    var s = box p;   // 에러, p는 local을 갖고 있으므로 boxing 불가
}