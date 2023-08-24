# Program

## General
A program is data as input of the machine. It consists of several commands that manipulate the machine.

```
Program = Module Declaration * Function Bodies * Entry Id * [Loading External Module Info]
``` 

`Function Bodies` are where actual command reside, and `Entry Id` indicates where the program starts.  To communicate with external environments, there is some information for loading external modules. Lastly, `Module Declaration`is for reflection of the program itself.
## Basic form

`Hello World!`를 출력하는 가장 기본적인 형태입니다.
%%Test(Program_Basic, Hello World!)%%
```
void Main()
{
    @Hello World!
}
```

이 프로그램은 Main전역함수에 대한 정의와 본문을 가지고 있습니다. `EntryId`는 별도로 정하지 않았기 때문에 `Main`이 됩니다. 외부 모듈 로딩 정보로 런타임 라이브러리 모듈을 로딩정보를 갖고 있습니다.

본문의 `@`는 command statement입니다. 따로 command provider를 정하지 않았기 때문에 기본적인 `echo` 역할을 합니다. 자세한 사항은 [[Command]] 에서 다룹니다. 