// $Error

{
    int x = 3;
    var l = box () => x; // 에러, local 캡쳐를 boxing하려고 했음. [x] () => x; // 값으로 저장하세요
}