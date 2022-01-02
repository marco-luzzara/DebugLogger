[![.NET](https://github.com/marco-luzzara/DebugLogger/actions/workflows/dotnet-test.yml/badge.svg)](https://github.com/marco-luzzara/DebugLogger/actions/workflows/dotnet-test.yml)

# DebugLogger
This library exports a `DebugLogger`, an implementation of `ILogger` for Debugging purposes, that are:
- Simple `string` print
- Printing variables in a pretty way
- Printing the variables passed to a function as arguments whenever the function is called

---

## Initialization

You can instantiate a `DebugLogger` like this:

```
var logger = new DebugLogger(
    new DebugLogger.DebugLoggerConfig() 
        { 
            OutWriter = writer, 
            MethodsWithContextLogging = new HashSet<MethodBase>() 
                {
                    typeof(YourClass).GetMethod("YourMethod"),
                    typeof(Convert).GetMethod("ToInt32", BindingFlags.Static | BindingFlags.Public, new Type[] 
                        {
                            typeof(bool)
                        }
                    ),
                    typeof(Uri).GetMethod("ToString")
                }
        }
    );
```

Where:
- `OutWriter` is of type `TextWriter` and its default value is `Console.Out`.
- `MethodsWithContextLogging` is an `ISet<MethodBase>` containing all the methods that, when called, should print the arguments used to call them. You can also change methods defined in external libraries or even in the `System` namespace.

Among the `DebugLoggerConfig` you can also specify the `IsEnabled` property, corresponding to the `ILogger.IsEnabled()` and a `VariableFormatter`: it accepts a variable name and value and returns a string that defaults to `{name} = {value}`. It is used in the `LogVariables` method.

---

## Logging
Each method contained in `MethodsWithContextLogging` is decorated with a prefix to extend the method itself and log the initial values of the arguments passed to it. A prefix is a concept coming from [Harmony](https://harmony.pardeike.net/articles/intro.html), a library that "patches" methods at runtime.

Basically, every time these methods called, their arguments are logged on a `TextWriter`, that is:
- The value of `OutWriter` if the class declaring the method has a field (the accessibility is not important) of type `DebugLogger`.
- `Console.Out` otherwise, including the case when the method called is a static one.

---

## Limitations
- The maximum number of arguments that a method in `MethodsWithContextLogging` can have is 7, but can be easily extended.
- The prefix in Harmony is a class containing only static methods, therefore reading the logger configurations involves some constraints. In the current implementation, configs are available only if there is a field of type `DebugLogger` in the class declaring the method intercepted.
- You might encounter an exception when trying to patch certain methods belonging to the `System` namespace because (here I guess) they are internally used by Harmony.

---

## Conclusions
A `DebugLogger` can be very useful when you have a stack trace and you need more information for a certain frame, or just when you would like to follow how an algorithm works without having to scatter log calls in each method.