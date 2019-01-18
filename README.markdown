# Flee (Supports .Net Core 2.0)
 Fast Lightweight Expression Evaluator.
 Convert this project vb.net to c#.
  
 ## Project Description
Flee is an expression parser and evaluator for the .NET framework. It allows you to compute the value of string expressions such as sqrt(a^2 + b^2) at runtime. It uses a custom compiler, strongly-typed expression language, and lightweight codegen to compile expressions directly to IL. This means that expression evaluation is extremely fast and efficient.

## Features
* Fast and efficient expression evaluation
* Small, lightweight library
* Compiles expressions to IL using a custom compiler, lightweight codegen, and the DynamicMethod class
* Expressions (and the IL generated for them) are garbage-collected when no longer used
* Does not create any dynamic assemblies that stay in memory
* Backed by a comprehensive suite of unit tests
* Culture-sensitive decimal point
* Fine-grained control of what types an expression can use
* Supports all arithmetic operations including the power (^) operator
* Supports string, char, boolean, and floating-point literals
* Supports 32/64 bit, signed/unsigned, and hex integer literals
* Features a true conditional operator
* Supports short-circuited logical operations
* Supports arithmetic, comparison, implicit, and explicit overloaded operators
* Variables of any type can be dynamically defined and used in expressions
* CalculationEngine: Reference other expressions in an expression and recalculate in natural order
* Expressions can index arrays and collections, access fields and properties, and call functions on various types
* Generated IL can be saved to an assembly and viewed with a disassembler

### Installing Flee

You should install [Flee with NuGet](https://www.nuget.org/packages/Flee):

    Install-Package Flee
    
Or via the .NET Core command line interface:

    dotnet add package Flee
    
## NuGet Packages

| Name  | NuGet |
| :---  | :---  |
| [Flee](https://www.nuget.org/packages/Flee)                       | [![Flee](https://img.shields.io/badge/nuget-v1.2.2-blue.svg)](https://www.nuget.org/packages/Flee)                 

## More information
* [Examples](https://github.com/mparlak/Flee/wiki/Examples) to learn how to create and evaluate expressions.

## License
Flee is licensed under the LGPL. This means that as long as you dynamically link (ie: add a reference) to the officially released assemblies, you can use it in commercial and non-commercial applications.
