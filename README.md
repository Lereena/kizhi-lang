# Kizhi programming language


## Basic syntax

```
- set {variable} {value} // Assign {value} to {variable}

- sub {variable} {value} // Subtract {value} from {variable}

- print {variable} // Print the value of a {variable}

- rem {variable} // Delete {variable} from memory

- def {function_name} // Define function with name {function_name}

- call {function_name} // Call function with name {function_name}
```

All variables are mutable and global. Variable values are integer numbers.

The code related to the function is separated by four spaces. Function can be defined after its call.

The program should be defined as follows:

```
set code
  // program text
end set code
```

The `run` command launches code interpretation and execution.

## Debugger syntax

```
- add break {line} // Add breakpoint to {line} (integer number from 0 to (program lines count - 1))

- step over // Go to the next line without entering the function

- step // Go to the next line 

- print mem // Print all variables and their values

- print trace // Print stacktrace (from the last called function to the first)

- run // Execute program to the next breakpoint
```

Program and debug example can be found in the `main` function.
