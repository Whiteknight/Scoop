# Scoop

Scoop is a "research project" to explore some ideas in programming languages and OO language design. For ease of implementation and simplicity of design, Scoop is being built as a Transpiler to output C# code. The generated C# can then be built using the normal .NET toolchain into assemblies and executables. 

Scoop Level 1 (L1) is a pure subset of C# (version 7-era). New features are implemented as extensions to the grammar, AST and transpiler. In this way we can explore new features individually and in combination to see how well they work.

## Thesis

While I believe C# is among the best of modern Object-Oriented, general-purpose programming languages, it is not without flaws. In fact, many of the popular, modern OO languages share similar problems. In particular:

1. These languages tend to consist of C-style procedural code organized into objects and methods, which leads to all the problems and mess of procedural languages (long methods, deep nesting, high cyclomatic complexity, etc).
1. These languages tend to make the wrong things easier than the correct things, by requiring extra syntax to do things the right way.

We've learned a heck of a lot over the last 20 years of programming and language development, and we don't need to keep doing things the way we started doing them back in the 1970s. Scoop is designed to address these two problems with the following goals:

1. Start with familiar C# syntax
1. Transpile to C# to leverage C# interoperability, the .NET toolchain and library ecosystem
1. Remove features from C# which contribute to the problems listed above
1. Change some default behaviors to make the right things easier and the wrong things harder
1. Add some new features designed for modern OO and not for legacy Procedural code, to replace features removed from C#.

## Level 1 (L1)

Scoop L1 is intended as a strict subset of C# removing features which are considered problematic, counter-productive, or contrary to the goals of Scoop. Some features are also ignored which, though not problematic per se, aren't germaine to the goals of this project and which are too much effort to implement for the benefit they bring.

L1 *is not intended to be a usable, general-purpose language* many of the features which are not copied from C# are necessary for non-trivial programs. It is intended that many of these omissions will be addressed in later levels. 

The goals of L1 are:
1. Create a working, albeit simple, transpiler
1. Implement the subset of C# which is most relevant and necessary for future work

### L1 FAQ

**Why not just use Roslyn?** 

There are a few reasons:
1. Roslyn is significantly more powerful and complicated than what we need. The modifications we needed to make to Roslyn to remove the features we don't want would have been significant.
1. By transpiling to C#, we can get a lot of the necessary features of a compiler "for free" (type inference, symbol management, optimization, IL code generation, etc)
1. We wanted the ability to dynamically modify the grammar to add or remove individual features, which is basically impossible using the Recursive Descent algorithm in Roslyn.

In short it didn't make sense for this project to try and integrate directly with Roslyn. Instead the Scoop transpiler operates as a "preprocessing" layer on top of Roslyn. Some features of C# will just not be available in this preprocessor for now.

## Language Extensions

Extensions to the language can be activated by rewriting parts of the parser tree. Once we've made these modifications, the new features will parse and transpile as expected.



