namespace RemoteConnectionConsole;

public struct InstanceData(
    string host,
    string username,
    int port,
    string password,
    bool isKeyAuth)
{
    public readonly string Host = host;
    public readonly string Username = username;
    public readonly int Port = port;

    public readonly string Password = password;
    public readonly bool IsKeyAuth = isKeyAuth;

    public static InstanceData? ConvertToInstanceData(Dictionary<string, string> instanceDataDictionary) {
        return new InstanceData(instanceDataDictionary["host"], instanceDataDictionary["username"],
            Convert.ToInt32(instanceDataDictionary["port"]), instanceDataDictionary["password"],
            instanceDataDictionary["isKeyAuth"].ToLower() == "true");
    }

    public Dictionary<string, string> ConvertToDictionary()
    {
        Dictionary<string, string> newDict = new()
        {
            {"host", Host},
            {"username", Username},
            {"port", Port.ToString()},
            {"password", Password},
            {"isKeyAuth", IsKeyAuth.ToString()}
        };

        return newDict;
    }
}