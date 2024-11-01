using CommandLine;
using Newtonsoft.Json;
using Renci.SshNet.Common;
using YamlDotNet.Serialization;

namespace RemoteConnectionConsole;

public class Program {
    private class Options
    {
        [Verb("open", HelpText = "Open Remote Console Session")]
        public class CompileOptions
        {
            [Value(1, MetaName = "input", Required = true, HelpText = "Instance data file")]
            public string? InputFile { get; set; }
            
            [Option("redirect-stdin", Required = false, HelpText = "Redirect stdin to this file")]
            public string? RedirectStdIn { get; set; }
            
            [Option("redirect-stdout", Required = false, HelpText = "Redirect stdout to this file")]
            public string? RedirectStdOut { get; set; }
            
            [Option("redirect-stderr", Required = false, HelpText = "Redirect stderr to this file")]
            public string? RedirectStdErr { get; set; }
        }
    }

    public static int Main(String[] args)
    {
        var parser = new Parser(settings =>
        {
            settings.HelpWriter = null;
        });
        var res = parser.ParseArguments<Options.CompileOptions>(args);
        return res.MapResult(Handle, HandleParseError);
    }
    
    static int HandleParseError(IEnumerable<Error> errs)
    {
        foreach (var err in errs)
        {
            if (err.Tag == ErrorType.HelpRequestedError)
            {
                // TODO : Add help
                continue;
            }

            if (err.Tag == ErrorType.VersionRequestedError)
            {
                Console.WriteLine("RemoteConnectionConsole version 1.0");
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

    static int Handle(Options.CompileOptions options)
    {
        Dictionary<string, string>? instData = null;
        try
        {
            instData = LoadConfig(options.InputFile!);
        }
        catch (Exception e)
        {
            Error(2, "Invalid config file");
        }
        AssertHas(["host", "username", "password", "port", "isKeyAuth"], instData);

        InstanceData? instanceData = InstanceData.ConvertToInstanceData(instData!);
        if (instanceData == null) Error(2, "Invalid config file");
        
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