using CommandLine;
using Newtonsoft.Json;
using Renci.SshNet.Common;
using YamlDotNet.Serialization;

namespace RemoteConnectionConsole;

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
    }
    public static int Main(String[] args)
    {
        var parser = new Parser(settings =>
        {
            settings.HelpWriter = null;
        });
        var res = parser.ParseArguments<Options.UseOptions, Options.OpenOptions, Options.PullOptions, 
            Options.PushOptions, Options.MoveOptions, Options.CopyOptions, Options.ListOptions, Options.DeleteOptions, 
            Options.CdOptions>(args);
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
            HandleParseError);
    }
    
    static void PrintVersion() => Console.WriteLine("RemoteConnectionConsole version 1.0");
    
    static int HandleParseError(IEnumerable<Error> errs)
    {
        foreach (var err in errs)
        {
            if (err.Tag == ErrorType.HelpRequestedError)
            {
                PrintVersion();
                Console.WriteLine();
                Console.WriteLine("usage => RemoteConnectionConsole <...> (--help) (--version)");
                Console.WriteLine();
                Console.WriteLine(" OPTIONS:");
                Console.WriteLine();
                Console.WriteLine("  instance-file > A JSON or YAML file containing data about the instance");
                Console.WriteLine("                -> host: <target host>");
                Console.WriteLine("                -> username: <login username>");
                Console.WriteLine("                -> password: <login password>");
                Console.WriteLine("                -> port: <host port>");
                Console.WriteLine("                !> Usually 22");
                Console.WriteLine("                -> isKeyAuth: <boolean>");
                Console.WriteLine("                !> If this is set to true, the password should contain the path of the key file in openssh format");
                Console.WriteLine();
                Console.WriteLine("  --redirect-stdin > The file that should replace the stdin stream");
                Console.WriteLine();
                Console.WriteLine("  --redirect-stdout > The file that should replace the stdout stream");
                Console.WriteLine();
                Console.WriteLine("  --redirect-stderr > The file that should replace the stderr stream");
                Console.WriteLine();
                Console.WriteLine("  --help > Display this help message");
                Console.WriteLine();
                Console.WriteLine("  --version > Display the current version");
                Console.WriteLine();
                Console.WriteLine("End of help");
                continue;
            }

            if (err.Tag == ErrorType.VersionRequestedError)
            {
                PrintVersion();
                continue;
            }
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
        }
        AssertHas(["host", "username", "password", "port", "isKeyAuth"], instData);

        InstanceData? instanceData = InstanceData.ConvertToInstanceData(instData!, filePath);
        if (instanceData == null) Error(2, "Invalid config file");
        return instanceData;
    }

    static InstanceData? LoadCurrentlyUsed(string? overwrite)
    {
        if (overwrite != null) return LoadInstance(overwrite);
        if (!File.Exists(".rcc-used")) Error(7, "Not currently using any instance!");
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
        var instanceData = LoadCurrentlyUsed(options.Using);
        var sftpDriver = new SftpDriver(instanceData!.Value);
        sftpDriver.Connect();
        var outputPath = options.Output ?? Path.GetFileName(options.InputFile!);
        sftpDriver.Pull(options.InputFile!, outputPath, options.Progress == 0);
        return 0;
    }
    
    static int HandlePush(Options.PushOptions options)
    {
        var instanceData = LoadCurrentlyUsed(options.Using);
        var sftpDriver = new SftpDriver(instanceData!.Value);
        sftpDriver.Connect();
        var outputPath = options.Output ?? Path.GetFileName(options.InputFile!);
        sftpDriver.Push(options.InputFile!, outputPath, options.Progress == 0);
        return 0;
    }
    
    static int HandleMove(Options.MoveOptions options)
    {
        var instanceData = LoadCurrentlyUsed(options.Using);
        var sftpDriver = new SftpDriver(instanceData!.Value);
        sftpDriver.Connect();
        sftpDriver.Move(options.InputFile!, options.TargetFile!, false, false);
        return 0;
    }
    
    static int HandleCopy(Options.CopyOptions options)
    {
        var instanceData = LoadCurrentlyUsed(options.Using);
        var sftpDriver = new SftpDriver(instanceData!.Value);
        sftpDriver.Connect();
        sftpDriver.Move(options.InputFile!, options.TargetFile!, true, options.Progress == 0);
        return 0;
    }
    
    static int HandleList(Options.ListOptions options)
    {
        var instanceData = LoadCurrentlyUsed(options.Using);
        var sftpDriver = new SftpDriver(instanceData!.Value);
        sftpDriver.Connect();
        sftpDriver.List();
        return 0;
    }
    
    static int HandleDelete(Options.DeleteOptions options)
    {
        var instanceData = LoadCurrentlyUsed(options.Using);
        var sftpDriver = new SftpDriver(instanceData!.Value);
        sftpDriver.Connect();
        sftpDriver.Delete(options.InputFile!);
        return 0;
    }

    static int HandleCd(Options.CdOptions options)
    {
        var instanceData = LoadCurrentlyUsed(options.Using)!.Value;
        var sftpDriver = new SftpDriver(instanceData);
        sftpDriver.Connect();
        instanceData.WorkingDirectory = sftpDriver.ChangeDirectory(options.Path!);
        instanceData.WriteToFile();
        Console.WriteLine($"Set new working directory to {instanceData.WorkingDirectory} for instance {instanceData.Path}");
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
        }
        catch (SshAuthenticationException e)
        {
            Error(5, "Authentication failed");
        }
        catch (Exception e)
        {
            Error(-1, "Unexpected exception:\n" + e);
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
        return new Dictionary<string, string>();
    }
    
    static void AssertHas(String[] fields, Dictionary<string, string>? dict)
    {
        if (dict == null)
        {
            Error(2, "Invalid config file!");
            return;
        }
        foreach (var field in fields)
        {
            if (!dict.ContainsKey(field)) Error(2, "Invalid config file!");
        }
    }

    public static void Error(int err, String msg)
    {
        Console.WriteLine($"Error ({err}) : {msg}");
        Environment.Exit(err);
    }
}