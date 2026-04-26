using Dysh.Core;
using FluentAssertions;
using Xunit;

namespace Dysh.UnitTests;

public sealed class ArgumentParserTests
{
    [Fact]
    public void Map_PositionalArgs_ArePassedAsIs()
    {
        CommandDescriptor result = ArgumentParser.Map("git", null, ["status"], []);

        result.Arguments.Should().Equal("status");
    }

    [Fact]
    public void Map_MultiplePositionalArgs_PreserveOrder()
    {
        CommandDescriptor result = ArgumentParser.Map("ls", null, ["-l", "/home"], []);

        result.Arguments.Should().Equal("-l", "/home");
    }

    [Fact]
    public void Map_SingleCharNamedArg_BoolTrue_EmitsSingleDashFlag()
    {
        CommandDescriptor result = ArgumentParser.Map("tar", null, [true], ["z"]);

        result.Arguments.Should().Equal("-z");
    }

    [Fact]
    public void Map_SingleCharNamedArg_BoolFalse_IsIgnored()
    {
        CommandDescriptor result = ArgumentParser.Map("tar", null, [false], ["z"]);

        result.Arguments.Should().BeEmpty();
    }

    [Fact]
    public void Map_SingleCharNamedArg_WithValue_EmitsFlagAndValue()
    {
        CommandDescriptor result = ArgumentParser.Map("gcc", null, ["output.o"], ["o"]);

        result.Arguments.Should().Equal("-o", "output.o");
    }

    [Fact]
    public void Map_MultiCharNamedArg_BoolTrue_EmitsDoubleDashFlag()
    {
        CommandDescriptor result = ArgumentParser.Map("git", null, [true], ["oneline"]);

        result.Arguments.Should().Equal("--oneline");
    }

    [Fact]
    public void Map_SingleCharNamedArg_WithIntValue_EmitsFlagAndValue()
    {
        // n is 1 char → -n
        CommandDescriptor result = ArgumentParser.Map("git", null, [10], ["n"]);

        result.Arguments.Should().Equal("-n", "10");
    }

    [Fact]
    public void Map_MultiCharNamedArg_WithStringValue_EmitsDoubleDashFlagAndValue()
    {
        CommandDescriptor result = ArgumentParser.Map("ffmpeg", null, ["out.mp4"], ["output"]);

        result.Arguments.Should().Equal("--output", "out.mp4");
    }

    [Fact]
    public void Map_NullNamedArg_IsIgnored()
    {
        CommandDescriptor result = ArgumentParser.Map("cmd", null, [null], ["flag"]);

        result.Arguments.Should().BeEmpty();
    }

    [Fact]
    public void Map_MixedPositionalAndNamedArgs_ProducesCorrectOrder()
    {
        // shell.git.log("HEAD", n: 10, oneline: true)
        // positional: ["HEAD"], named: [10, true] with names ["n", "oneline"]
        CommandDescriptor result = ArgumentParser.Map(
            executable:    "git",
            subcommand:    "log",
            args:          ["HEAD", 10, true],
            argumentNames: ["n", "oneline"]);

        result.Arguments.Should().Equal("HEAD", "-n", "10", "--oneline");
    }

    [Fact]
    public void Map_SetsExecutableCorrectly()
    {
        CommandDescriptor result = ArgumentParser.Map("docker", null, [], []);

        result.Executable.Should().Be("docker");
    }

    [Fact]
    public void Map_SetsSubcommandCorrectly()
    {
        CommandDescriptor result = ArgumentParser.Map("docker", "ps", [], []);

        result.Subcommand.Should().Be("ps");
    }

    [Fact]
    public void Map_NullSubcommand_IsPreserved()
    {
        CommandDescriptor result = ArgumentParser.Map("ls", null, [], []);

        result.Subcommand.Should().BeNull();
    }

    [Fact]
    public void CommandDescriptor_ToCommandLine_WithSubcommand_FormatsCorrectly()
    {
        CommandDescriptor result = ArgumentParser.Map("git", "log", [true], ["oneline"]);

        result.ToCommandLine().Should().Be("git log --oneline");
    }

    [Fact]
    public void CommandDescriptor_ToCommandLine_WithoutSubcommand_FormatsCorrectly()
    {
        CommandDescriptor result = ArgumentParser.Map("ls", null, ["-l"], []);

        result.ToCommandLine().Should().Be("ls -l");
    }
}
