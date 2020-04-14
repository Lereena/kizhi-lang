using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace kizhi_lang
{
    public class Interpreter
    {
        private readonly TextWriter _writer;

        private readonly Dictionary<string, (int value, int lastChange)> _variables 
            = new Dictionary<string, (int, int)>();
        private readonly Dictionary<string, List<Command>> _functions 
            = new Dictionary<string, List<Command>>();
        private readonly Dictionary<string, Action<int, string[]>> _programCommands 
            = new Dictionary<string, Action<int, string[]>>();

        private readonly HashSet<int> _breakPoints 
            = new HashSet<int>();
        private readonly Stack<(int lineNum, string name, Command breakStop)> _callStack 
            = new Stack<(int, string, Command)>();
        private readonly LinkedList<Command> _executionQueue 
            = new LinkedList<Command>();

        private LinkedList<Command> _mainQueue;

        public Interpreter(TextWriter writer)
        {
            _writer = writer;

            _programCommands.Add("set", 
                (callingLine, parameters) 
                    => _variables[parameters[0]] = (int.Parse(parameters[1]), callingLine));
            _programCommands.Add("sub",
                (callingLine, parameters) 
                    => SafeExecuteCommand(parameters[0], 
                        () => _variables[parameters[0]] 
                            = (_variables[parameters[0]].value - int.Parse(parameters[1]), callingLine)));
            _programCommands.Add("print",
                (callingLine, parameters) 
                    => SafeExecuteCommand(parameters[0], 
                        () => _writer.WriteLine(_variables[parameters[0]].value)));
            _programCommands.Add("rem",
                (callingLine, parameters) 
                    => SafeExecuteCommand(parameters[0], () => _variables.Remove(parameters[0])));
        }

        private void SafeExecuteCommand(string variableName, Action action)
        {
            if (_variables.ContainsKey(variableName))
                action();
            else
                _writer.WriteLine("Variable is absent");
        }
        
        public void ExecuteLine(string command)
        {
            if (command.StartsWith("add break"))
            {
                var splitCommand = command.Split();
                _breakPoints.Add(int.Parse(splitCommand[2]));
                return;
            }
            
            switch (command)
            {
                case "set code":
                    break;
                case "end set code":
                    break;
                case "run":
                    _variables.Clear();
                    RunProgram();
                    break;
                case "print trace":
                    foreach (var (line, funcName, breaker) in _callStack)
                        _writer.WriteLine($"{line} {funcName}");
                    break;
                case "print mem":
                    foreach (var variable in _variables)
                        _writer.WriteLine($"{variable.Key} {variable.Value.value} {variable.Value.lastChange}");
                    break;
                case "step":
                    Step();
                    break;
                case "step over":
                    StepOver();
                    break;
                default:
                    Interpret(command);
                    break;
            }
        }

        private void Interpret(string code)
        {
            var splitCode = code.Split(new[] {"\r\n", "\n", "\r"},
                StringSplitOptions.RemoveEmptyEntries);
            var funcToWrite = "";
            
            for (var lineNum = 0; lineNum < splitCode.Length; lineNum++)
            {
                var currentLine = splitCode[lineNum];
                
                if (currentLine.StartsWith("def"))
                {
                    var funcName = currentLine.Substring(4);
                    _functions[funcName] = new List<Command>();
                    funcToWrite = funcName;
                    continue;
                }
                
                if (currentLine.StartsWith("    "))
                {
                    _functions[funcToWrite].Add(ParseCommand(lineNum, currentLine.TrimStart()));
                    continue;
                }

                _executionQueue.AddLast(ParseCommand(lineNum, currentLine));
            }
        }

        private void RunProgram()
        {
            _mainQueue = new LinkedList<Command>(_executionQueue);
            while (_mainQueue.Count > 0)
            {
                var command = _mainQueue.First.Value;
                if (_breakPoints.Contains(command.LineNum))
                {
                    _breakPoints.Remove(command.LineNum);
                    return;
                }
                
                _mainQueue.RemoveFirst();
                if (command.Name == "call")
                {
                    var funcName = command.Arguments[0];
                    _callStack.Push(
                        (command.LineNum, funcName, _mainQueue.Count > 0 ? _mainQueue.First.Value : null));
                    PlaceFuncToExecutionQueue(funcName);
                    continue;
                }

                _breakPoints.Remove(command.LineNum);
                if (_callStack.Count > 0 
                        && _callStack.Peek().breakStop != null
                        && command.LineNum == _callStack.Peek().breakStop.LineNum)
                    _callStack.Pop();

                _programCommands[command.Name](command.LineNum, command.Arguments);
            }
        }

        private void Step()
        {
            if (_executionQueue.Count == 0) return;
            var currentCommand = _executionQueue.First.Value;
            _executionQueue.RemoveFirst();
            
            if (currentCommand.Name == "call")
            {
                var funcName = currentCommand.Arguments[0];
                var stopCommand = _executionQueue.Count > 0 ? _executionQueue.First.Value : null;
                _callStack.Push((currentCommand.LineNum, funcName, stopCommand));
                PlaceFuncToExecutionQueue(funcName);

                _breakPoints.Remove(_executionQueue.First.Value.LineNum);
                return;
            }

            _breakPoints.Remove(currentCommand.LineNum);

            if (_callStack.Count > 0 
                    && _callStack.Peek().breakStop != null
                    && currentCommand.LineNum == _callStack.Peek().breakStop.LineNum)
                _callStack.Pop();

            _programCommands[currentCommand.Name](currentCommand.LineNum,
                currentCommand.Arguments);
        }

        private void StepOver()
        {
            if (_executionQueue.Count == 0) return;

            if (_executionQueue.First.Value.Name != "call")
            {
                Step();
                return;
            }

            var currentCommand = _executionQueue.First.Value;
            _executionQueue.RemoveFirst();

            var funcName = currentCommand.Arguments[0];
            var stopCommand = _executionQueue.Count > 0 ? _executionQueue.First.Value : null;
            _callStack.Push((currentCommand.LineNum, funcName, stopCommand));
            PlaceFuncToExecutionQueue(funcName);

            _breakPoints.Remove(_executionQueue.Last.Value.LineNum);

            if (stopCommand == null)
            {
                _breakPoints.Clear();
                RunProgram();
                _callStack.Clear();
            }
            else
                while (_executionQueue.First.Value.LineNum != stopCommand.LineNum)
                    Step();
        }

        private Command ParseCommand(int lineNum, string line)
        {
            var splitLine = line.Split();
            return new Command(lineNum, splitLine[0], splitLine.Skip(1).ToArray());
        }
        
        private void PlaceFuncToExecutionQueue(string name)
        {
            var funcToPlace = _functions[name];
            for (var i = funcToPlace.Count - 1; i >= 0; i--)
                _mainQueue.AddFirst(funcToPlace[i]);
        }
    }
}