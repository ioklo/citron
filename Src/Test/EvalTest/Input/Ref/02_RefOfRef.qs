// 4
int s = 3;
ref<int> i = ref s; // 이후에 i는 int 처럼 동작한다
ref<int> j = ref i; // s의 ref
j = 4;
@$s