// $Error

int i;
ref S p = ref i; // uninitialized도 트래킹 해야 하나
@{$p} // 에러, uninitialized