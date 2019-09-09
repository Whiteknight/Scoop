# TODO

## Required L1 Items:

These items are required in L1 before we call it "complete" and cutover to L2.

1. Comments and cleanups for parser rules
1. Improve test coverage of transpiler

## Items we might want in L1

These items we do want to have, though it's not clear whether to put them in L1
or save them for after L2 cutover

1. Named arguments
1. "try"/"catch"/"when"/"finally"/re"throw;" (If we want these as-is, put them in L1. Otherwise, put a replacement in L2)
1. partial methods?
1. "params" keyword
1. Validation visitor?
1. In-place tree updating for visitors?
1. Scoop.Cli should search all files in folder recursively from current directory
1. "lock" statements (only if done using a single statement instead of a block)
1. Tuples and destructuring assignment (this may already work?)
1. "in"/"out" generic type param modifiers
1. "yield" enumerable methods?

## L2 Items

These items are planned for L2

1. Syntax for easy explicit typing of variable declarations
1. Syntax for easy casting
1. factory/filter/strategy methods
1. Named constructors