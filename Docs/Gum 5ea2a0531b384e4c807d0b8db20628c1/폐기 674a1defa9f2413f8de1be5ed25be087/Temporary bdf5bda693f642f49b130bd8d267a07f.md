# Temporary

F#이지만 Ocaml Syntax Highlighting으로.. 보니까 Func랑 Type declaration이 빠진

```ocaml
module Syntax

type LiteralTypeID = {
    name: string
}

type TypeInstTypeID = {
    typeID: LiteralTypeID
    typeArgs: TypeID list
}

and TupleTypeID = {
    types: TypeID list 
    name: string option
}

and UnionTypeID = {
    types: TypeID list 
}

and NullableTypeID = {
    typeID: TypeID // default, tuple, typeInst
}

and TypeID = 
    | Literal of LiteralTypeID // A
    | TypeInst of TypeInstTypeID // A<B?>
    | Nullable of NullableTypeID // A?
    | Tuple of TupleTypeID // (A, B?, C)
    | Union of UnionTypeID // (A | B)

type TypeParam = { bParams: bool; name: string }
type FuncParam = { 
    bParams: bool
    bOut: bool
    bRef: bool
    typeID: TypeID
    name: string 
}

type Accessibility = 
    | Private
    | Protected
    | PublicGet
    | Public

type MemberFuncKind = 
    | Normal
    | Virtual
    | Override
    | Static

type SyncOption = Sync | Async

type FileUnit = {
    decls: FileUnitDecl list
}

and FileUnitDecl = 
    | Class of ClassDecl
    | Struct of StructDecl
    | Interface of InterfaceDecl
    | Func of GlobalFuncDecl
    | Stmt of Stmt

and TypeParamConstraint = {
    typeVar: string
    constraintTypes: TypeID list
}

and ClassDecl = { 
    name: string
    typeParams: TypeParam list
    typeParamConstraints: TypeParamConstraint list
    baseTypes: TypeID list
    memberDecls: MemberDecl list 
}

and StructDecl = { 
    name: string
    typeParams: TypeParam list
    typeParamConstraints: TypeParamConstraint list
    baseTypes: TypeID list
    memberDecls: MemberDecl list 
}

and InterfaceMemberDecl = {
    bStatic: bool
    bAsync: bool
    retType: TypeID
    name: string
    funcParams: FuncParam list    
}

and InterfaceDecl = { 
    name: string
    typeParams: TypeParam list
    typeParamConstraints: TypeParamConstraint list
    baseTypes: TypeID list
    memberDecls: MemberDecl list
}

and MemberDecl = 
    | Constructor of MemberConstructorDecl
    | Var of MemberVarDecl
    | Func of MemberFuncDecl

and MemberVarDecl = {
    accessibility: Accessibility
    varType: TypeID
    names: string list
}

and MemberFuncDecl = {
    accessibility: Accessibility
    funcKind: MemberFuncKind
    syncOption: SyncOption

    retTypeID: TypeID
    name: string
    typeParams: TypeParam list
    funcParams: FuncParam list
    
    stmts: Stmt list
}

and MemberConstructorDecl = {
    accessibility: Accessibility // Only Public, Protected allowed
    syncOption: SyncOption

    retTypeID: TypeID
    name: string
    typeParams: TypeParam list
    funcParams: FuncParam list

    baseConstructorArguments: CallArgument list

    stmts: Stmt list
}

and VarDecl = {
    varType: TypeID option // implicit 인 경우 type이 존재하지 않는다
    varDeclItems: VarDeclItem list
}

and GlobalFuncDecl = {    

    syncOption: SyncOption

    retTypeID: TypeID
    name: string
    typeParams: TypeParam list
    typeParamConstraints: TypeParamConstraint list
    funcParams: FuncParam list

    stmts: Stmt list
}

and GlobalVarDecl = VarDecl    

and CallArgument = { bOut: bool; exp: Exp }

and UnaryOperatorKind = 
    | PlusPlus             // ++, unary, postfix
    | MinusMinus           // --, unary, postfix
    | Excl                 // !, unary

and BinaryOperatorKind = 
    | Plus
    | Minus
    | Star
    | Slash
    | EqualEqual           // ==
    | Less                 
    | LessEqual
    | Greater              // GT, GTEqual은 Less, LessEqual로 변환된다
    | GreaterEqual         // short circuit으로 따로 처리

and ShortcutOperatorKind = 
    | LogicalOr 
    | LogicalAdd
    
and Exp = 
    | UnaryOperator of UnaryOperatorExp
    | BinaryOperator of BinaryOperatorExp    
    | ShortcutOperator of ShortcutOperatorExp
    | Indexer of IndexerExp
    | NullLiteral 
    | IntLiteral of IntLiteralExp 
    | BoolLiteral of BoolLiteralExp 
    | StringLiteral of StringLiteralExp     
    | ListConstructor of ListConstructorExp   
    | TupleConstructor of TupleConstructorExp 
    | ID of IDExp 
    | New of NewExp 
    | Call of CallExp 
    | Assign of AssignExp  
    | Member of MemberExp 
    | Conditional of ConditionalExp 
    | NullConditional of NullConditionalExp 
    | Await of AwaitExp 
    | Launch of LaunchExp 

and Stmt = 
    | Blank
    | Block of BlockStmt
    | VarDecl of VarDeclStmt
    | Return of ReturnStmt
    | Continue
    | Break
    | If of IfStmt
    | TypeGuard of TypeGuardStmt
    | While of WhileStmt
    | DoWhile of DoWhileStmt
    | For of ForStmt
    | Foreach of ForeachStmt
    | Exp of ExpStmt
    | YieldReturn of YieldReturnStmt
    | YieldBreak 
    
// TODO: CallArgument list를 포함하는 모든 것들에 params 적용
and UnaryOperatorExp = { kind: UnaryOperatorKind; operand: Exp }
and BinaryOperatorExp = { kind: BinaryOperatorKind; leftOperand: Exp; rightOperator: Exp }
and ShortcutOperatorExp = { kind: ShortcutOperatorKind; leftOperand: Exp; rightOperator: Exp }
and IndexerExp = { operands: Exp list }
and IntLiteralExp = { value: int }
and BoolLiteralExp = { value: bool }
and StringLiteralExp = { value : string }
and InterfaceConstructorExp = { interfaceType: TypeID; exp: Exp }
and ListConstructorExp = { elems: Exp list; elemType: TypeID }
and TupleItem = { name: string option; exp: Exp }
and TupleConstructorExp = { items: TupleItem list }
and IDExp = {name: string; typeArgs: TypeID list }
and NewExp = { objectType: TypeID; args: CallArgument list; }

and CallExp = { funcExp: Exp; args: CallArgument list; paramsExp: Exp option }
and AssignExp = { leftExp: Exp; rightExp: Exp }    
and MemberExp = { exp: Exp; id: IDExp } 
and ConditionalExp = { cond: Exp; trueExp: Exp; falseExp: Exp } 
and NullConditionalExp = { exp: Exp; nullExp: Exp }
and AwaitExp = { callExp: CallExp }
and RefExp = { exp: Exp } 
and DerefExp = { exp: Exp } 
and LaunchExp = { exp: Exp option; parallelStmts: Stmt }

and VarDeclItem = { name: string; exp: Exp option }

and BlockStmt = { stmts: Stmt list }
and VarDeclStmt = VarDecl
and ReturnStmt = { exp: Exp option; }
and IfStmt = { cond: Exp; trueBody: Stmt; falseBody: Stmt }    
and TypeGuardStmt = { idExp: IDExp; typeGuardType: TypeID; trueBody: Stmt; falseBody: Stmt }
and WhileStmt = {cond : Exp; body: Stmt; }
and DoWhileStmt = { body: Stmt; cond: Exp }
and ForStmt = { initializer: Stmt; cond: Exp; condExp: Exp; body: Stmt }
and ForeachStmt = { typeParams: TypeParam list; elemType: TypeID option; elemName: string; enumExp: Exp; body: Stmt; } 
and ExpStmt = { exp: Exp; }
and YieldReturnStmt = { exp:Exp; }

type REPLUnitItem = 
    | GlobalFunc of GlobalFuncDecl
    | Class of ClassDecl
    | Struct of StructDecl
    | Interface of InterfaceDecl
    | Stmt of Stmt

type REPLUnit = {
    items: REPLUnitItem list
}
```