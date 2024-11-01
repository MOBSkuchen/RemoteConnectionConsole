using Renci.SshNet;
using Renci.SshNet.Common;

namespace RemoteConnectionConsole;


public class SftpDriver
{
    private readonly SshClient _sshClient;
    private readonly InstanceData _instanceData;
    private Shell _shell = null!;
    private bool _terminated = false;
    private readonly PipeStream _inStream = new PipeStream();

    public Stream? RedirectedStdin;
    public Stream? RedirectedStdout;
    public Stream? RedirectedStderr;

    public SftpDriver(InstanceData instanceData)
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
    }

    private void OnStopped(object? o, EventArgs eventArgs) {
        _terminated = true;
    }
    
    public void Connect()
    {
        _sshClient.Connect();
        _shell = _sshClient.CreateShell(RedirectedStdin ?? _inStream, 
            RedirectedStdout ?? Console.OpenStandardOutput(),
            RedirectedStderr ?? Console.OpenStandardError());
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
    
    public void Close() {
        if (RedirectedStdin != null) RedirectedStdin.Close();
        if (RedirectedStdout != null) RedirectedStdout.Close();
        if (RedirectedStderr != null) RedirectedStderr.Close();
    }
}