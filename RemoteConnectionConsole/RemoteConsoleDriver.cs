using Renci.SshNet;
using Renci.SshNet.Common;

namespace RemoteConnectionConsole;


public class RemoteConsoleDriver
{
    private readonly SshClient _sshClient;
    private readonly InstanceData _instanceData;
    private Shell _shell = null!;
    private bool _terminated = false;
    private readonly PipeStream _inStream = new PipeStream();

    public Stream? RedirectedStdin;
    public Stream? RedirectedStdout;
    public Stream? RedirectedStderr;

    public RemoteConsoleDriver(InstanceData instanceData)
    {
        if (!instanceData.IsKeyAuth)
        {
            _sshClient = new SshClient(instanceData.Host, instanceData.Port, instanceData.Username,
                instanceData.Password);
        } else {
            _sshClient = new SshClient(instanceData.Host, instanceData.Port, instanceData.Username,
                new PrivateKeyFile(instanceData.Password));
        }
        _instanceData = instanceData;
        Console.Title = $"{instanceData.Username}@{instanceData.Host}";
    }

    private void OnStopped(object? o, EventArgs eventArgs) {
        _terminated = true;
    }
    
    public void Connect(bool cd)
    {
        var iS = RedirectedStdin ?? _inStream;
        var oS = RedirectedStdout ?? Console.OpenStandardOutput();
        var eS = RedirectedStderr ?? Console.OpenStandardError();
        
        _sshClient.Connect();
        if (cd) _sshClient.RunCommand($"cd {_instanceData.WorkingDirectory}");
        _shell = _sshClient.CreateShell(iS, oS, eS, string.Empty, Convert.ToUInt32(Console.WindowWidth),
            Convert.ToUInt32(Console.WindowHeight), Convert.ToUInt32(Console.WindowHeight), Convert.ToUInt32(Console.WindowHeight), new Dictionary<TerminalModes, uint>());
        _shell.Stopping += OnStopped;
        _shell.Start();
    }

    public void Start()
    {
        Console.TreatControlCAsInput = true;

        while (!_terminated)
        {
            if (RedirectedStdin != null) RedirectedStdin.Flush();
            if (RedirectedStdout != null) RedirectedStdout.Flush();
            if (RedirectedStderr != null) RedirectedStderr.Flush();
            if (!Console.KeyAvailable) continue;
            var key = Console.ReadKey(true);
            _inStream.WriteByte((byte) key.KeyChar);
            _inStream.Flush();
        }
    }

    public void Stop()
    {
        _shell.Stop();
        _sshClient.Disconnect();
        _shell.Dispose();
        _sshClient.Dispose();
    }

    public int Copy(string oldPath, string newPath)
    {
        var cmd = _sshClient.CreateCommand($"cp {oldPath} {newPath} -r");
        Console.WriteLine("Pending...");
        var e = cmd.Execute();

        Console.CursorTop -= 1;
        Program.ClearCurrentConsoleLine();

        byte[] buffer = new byte[cmd.OutputStream.Length];
        var read = cmd.OutputStream.Read(buffer);
        var charBuffer = Console.Out.Encoding.GetChars(buffer, 0, read);
        Console.Out.Write(charBuffer, 0, read);

        return cmd.ExitStatus;
    }

    public void Close() {
        if (RedirectedStdin != null) RedirectedStdin.Close();
        if (RedirectedStdout != null) RedirectedStdout.Close();
        if (RedirectedStderr != null) RedirectedStderr.Close();
    }
}