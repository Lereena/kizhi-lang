namespace kizhi_lang
{
    public class Command
    {
        public int LineNum { get; }
        public string Name { get; }
        public string[] Arguments { get; }

        public Command(int lineNum, string name, string[] arguments)
        {
            LineNum = lineNum;
            Name = name;
            Arguments = arguments;
        }
    }
}