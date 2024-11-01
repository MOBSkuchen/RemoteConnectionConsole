using Renci.SshNet;

namespace RemoteConnectionConsole;

public class SftpDriver
{
    private readonly SshClient _sshClient;
    private readonly SftpClient _sftpClient;
    private readonly InstanceData _instanceData;

    public SftpDriver(InstanceData instanceData)
    {
        if (!instanceData.IsKeyAuth)
        {
            _sshClient = new SshClient(instanceData.Host, instanceData.Port, instanceData.Username,
                instanceData.Password);
            _sftpClient = new SftpClient(instanceData.Host, instanceData.Port, instanceData.Username,
                instanceData.Password);
        } else {
            _sshClient = new SshClient(instanceData.Host, instanceData.Port, instanceData.Username,
                new PrivateKeyFile(instanceData.Password));
            _sftpClient = new SftpClient(instanceData.Host, instanceData.Port, instanceData.Username,
                new PrivateKeyFile(instanceData.Password));
        }
        _instanceData = instanceData;
    }

    public void Connect()
    {
        _sshClient.Connect();
        _sftpClient.Connect();
    }

    public void Dispose()
    {
        _sftpClient.Disconnect();
        _sftpClient.Dispose();
        _sshClient.Disconnect();
        _sshClient.Dispose();
    }
    
    private static void ProgressBar(int progress, int tot)
    {
        //draw empty progress bar
        Console.CursorLeft = 0;
        Console.Write("["); //start
        Console.CursorLeft = 32;
        Console.Write("]"); //end
        Console.CursorLeft = 1;
        float onechunk = 30.0f / tot;

        //draw filled part
        int position = 1;
        for (int i = 0; i < onechunk * progress; i++)
        {
            Console.BackgroundColor = ConsoleColor.Green;
            Console.CursorLeft = position++;
            Console.Write(" ");
        }

        //draw unfilled part
        for (int i = position; i <= 31; i++)
        {
            Console.BackgroundColor = ConsoleColor.Gray;
            Console.CursorLeft = position++;
            Console.Write(" ");
        }

        //draw totals
        Console.CursorLeft = 35;
        Console.BackgroundColor = ConsoleColor.Black;
        Console.Write(progress.ToString() + " of " + tot.ToString() + "    "); //blanks at the end remove any excess
    }

    public bool Exists(string remotePath) => _sftpClient.Exists(remotePath);

    public void Pull(string remotePath, string localPath, bool showProgress)
    {
        if (!Exists(remotePath)) Program.Error(8, "Remote path does not exist");
        int totalSize = (int) _sftpClient.GetAttributes(remotePath).Size;
        Stream localFileStream = File.OpenWrite(localPath);
        if (showProgress) _sftpClient.DownloadFile(remotePath, localFileStream, obj => { ProgressBar((int) obj, totalSize); });
        else _sftpClient.DownloadFile(remotePath, localFileStream);
        localFileStream.Flush();
        localFileStream.Close();
        localFileStream.Dispose();
        if (showProgress)
        {
            Console.CursorLeft = 0;
            Console.Write("\n");
        }
        Console.WriteLine($"Pulled {remotePath} to {localPath}");
    }
    
    public void Push(string localPath, string remotePath, bool showProgress)
    {
        if (!File.Exists(localPath)) Program.Error(8, "Local path does not exist");
        int totalSize = (int) new FileInfo(localPath).Length;
        Stream localFileStream = File.OpenRead(localPath);
        if (showProgress) _sftpClient.UploadFile(localFileStream, remotePath, obj => { ProgressBar((int) obj, totalSize); });
        else _sftpClient.UploadFile(localFileStream, remotePath);
        localFileStream.Close();
        localFileStream.Dispose();
        if (showProgress)
        {
            Console.CursorLeft = 0;
            Console.Write("\n");
        }
        Console.WriteLine($"Pushed {localPath} to {remotePath}");
    }

}