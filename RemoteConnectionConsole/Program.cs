using CommandLine;
using Newtonsoft.Json;
using Renci.SshNet.Common;
using YamlDotNet.Serialization;

namespace RemoteConnectionConsole;

public static class CommandsHandler
{
    public const string VERSION = "2.0";
    
    public static void Open()
    {
        Console.WriteLine("Command: open - Open Remote Console Session");
        Console.WriteLine("Options:");
        Console.WriteLine("  --redirect-stdin      Redirect stdin to this file (optional)");
        Console.WriteLine("  --redirect-stdout     Redirect stdout to this file (optional)");
        Console.WriteLine("  --redirect-stderr     Redirect stderr to this file (optional)");
        Console.WriteLine("  -u, --use             Temporarily use an instance (optional)");
        Console.WriteLine("  -c, --cd              Change CWD (optional)");
    }

    public static void Version()
    {
        Console.WriteLine("Command: version - Get the current version");
        Console.WriteLine("Options: None");
    }

    public static void Use()
    {
        Console.WriteLine("Command: use - Select instance to be used further");
        Console.WriteLine("Options:");
        Console.WriteLine("  instance-file         Instance data file (required)");
    }

    public static void Pull()
    {
        Console.WriteLine("Command: pull - Pull file from remote");
        Console.WriteLine("Options:");
        Console.WriteLine("  remote-file           Remote file to pull (required)");
        Console.WriteLine("  -o, --output          Local path to write to (optional, leave blank to auto fill)");
        Console.WriteLine("  -p, --progress        Do not show progress bar (optional)");
        Console.WriteLine("  -u, --use             Temporarily use an instance (optional)");
    }

    public static void Push()
    {
        Console.WriteLine("Command: push - Push file to remote");
        Console.WriteLine("Options:");
        Console.WriteLine("  local-file            Local file to push (required)");
        Console.WriteLine("  -o, --output          Remote path to write to (optional, leave blank to auto fill)");
        Console.WriteLine("  -p, --progress        Do not show progress bar (optional)");
        Console.WriteLine("  -u, --use             Temporarily use an instance (optional)");
    }

    public static void Move()
    {
        Console.WriteLine("Command: move - Move remote file");
        Console.WriteLine("Options:");
        Console.WriteLine("  old-file              Old remote file (required)");
        Console.WriteLine("  new-file              New remote file (required)");
        Console.WriteLine("  -u, --use             Temporarily use an instance (optional)");
    }

    public static void Copy()
    {
        Console.WriteLine("Command: copy - Copy remote file");
        Console.WriteLine("Options:");
        Console.WriteLine("  old-file              Old remote file (required)");
        Console.WriteLine("  new-file              New remote file (required)");
        Console.WriteLine("  -p, --progress        Do not show progress bar (optional)");
        Console.WriteLine("  -u, --use             Temporarily use an instance (optional)");
    }

    public static void Delete()
    {
        Console.WriteLine("Command: del - Delete remote file");
        Console.WriteLine("Options:");
        Console.WriteLine("  remote-file           Remote file to delete (required)");
        Console.WriteLine("  -u, --use             Temporarily use an instance (optional)");
    }

    public static void List()
    {
        Console.WriteLine("Command: list - List CWD");
        Console.WriteLine("Options:");
        Console.WriteLine("  -u, --use             Temporarily use an instance (optional)");
    }

    public static void ChangeDirectory()
    {
        Console.WriteLine("Command: cd - Change CWD for an instance");
        Console.WriteLine("Options:");
        Console.WriteLine("  path                  Path to change to (required)");
        Console.WriteLine("  -u, --use             Temporarily use an instance (optional)");
    }

    public static void PrintGeneralHelp()
    {
        Console.WriteLine("Available Commands:");
        Console.WriteLine("  open       - Open Remote Console Session");
        Console.WriteLine("  use        - Select instance to be used further");
        Console.WriteLine("  pull       - Pull file from remote");
        Console.WriteLine("  push       - Push file to remote");
        Console.WriteLine("  move       - Move remote file");
        Console.WriteLine("  copy       - Copy remote file");
        Console.WriteLine("  del        - Delete remote file");
        Console.WriteLine("  list       - List CWD");
        Console.WriteLine("  cd         - Change CWD for an instance");
        Console.WriteLine("  help       - Print help [about a command]");
        Console.WriteLine("  --version  - Print version");
        Console.WriteLine("\nUse 'RemoteConnectionConsole help <name>' to get detailed options for each command.");
    }

    public static void Help()
    {
        Console.WriteLine("Command: help - Get help");
        Console.WriteLine("Options:");
        Console.WriteLine("  command               Command to learn about (optional)");
    }
}

public class Program {
    private class Options
    {
        [Verb("open", HelpText = "Open Remote Console Session")]
        public class OpenOptions
        {
            [Option("redirect-stdin", Required = false, HelpText = "Redirect stdin to this file")]
            public string? RedirectStdIn { get; set; }
            
            [Option("redirect-stdout", Required = false, HelpText = "Redirect stdout to this file")]
            public string? RedirectStdOut { get; set; }
            
            [Option("redirect-stderr", Required = false, HelpText = "Redirect stderr to this file")]
            public string? RedirectStdErr { get; set; }
            
            [Option('u', "use", HelpText = "Temporarily use an instance")]
            public string? Using { get; set; }

            [Option('c', "cd", FlagCounter = true, HelpText = "Change CWD")]
            public int Cd { get; set; }
        }
        
        [Verb("use", HelpText = "Select instance to be used further")]
        public class UseOptions
        {
            [Value(1, MetaName = "instance-file", Required = true, HelpText = "Instance data file")]
            public string? InputFile { get; set; }
        }
        
        [Verb("pull", HelpText = "Pull file from remote")]
        public class PullOptions
        {
            [Value(1, MetaName = "remote-file", Required = true, HelpText = "Remote file")]
            public string? InputFile { get; set; }
            
            [Option('o', "output", HelpText = "Local path to write to, leave blank to auto fill")]
            public string? Output { get; set; }
            
            [Option('p', "progress", Required = false, FlagCounter = true, HelpText = "Do not show progress bar")]
            public int Progress { get; set; }
            
            [Option('u', "use", HelpText = "Temporarily use an instance")]
            public string? Using { get; set; }
        }
        
        [Verb("push", HelpText = "Push file to remote")]
        public class PushOptions
        {
            [Value(1, MetaName = "local-file", Required = true, HelpText = "Local file")]
            public string? InputFile { get; set; }
            
            [Option('o', "output", HelpText = "Remote path to write to, leave blank to auto fill")]
            public string? Output { get; set; }
            
            [Option('p', "progress", Required = false, FlagCounter = true, HelpText = "Do not show progress bar")]
            public int Progress { get; set; }
            
            [Option('u', "use", HelpText = "Temporarily use an instance")]
            public string? Using { get; set; }
        }
        
        [Verb("move", HelpText = "Move remote file")]
        public class MoveOptions
        {
            [Value(1, MetaName = "old-file", Required = true, HelpText = "Old remote file")]
            public string? InputFile { get; set; }
            
            [Value(2, MetaName = "new-file", Required = true, HelpText = "New remote file")]
            public string? TargetFile { get; set; }
            
            [Option('u', "use", HelpText = "Temporarily use an instance")]
            public string? Using { get; set; }
        }
        
        [Verb("copy", HelpText = "Copy remote file")]
        public class CopyOptions
        {
            [Value(1, MetaName = "old-file", Required = true, HelpText = "Old remote file")]
            public string? InputFile { get; set; }
            
            [Value(2, MetaName = "new-file", Required = true, HelpText = "New remote file")]
            public string? TargetFile { get; set; }
            
            [Option('p', "progress", Required = false, FlagCounter = true, HelpText = "Do not show progress bar")]
            public int Progress { get; set; }
            
            [Option('u', "use", HelpText = "Temporarily use an instance")]
            public string? Using { get; set; }
        }
        
        [Verb("del", HelpText = "Delete remote file")]
        public class DeleteOptions
        {
            [Value(1, MetaName = "remote-file", Required = true, HelpText = "Remote file")]
            public string? InputFile { get; set; }
            
            [Option('u', "use", HelpText = "Temporarily use an instance")]
            public string? Using { get; set; }
        }
        
        [Verb("list", HelpText = "List CWD")]
        public class ListOptions
        {
            
            [Option('u', "use", HelpText = "Temporarily use an instance")]
            public string? Using { get; set; }
        }
        
        [Verb("cd", HelpText = "Change CWD for an instance")]
        public class CdOptions
        {
            [Value(1, MetaName = "path", Required = true, HelpText = "Path")]
            public string? Path { get; set; }
            
            [Option('u', "use", HelpText = "Temporarily use an instance")]
            public string? Using { get; set; }
        }
        
        [Verb("console", HelpText = "Open persistent session")]
        public class ConsoleOptions
        {
            [Option('u', "use", HelpText = "Temporarily use an instance")]
            public string? Using { get; set; }
        }

        [Verb("help", HelpText = "Get help")]
        public class HelpOptions
        {
            [Value(1, MetaName = "cmd", Required = false, HelpText = "Command")]
            public string? Cmd { get; set; }
        }
        
        [Verb("version")]
        public class VersionOptions { }
    }

    static Parser _parser = null!;

    private static InstanceData? _instanceData;
    private static SftpDriver? _sftpDriver;

    static int ParseArguments(String[] args)
    {
        var res = _parser.ParseArguments<Options.UseOptions, Options.OpenOptions, Options.PullOptions, 
            Options.PushOptions, Options.MoveOptions, Options.CopyOptions, Options.ListOptions, Options.DeleteOptions, 
            Options.CdOptions, Options.HelpOptions, Options.VersionOptions, Options.ConsoleOptions>(args);
        try
        {
            return res.MapResult(
                (Options.OpenOptions options) => HandleOpen(options),
                (Options.UseOptions options) => HandleUse(options),
                (Options.PullOptions options) => HandlePull(options),
                (Options.PushOptions options) => HandlePush(options),
                (Options.MoveOptions options) => HandleMove(options),
                (Options.CopyOptions options) => HandleCopy(options),
                (Options.ListOptions options) => HandleList(options),
                (Options.DeleteOptions options) => HandleDelete(options),
                (Options.CdOptions options) => HandleCd(options),
                (Options.HelpOptions options) => HandleHelp(options),
                (Options.VersionOptions options) => HandleVersion(options),
                (Options.ConsoleOptions options) => HandleConsole(options),
                HandleParseError);
        }
        catch (Exception e)
        {
            Error(-1, $"Unexpected exception: {e}", true);
        }

        return 0;
    }

    public static bool HardError = true;
    
    public static int Main(String[] args)
    {
        _parser = new Parser(settings =>
        {
            settings.HelpWriter = null;
            settings.AutoHelp = false;
            settings.AutoVersion = false;
        });
        Console.CancelKeyPress += delegate
        {
            Console.WriteLine("Aborted");
            Environment.Exit(0);
        };

        return ParseArguments(args);
    }
    
    static void PrintVersion() => Console.WriteLine("RemoteConnectionConsole version " + CommandsHandler.VERSION);

    static int HandleVersion(Options.VersionOptions options)
    {
        Console.WriteLine("You are currently using the amazing RemoteConnectionConsole version " + CommandsHandler.VERSION);
        return 0;
    }
    
    static SftpDriver? GetSftpDriver(string? use)
    {
        if (_sftpDriver != null) return _sftpDriver;
        var instanceData = GetInstanceData(use);
        if (instanceData == null) return null;
        var sftpDriver = new SftpDriver(instanceData.Value);
        sftpDriver.Connect();
        _sftpDriver = sftpDriver;
        return sftpDriver;
    }

    static InstanceData? GetInstanceData(string? use)
    {
        if (use == null)
        {
            _instanceData = LoadCurrentlyUsed(null);
            return _instanceData;
        };
        return LoadCurrentlyUsed(use);
    }
    
    public static void ClearCurrentConsoleLine()
    {
        int currentLineCursor = Console.CursorTop;
        Console.SetCursorPosition(0, Console.CursorTop);
        Console.Write(new string(' ', Console.WindowWidth)); 
        Console.SetCursorPosition(0, currentLineCursor);
    }
    
    static int HandleParseError(IEnumerable<Error> errs)
    {
        foreach (var err in errs)
        {
            string message;
            switch (err.Tag)
            {
                case ErrorType.BadFormatTokenError:
                    message = "Bad format token";
                    break;
                case ErrorType.MissingValueOptionError:
                    message = "Missing value option";
                    break;
                case ErrorType.UnknownOptionError:
                    message = "Unknown option";
                    break;
                case ErrorType.MissingRequiredOptionError:
                    message = "Missing required option";
                    break;
                case ErrorType.MutuallyExclusiveSetError:
                    message = "Mutually exclusive set";
                    break;
                case ErrorType.BadFormatConversionError:
                    message = "Bad format conversion";
                    break;
                case ErrorType.SequenceOutOfRangeError:
                    message = "Sequence out of range";
                    break;
                case ErrorType.RepeatedOptionError:
                    message = "Repeated option";
                    break;
                case ErrorType.NoVerbSelectedError:
                    message = "No verb selected";
                    break;
                case ErrorType.BadVerbSelectedError:
                    message = "Invalid option";
                    break;
                case ErrorType.SetValueExceptionError:
                    message = "Set value exception";
                    break;
                case ErrorType.InvalidAttributeConfigurationError:
                    message = "Invalid attribute configuration";
                    break;
                case ErrorType.MissingGroupOptionError:
                    message = "Missing group option";
                    break;
                case ErrorType.GroupOptionAmbiguityError:
                    message = "Group option ambiguity";
                    break;
                case ErrorType.MultipleDefaultVerbsError:
                    message = "Multiple default verbs";
                    break;
                default:
                    message = "Unknown";
                    break;
            }
            Error(6, $"Parser error: {message}");
            return 6;
        }

        return 1;
    }

    static InstanceData? LoadInstance(string filePath)
    {
        Dictionary<string, string>? instData = null;
        try
        {
            instData = LoadConfig(filePath);
        }
        catch (Exception e)
        {
            Error(2, "Invalid config file");
            return null;
        }

        if (!AssertHas(["host", "username", "password", "port", "isKeyAuth"], instData)) return null;

        InstanceData? instanceData = InstanceData.ConvertToInstanceData(instData!, filePath);
        if (instanceData == null)
        {
            Error(2, "Invalid config file");
            return null;
        }
        return instanceData;
    }

    static InstanceData? LoadCurrentlyUsed(string? overwrite)
    {
        if (overwrite != null) return LoadInstance(overwrite);
        if (!File.Exists(".rcc-used"))
        {
            Error(7, "Not currently using any instance!");
            return null;
        }
        return LoadInstance(File.ReadAllText(".rcc-used"));
    }

    static int HandleUse(Options.UseOptions options)
    {
        if ((options.InputFile! == "." || options.InputFile! == "/" || options.InputFile! == "-") && File.Exists(".rcc-used"))
        {
            File.Delete(".rcc-used");
            Console.WriteLine("Using: None");
        } else
        {
            LoadInstance(options.InputFile!);
            File.WriteAllText(".rcc-used", Path.GetFullPath(options.InputFile!));
            Console.WriteLine($"Using: {options.InputFile}");
        }
        return 0;
    }

    static int HandlePull(Options.PullOptions options)
    {
        var sftpDriver = GetSftpDriver(options.Using);
        if (sftpDriver == null) return -1;
        var outputPath = options.Output ?? Path.GetFileName(options.InputFile!);
        sftpDriver.Pull(options.InputFile!, outputPath, options.Progress == 0);
        return 0;
    }
    
    static int HandlePush(Options.PushOptions options)
    {
        var sftpDriver = GetSftpDriver(options.Using);
        if (sftpDriver == null) return -1;
        var outputPath = options.Output ?? Path.GetFileName(options.InputFile!);
        sftpDriver.Push(options.InputFile!, outputPath, options.Progress == 0);
        return 0;
    }
    
    static int HandleMove(Options.MoveOptions options)
    {
        var sftpDriver = GetSftpDriver(options.Using);
        if (sftpDriver == null) return -1;
        sftpDriver.Move(options.InputFile!, options.TargetFile!, false, false);
        return 0;
    }
    
    static int HandleCopy(Options.CopyOptions options)
    {
        var sftpDriver = GetSftpDriver(options.Using);
        if (sftpDriver == null) return -1;
        sftpDriver.Move(options.InputFile!, options.TargetFile!, true, options.Progress == 0);
        return 0;
    }
    
    static int HandleList(Options.ListOptions options)
    {
        var sftpDriver = GetSftpDriver(options.Using);
        if (sftpDriver == null) return -1;
        sftpDriver.List();
        return 0;
    }
    
    static int HandleDelete(Options.DeleteOptions options)
    {
        var sftpDriver = GetSftpDriver(options.Using);
        if (sftpDriver == null) return -1;
        sftpDriver.Delete(options.InputFile!);
        return 0;
    }

    static int HandleCd(Options.CdOptions options)
    {
        var sftpDriver = GetSftpDriver(options.Using);
        if (sftpDriver == null) return -1;
        sftpDriver.InstanceData.WorkingDirectory = sftpDriver.ChangeDirectory(options.Path!);
        sftpDriver.InstanceData.WriteToFile();
        _instanceData = sftpDriver.InstanceData;
        Console.WriteLine($"Set new working directory to {sftpDriver.InstanceData.WorkingDirectory} for instance {sftpDriver.InstanceData.Path}");
        return 0;
    }

    static int HandleConsole(Options.ConsoleOptions options)
    {
        if (options.Using != null) GetInstanceData(options.Using);
        if (File.Exists(".rcc-used")) GetInstanceData(null);
        HardError = false;
        while (true)
        {
            if (_instanceData == null) Console.Write("None > ");
            else Console.Write($"{_instanceData.Value.Username}@{_instanceData.Value.Host} {_instanceData.Value.WorkingDirectory} > ");

            string cmd = Console.ReadLine() ?? "exit";
            if (cmd == "exit") return 0;
            if (cmd.StartsWith("console"))
            {
                Console.WriteLine("Can not start console in a currently running console session!");
                continue;
            }
            ParseArguments(cmd.SplitArgs());
        }
    }

    static int HandleHelp(Options.HelpOptions options)
    {
        PrintVersion();
        Console.WriteLine();
        
        if (options.Cmd == null) CommandsHandler.PrintGeneralHelp();
        else
        {
            switch (options.Cmd)
            {
                case "open": CommandsHandler.Open(); break;
                case "help": CommandsHandler.Help(); break;
                case "del": CommandsHandler.Delete(); break;
                case "cd": CommandsHandler.ChangeDirectory(); break;
                case "pull": CommandsHandler.Pull(); break;
                case "push": CommandsHandler.Push(); break;
                case "move": CommandsHandler.Move(); break;
                case "copy": CommandsHandler.Copy(); break;
                case "list": CommandsHandler.List(); break;
                case "use": CommandsHandler.Use(); break;
                case "version": CommandsHandler.Version(); break;
                default: Error(6, "Unknown command"); return 6;
            }
        }
        
        Console.WriteLine();
        Console.WriteLine("End of help");
        
        return 0;
    }

    static int HandleOpen(Options.OpenOptions options)
    {
        var instanceData = LoadCurrentlyUsed(options.Using);
        
        RemoteConsoleDriver remoteConsoleDriver = new RemoteConsoleDriver(instanceData!.Value);
        
        if (options.RedirectStdIn != null)
        {
            remoteConsoleDriver.RedirectedStdin = File.Open(options.RedirectStdIn, FileMode.OpenOrCreate);
            remoteConsoleDriver.RedirectedStdin.Flush();
        }
        
        if (options.RedirectStdOut != null)
        {
            remoteConsoleDriver.RedirectedStdout = File.Open(options.RedirectStdOut, FileMode.OpenOrCreate);
            remoteConsoleDriver.RedirectedStdout.Flush();
        }
        
        if (options.RedirectStdErr != null)
        {
            remoteConsoleDriver.RedirectedStderr = File.Open(options.RedirectStdErr, FileMode.OpenOrCreate);
            remoteConsoleDriver.RedirectedStderr.Flush();
        }

        try
        {
            remoteConsoleDriver.Connect(options.Cd != 0);
            remoteConsoleDriver.Start();
            remoteConsoleDriver.Stop();
        }
        catch (System.Net.Sockets.SocketException e)
        {
            Error(4, "Host could not be reached");
            return 4;
        }
        catch (SshAuthenticationException e)
        {
            Error(5, "Authentication failed");
            return 5;
        }
        catch (Exception e)
        {
            Error(-1, "Unexpected exception:\n" + e);
            return -1;
        }
        finally
        {
            remoteConsoleDriver.Close();
        }
        return 0;
    }
    
    static Dictionary<string, string>? LoadConfig(String path)
    {
        if (path.EndsWith(".json")) return JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(path));
        if (path.EndsWith(".yml")) return new Deserializer().Deserialize<Dictionary<string, string>>(File.ReadAllText(path));
        Error(3, "Invalid file type. Supported are .json & .yml!");
        return null;
    }
    
    static bool AssertHas(String[] fields, Dictionary<string, string>? dict)
    {
        if (dict == null)
        {
            Error(2, "Invalid config file!");
            return false;
        }
        foreach (var field in fields)
        {
            if (!dict.ContainsKey(field))
            {
                Error(2, "Invalid config file!");
                return false;
            }
        }
        return true;
    }

    public static void Error(int err, String msg, bool forceExit = false)
    {
        Console.WriteLine($"Error ({err}) : {msg}");
        if (HardError || forceExit)Environment.Exit(err);
    }
}