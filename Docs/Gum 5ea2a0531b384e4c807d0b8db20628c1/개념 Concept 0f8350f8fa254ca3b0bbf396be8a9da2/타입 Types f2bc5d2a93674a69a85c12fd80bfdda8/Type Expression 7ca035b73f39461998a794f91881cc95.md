# Type Expression

```csharp
TypeId = Namespace ( '.' ... )* '.' Type
       | TypeId '.' ID

RefTypeExp =                     // Referenceable Type Expression
    | TypeId '<' RefTypeExp ',' ... '>'  // 
    | '(' RefTypeExp ',' ... ')' 
    | RefTypeExp '?'    // Nullable

TypeExp = 
    | RefTypeExp '&'    // Scoped
    | RefTypeExp '*'    // Heap
    | TypeExp => TypeExp
    | 'void'
```