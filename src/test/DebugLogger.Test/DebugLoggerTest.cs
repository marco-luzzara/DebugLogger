namespace Logging;

using System.IO;
using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Reflection;

[TestClass]
public class DebugLoggerTest
{
    private DebugLogger logger;
    private TextWriter writer;

    [TestInitialize]
    public void Initialize()
    {
        writer = new StringWriter();
        logger = new DebugLogger(
            new DebugLogger.DebugLoggerConfig() 
                { 
                    OutWriter = writer, 
                    MethodsWithContextLogging = new HashSet<MethodBase>() 
                        {
                            typeof(DebugLoggerTest).GetMethod("Dummy"),
                            typeof(DebugLoggerTest).GetMethod("DummyWithParameters"),
                            typeof(DebugLoggerTest).GetMethod("DummyThatCallsAnother"),
                            typeof(Convert).GetMethod("ToInt32", BindingFlags.Static | BindingFlags.Public, new Type[] 
                                {
                                    typeof(bool)
                                }
                            ),
                            typeof(Uri).GetMethod("ToString")
                        }
                }
        );
    }

    [TestMethod]
    public void LogVariables_NoVariable_PrintNothing()
    {
        logger.LogVariables();
        Assert.AreEqual("", writer.ToString());
    }

    [TestMethod]
    public void LogVariables_ManyVariables_WhiteSeparatedPrint()
    {
        logger.LogVariables(" ", ("a", 2), ("b", "ok"));
        Assert.AreEqual("a = 2 b = ok\n", writer.ToString());
    }

    public void Dummy()
    {
    }

    [TestMethod]
    public void LogContext_Call0ArgumentFunction()
    {
        Dummy();

        Assert.AreEqual("DebugLoggerTest - Dummy\n", writer.ToString());
    }

    public void DummyWithParameters(string first, int second)
    {
    }

    [TestMethod]
    public void LogContext_CallManyArgumentsFunction()
    {
        DummyWithParameters("ok", 10);

        Assert.AreEqual("DebugLoggerTest - DummyWithParameters\nfirst: ok\nsecond: 10\n", writer.ToString());
    }

    public void DummyThatCallsAnother(double d)
    {
        Dummy();
    }

    [TestMethod]
    public void LogContext_NestedFunction_LogBothOuterAndInner()
    {
        DummyThatCallsAnother(1.2);

        Assert.AreEqual("DebugLoggerTest - DummyThatCallsAnother\nd: 1.2\nDebugLoggerTest - Dummy\n", writer.ToString());
    }

    [TestMethod]
    public void LogContext_LogExternalStaticMethod()
    {
        var oldWriter = Console.Out;
        try 
        {
            Console.SetOut(logger.Configs.OutWriter);
            Convert.ToInt32(true);
            
            Assert.AreEqual("Convert - ToInt32\nvalue: True\n", writer.ToString());
        }
        finally
        {
            Console.SetOut(oldWriter);
        }
    }

    [TestMethod]
    public void LogContext_LogExternalInstanceMethod()
    {
        var originalWriter = Console.Out;
        try 
        {
            Console.SetOut(logger.Configs.OutWriter);
            var uri = new Uri("http://test.com");
            uri.ToString();
            
            Assert.AreEqual("Uri - ToString\n", writer.ToString());
        }
        finally
        {
            Console.SetOut(originalWriter);
        }
    }
}