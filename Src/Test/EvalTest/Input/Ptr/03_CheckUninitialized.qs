// $Error

int i;       // uninitialized
int* p = &i; // 에러, uninitialized는 포인터로 가리킬 수 없습니다
@{$p} 