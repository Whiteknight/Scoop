# Scoop Level 1

Scoop L1 is intended to be a strict subset of C# syntax, removing features which are considered problematic or which encourage bad design. 

## C# Passthrough

Because of some of the limitations of L1, a mechanism has been added to allow passthrough of verbatim C# to go through the transpiler un-touched. In this way we can use
all of the missing features:

```csharp
c# { 
    // literal C# goes goes here
}
```

Every use of this mechanism should be considered a code-smell and ways should be found to avoid it where possible. This structure may be removed in future Layers.

## Removed Features

The following features are removed from C#:

### Procedural Control Structures

Classical, procedural control structures are removed in Scoop. These structures tend to encourage long methods and deep nesting, which are problems. 

* `if`/`else` blocks and `switch`/`case`/`default` blocks are removed (`default(T)` is still allowed)
* Loops `while`, `do`/`while`, `for` and `foreach` and keywords `break` and `continue` are removed
* `try`/`catch`/`finally` is removed (though these or something like them may eventually be re-added to support this necessary functionality)
* `goto` is removed
* Blocks "{" "}" are disallowed along with all variable scoping rules. Variables in a method are scoped to the method no matter where they are declared. Structures which are included, such as "using" only support a single statement instead of a block (it's suggested to use that opportunity to call a method if you want to execute multiple statements or have a separate scope for variables)

### Concrete Inheritance

Concrete inheritance leads to difficult-to-read situations and is an inferior mechanism for code reuse. Concrete inheritance of classes written in Scoop is disallowed, though inheritance of interfaces is still supported and encouraged. The following changes are made:

1. Classes cannot inherit from other classes. Due to naivete of the parser, this restriction is implemented in generated code and will be caught some time after ScoopL1->C# transpilation
1. `protected`, `base`, `abstract`, `override` and `virtual` keywords are disallowed. `new` keyword as a method/property modifier is disallowed
1. Generated classes are marked `sealed` in C#. There is no explicit `sealed` keyword in Scoop and no way to have a generated class not be marked `sealed`.

### Unsafe modes and pointers

C# Supports un-safe and un-checked modes to allow high-performance code to avoid some of the basic safety checks and also to enable direct access to pointers and memory. Scoop intends to be a high-level pure-OO language and so these things are not implemented. If these features are needed, use of C# directly is preferred.

1. `unsafe`, `checked` and `unchecked` keywords are disallowed
1. Optimization hints such as `stackalloc`, `fixed` and `volatile`

### Access Modifiers 

1. The `extern` keyword is not necessary for this project and so is removed
1. The `internal` keyword is removed.

### Static anything

The `static` keyword is removed, including anything which relies on static classes/methods (extension methods, etc)

### Events

The `event` keyword is removed, including `add` and `remove` contextual keywords. Object-based alternatives such as `IObservable` or message bus solutions are preferred to share state updates with outside classes

### Operator Overloading

For now, operator overloading is removed. This includes keywords `implicit`, `explicit` and `operator`

### Pass-By-Reference

Keywords `out` and `ref` on method parameters are disallowed. Returning objects or value tuples is preferred. `out` to indicate contravariance in generic interface parameters may still be supported

### Properties

Properties frequently violate encapsulation by allowing external code to access the internal state of an object. For this reason, properties are disallowed. Accessor methods are preferred. Keywords `get` and `set` are disallowed and `value` is never treated as a keyword in any context.

### Difficult Features

Due to the syntactical nature of C#, some features which may be benign or even desireable to have in Scoop are not implemented because of the complexity of parsing these features. This is often due to ambiguity in the grammer which cannot be resolved in a finite number of steps, or because of the difficulty in implementing these features in a simple recursive-descent parser. Also, some features are not implemented because they are redundant and the effort to implement parser rules is not worth the benefit of having the feature in Scoop L1. 

1. Declaring variables with explicit types is disallowed. The `var` keyword must be used for all variable declarations (parameter declarations may use type names like normal). This is because of the difficulty of parsing type names without contextual clues.
    * Likewise the `(type)` cast operator is disallowed. It is too difficult to differentiate this operator from an expression without contextual clues and arbitrary lookahead. The `as` and `is` keywords are implemented for reference types, though there is currently no way to cast to a value type in Scoop L1 (L2 may add new, unambiguous, syntax to address this shortcoming)
1. LINQ keywords and integrated queries are removed from the language, including keywords `select`, `from`, `let`. The integrated query language requires a lot of effort to parse correctly, when all the same functionality can be reproduced using LINQ method chains.
1. Local methods are not implemented. 

### Features To Be Determined

Several features of C# are desirable to have in Scoop but may not be strictly necessary to have for the successful completion of L1. These features may be ported in L1, may be ported in a later layer, or may be replaced in a later layer with new syntax to achieve a similar result in a superior way.

1. `try`/`catch`/`when`/`finally`/`throw` keywords are not currently implemented.
1. Iterator methods with `yield return` and `yield break` keywords are not currently implemented

### C# 8 features

It is not yet known how L1 will interact with C#8. Some features of the new language version are desireable to have, and other features are not. The L1-L2 cutover may happen before the general release of C#8 in which case new features, if desired, will be implemented in a later layer.

## Transpiler Features

Scoop L1 will not include several basic parts of a compiler, for brevity and ease of implementation. L1 will not include a symbol table, for example, instead leaving the work of managing variable scope and variable lifetimes to Roslyn. Validation, if any, will focus on problems related specifically to Scoop syntax and semantics. Any error or warning which can be caught and handled by Roslyn will be passed through. L1 also will not include any kind of optimizer and absolutely no IL code generation.