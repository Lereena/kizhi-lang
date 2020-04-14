using System;
using System.IO;

namespace kizhi_lang
{
    class Program
    {
        static void Main(string[] args)
        {
            var wrt = new StringWriter();
            var interpreter = new Interpreter(wrt);
            
            interpreter.ExecuteLine("set code");
            interpreter.ExecuteLine(@"def test
    set a 4
print t
set t 5
call test
sub a 3
call test");
            interpreter.ExecuteLine("end set code");
            interpreter.ExecuteLine("add break 1");
            interpreter.ExecuteLine("run");
            interpreter.ExecuteLine("print trace");
            interpreter.ExecuteLine("run");
            interpreter.ExecuteLine("run");
            interpreter.ExecuteLine("print mem");
            
            Console.WriteLine(wrt.ToString());
        }
    }
}