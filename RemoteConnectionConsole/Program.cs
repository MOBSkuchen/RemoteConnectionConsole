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
            [Value(1, MetaName = "instance-file", Required = true, HelpText = "Instance data file")]
            public string? InputFile { get; set; }
            
            [Option("redirect-stdin", Required = false, HelpText = "Redirect stdin to this file")]
            public string? RedirectStdIn { get; set; }
            
            [Option("redirect-stdout", Required = false, HelpText = "Redirect stdout to this file")]
            public string? RedirectStdOut { get; set; }
            
            [Option("redirect-stderr", Required = false, HelpText = "Redirect stderr to this file")]
            public string? RedirectStdErr { get; set; }
        }
        
        [Verb("use", HelpText = "Select instance to be used further")]
        public class UseOptions
        {
            [Value(1, MetaName = "instance-file", Required = true, HelpText = "Instance data file")]
            public string? InputFile { get; set; }
        }
        /*
        [Option('u', "use", HelpText = "Temporarily use an instance")]
        public string? Using { get; set; }
         */
    }

    public static int Main(String[] args)
    {
        var parser = new Parser(settings =>
        {
            settings.HelpWriter = null;
        });
        var res = parser.ParseArguments<Options.UseOptions, Options.OpenOptions>(args);
        return res.MapResult(
            (Options.OpenOptions options) => HandleOpen(options),
            (Options.UseOptions options) => HandleUse(options),
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
                    message = "Bad verb selected";
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

        InstanceData? instanceData = InstanceData.ConvertToInstanceData(instData!);
        if (instanceData == null) Error(2, "Invalid config file");
        return instanceData;
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

    static int HandleOpen(Options.OpenOptions options)
    {
        var instanceData = LoadInstance(options.InputFile!);
        
        SftpDriver sftpDriver = new SftpDriver(instanceData!.Value);
        
        if (options.RedirectStdIn != null)
        {
            sftpDriver.RedirectedStdin = File.Open(options.RedirectStdIn, FileMode.OpenOrCreate);
            sftpDriver.RedirectedStdin.Flush();
        }
        
        if (options.RedirectStdOut != null)
        {
            sftpDriver.RedirectedStdout = File.Open(options.RedirectStdOut, FileMode.OpenOrCreate);
            sftpDriver.RedirectedStdout.Flush();
        }
        
        if (options.RedirectStdErr != null)
        {
            sftpDriver.RedirectedStderr = File.Open(options.RedirectStdErr, FileMode.OpenOrCreate);
            sftpDriver.RedirectedStderr.Flush();
        }

        try
        {
            sftpDriver.Connect();
            sftpDriver.Start();
            sftpDriver.Stop();
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
            sftpDriver.Close();
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

    static void Error(int err, String msg)
    {
        Console.WriteLine($"Error ({err}) : {msg}");
        Environment.Exit(err);
    }
}