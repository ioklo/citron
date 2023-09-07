// $Error

void Main()
{
    int* s = box 3; // 에러, box가 고정되지 못함
    @${*s}
}