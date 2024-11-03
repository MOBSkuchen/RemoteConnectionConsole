using System.Text.Json;
using YamlDotNet.Serialization;

namespace RemoteConnectionConsole;

public struct InstanceData(
    string host,
    string username,
    int port,
    string password,
    bool isKeyAuth,
    string workingDirectory,
    string path)
{
    public readonly string Host = host;
    public readonly string Username = username;
    public readonly int Port = port;

    public readonly string Password = password;
    public readonly bool IsKeyAuth = isKeyAuth;
    public string WorkingDirectory = workingDirectory;

    public readonly string Path = path;

    public static InstanceData? ConvertToInstanceData(Dictionary<string, string> instanceDataDictionary, string path) {
        return new InstanceData(instanceDataDictionary["host"], instanceDataDictionary["username"],
            Convert.ToInt32(instanceDataDictionary["port"]), instanceDataDictionary["password"],
            instanceDataDictionary["isKeyAuth"].ToLower() == "true", instanceDataDictionary.GetValueOrDefault("workingDirectory", "/"), path);
        
    }

    private Dictionary<string, string> ConvertToDictionary()
    {
        Dictionary<string, string> newDict = new()
        {
            {"host", Host},
            {"username", Username},
            {"port", Port.ToString()},
            {"password", Password},
            {"isKeyAuth", IsKeyAuth.ToString()},
            {"workingDirectory", WorkingDirectory}
        };

        return newDict;
    }

    public void WriteToFile()
    {
        if (Path.EndsWith(".json")) JsonSerializer.Serialize(File.OpenWrite(Path), ConvertToDictionary());
        else if (Path.EndsWith(".yml")) File.WriteAllText(Path, new Serializer().Serialize(ConvertToDictionary()));
    }
}