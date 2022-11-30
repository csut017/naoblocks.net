namespace NaoBlocks.Client.Terminal.Tests
{
    public partial class InstructionBaseTests
    {
        [Fact]
        public void DisplayHelpTextDisplaysNothing()
        {
            var instruction = new TestInstruction();
            var console = new TestConsole();
            instruction.DisplayHelpText(console);
            Assert.Empty(console.Output);
        }

        [Fact]
        public void ParseNamedArgsHandesNamedArgs()
        {
            var instruction = new TestInstruction();
            var (args, named) = instruction.ParseArgs(new[] { "plain", "--named", "--value=one" });
            Assert.Equal(new[] { "plain" }, args);
            Assert.Equal(new[] { "named", "value" }, named.Select(a => a.Key).ToArray());
            Assert.Equal(new[] { string.Empty, "one" }, named.SelectMany(a => a.Value).ToArray());
        }

        [Fact]
        public void ParseNamedArgsHandesRepeatedNamedArgs()
        {
            var instruction = new TestInstruction();
            var (args, named) = instruction.ParseArgs(new[] { "plain", "--named", "--value=one", "--value=two" });
            Assert.Equal(new[] { "plain" }, args);
            Assert.Equal(new[] { "named", "value" }, named.Select(a => a.Key).ToArray());
            Assert.Equal(new[] { string.Empty, "one", "two" }, named.SelectMany(a => a.Value).ToArray());
        }
    }
}