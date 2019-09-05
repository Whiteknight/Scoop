# Scoop

Scoop is a "research project" to try to make a new programming language which avoids some of the common problems with modern OO languages. For ease of implementation and simplicity of design, Scoop is being built as a Transpiler to output C# code. The generated C# can then be built using the normal .NET toolchain into assemblies and executables. Scoop is being developed in a series of phases or **Levels**. Each level has a particular goal. When that goal is acheived the code will be copied to the next level and the next set of features will begin development.

## Thesis

While I believe C# is among the best of modern Object-Oriented, general-purpose programming languages, it is not without flaws. In fact, many of the popular, modern OO languages share similar flaws. In particular:

1. These languages tend to consist of C-style procedural code organized into objects and methods, which leads to all the problems and mess of procedural languages (long methods, deep nesting, high cyclomatic complexity, etc)
1. These languages tend to make the wrong things easier than the correct things, by requiring extra syntax to do things the right way.

Scoop is designed to address these two problems with the following goals:

1. Start with familiar C# syntax
1. Transpile to C# to leverage C# interoperability, the .NET toolchain and library ecosystem
1. Remove features from C# which contribute to the problems listed above
1. Change some default behaviors to make the right things easier and the wrong things harder
1. Add some new features designed for modern OO and not for legacy Procedural code, to replace features removed from C#.

## Level 1 (L1)

Scoop L1 is intended as a strict subset of C# removing features which are considered problematic, counter-productive, or contrary to the goals of Scoop. At the completion of L1 all features from C# which are considered necessary will be implemented and features from C# which are not desired will be ignored.

L1 *is not intended to be a usable, general-purpose language* many of the features which are not copied from C# are necessary for non-trivial programs. It is intended that many of these omissions will be addressed in later levels. In particular, several classical control structures inherited from C and C++, are removed such as `if`/`else`, `for`, `while`, and `foreach`. 

The goals of L1 are:
1. Create a working, albeit simple, transpiler
1. Implement the subset of C# which is most relevant and necessary for future work

## Level 2 (L2)

Scoop L2 builds on L1 by starting to add some new features to replace the features removed from C# in L1. These new features are being designed to try and promote best-practices, patterns, and good design by default.

