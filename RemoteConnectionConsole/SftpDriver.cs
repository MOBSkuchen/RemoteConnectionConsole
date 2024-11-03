using System.Globalization;
using Microsoft.VisualBasic;
using Renci.SshNet;
using Renci.SshNet.Common;
using Renci.SshNet.Sftp;

namespace RemoteConnectionConsole;

public class SftpDriver
{
    private readonly SftpClient _sftpClient;
    public InstanceData InstanceData;

    public SftpDriver(InstanceData instanceData)
    {
        if (!instanceData.IsKeyAuth)
        {
            _sftpClient = new SftpClient(instanceData.Host, instanceData.Port, instanceData.Username,
                instanceData.Password);
        } else {
            _sftpClient = new SftpClient(instanceData.Host, instanceData.Port, instanceData.Username,
                new PrivateKeyFile(instanceData.Password));
        }
        InstanceData = instanceData;
    }

    public void Connect()
    {
        _sftpClient.Connect();
        _sftpClient.ChangeDirectory(InstanceData.WorkingDirectory);
    }

    public void Dispose()
    {
        _sftpClient.Disconnect();
        _sftpClient.Dispose();
    }
    
    static string FormatSize(long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB", "TB", "PB", "EB" };
        double len = bytes;
        int order = 0;

        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len /= 1024;
        }

        return $"{len:0.##} {sizes[order]}";
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
            Console.CursorLeft = position++;
            Console.Write("=");
        }

        //draw unfilled part
        for (int i = position; i <= 31; i++)
        {
            Console.CursorLeft = position++;
            Console.Write(" ");
        }

        //draw totals
        Console.CursorLeft = 35;
        Console.Write($"{FormatSize(progress)} of {FormatSize(tot)} {(progress * 100 / tot)}%   "); //blanks at the end remove any excess
    }

    private bool Exists(string remotePath) => _sftpClient.Exists(remotePath);
    
    void DownloadDirectory(string sourceRemotePath, string destLocalPath, bool showProgress) {
        Directory.CreateDirectory(destLocalPath);
        IEnumerable<SftpFile> files = _sftpClient.ListDirectory(sourceRemotePath);
        foreach (SftpFile file in files)
        {
            if ((file.Name != ".") && (file.Name != ".."))
            {
                string sourceFilePath = sourceRemotePath + "/" + file.Name;
                string destFilePath = Path.Combine(destLocalPath, file.Name);
                Console.WriteLine($"Pulling {sourceFilePath} to {destFilePath}");
                if (file.IsDirectory)
                {
                    DownloadDirectory(sourceFilePath, destFilePath, showProgress);
                }
                else
                {
                    int totalSize = (int) _sftpClient.GetAttributes(sourceFilePath).Size;
                    using Stream fileStream = File.Create(destFilePath);
                    if (showProgress) _sftpClient.DownloadFile(sourceFilePath, fileStream,
                        obj => { ProgressBar((int) obj, totalSize); });
                    else _sftpClient.DownloadFile(sourceFilePath, fileStream);
                }
                Program.ClearCurrentConsoleLine();
                Console.CursorTop -= 1;
                Program.ClearCurrentConsoleLine();
            }
        }
    }

    public void Pull(string remotePath, string localPath, bool showProgress)
    {
        if (showProgress) Console.CursorVisible = false;
        if (!Exists(remotePath))
        {
            Program.Error(8, "Remote path does not exist");
            return;
        }
        
        if (File.Exists(localPath) || Directory.Exists(localPath))
        {
            Program.Error(9, "New local path already exists");
            return;
        }

        if (IsDir(remotePath))
        {
            DownloadDirectory(remotePath, localPath, showProgress);
        }
        else
        {
            int totalSize = (int) _sftpClient.GetAttributes(remotePath).Size;
            Stream localFileStream = File.OpenWrite(localPath);
            if (showProgress) _sftpClient.DownloadFile(remotePath, localFileStream, obj => { ProgressBar((int) obj, totalSize); });
            else _sftpClient.DownloadFile(remotePath, localFileStream);
            localFileStream.Flush();
            localFileStream.Close();
            localFileStream.Dispose();
        }
        Program.ClearCurrentConsoleLine();
        Console.WriteLine($"Pulled {remotePath} to {localPath}");
        if (showProgress) Console.CursorVisible = true;
    }
    
    void UploadFile(string localPath, string remotePath, bool showProgress)
    {
        if (showProgress) Console.CursorVisible = false;
        if (Exists(remotePath))
        {
            Console.WriteLine($"Skipping {localPath}, because it already exists");
            return;
        }
        remotePath = remotePath.Replace('\\', '/');
        localPath = localPath.Replace('\\', '/');
        Console.WriteLine($"Uploading {localPath} to {remotePath}");
        int totalSize = (int) new FileInfo(localPath).Length;
        Stream localFileStream = File.OpenRead(localPath);
        if (showProgress)
            _sftpClient.UploadFile(localFileStream, remotePath, obj => { ProgressBar((int) obj, totalSize); });
        else 
            _sftpClient.UploadFile(localFileStream, remotePath);
        localFileStream.Close();
        localFileStream.Dispose();
        Program.ClearCurrentConsoleLine();
        Console.CursorTop -= 1;
        Program.ClearCurrentConsoleLine();
        if (showProgress) Console.CursorVisible = true;
    }
    
    void UploadDir(string localPath, string remotePath, bool showProgress)
    {
        if (Exists(remotePath))
        {
            Console.WriteLine($"Skipping {localPath}, because it already exists");
            return;
        }
        _sftpClient.CreateDirectory(remotePath);
        remotePath = remotePath.Replace('\\', '/');
        localPath = localPath.Replace('\\', '/');
        
        Console.WriteLine($"Uploading {localPath} to {remotePath}");
        
        foreach (var dirEntry in Directory.GetDirectories(localPath))
        {
            UploadDir(dirEntry.Replace('\\', '/'), dirEntry.Replace('\\', '/'), showProgress);
        }
        
        foreach (var dirEntry in Directory.GetFiles(localPath))
        {
            UploadFile(dirEntry.Replace('\\', '/'), Path.Combine(remotePath, Path.GetRelativePath(localPath, dirEntry.Replace('\\', '/'))), showProgress);
        }
        
        Program.ClearCurrentConsoleLine();
        Console.CursorTop -= 1;
        Program.ClearCurrentConsoleLine();
    }
    
    public void Push(string localPath, string remotePath, bool showProgress)
    {
        if (showProgress) Console.CursorVisible = false;
        if (!File.Exists(localPath) && !Directory.Exists(localPath))
        {
            Program.Error(8, "Local path does not exist");
            return;
        }

        remotePath = Path.Combine(_sftpClient.WorkingDirectory, remotePath);
        remotePath = remotePath.Replace('\\', '/');

        if (File.GetAttributes(localPath).HasFlag(FileAttributes.Directory)) UploadDir(localPath, remotePath, showProgress);
        else UploadFile(localPath, remotePath, showProgress);
        if (showProgress) Program.ClearCurrentConsoleLine();
        Console.WriteLine($"Pushed {localPath} to {remotePath}");
        if (showProgress) Console.CursorVisible = true;
    }

    void CopyFile(string oldPath, string newPath, bool showProgress)
    {
        var reader = _sftpClient.OpenRead(oldPath);
        var writer = _sftpClient.OpenWrite(newPath);
        var target = reader.Length;
        var completed = 0;
        while (target > completed)
        {
            byte[] buffer = new byte[1024];
            var read = reader.Read(buffer);
            completed += read;
            writer.Write(buffer, 0, read);
            if (showProgress) ProgressBar(completed, (int) target);
        }
    }
    
    void CopyDirectory(string newpath, string oldPath, bool showProgress)
    {
        string newPath;
        string oldP;
        _sftpClient.CreateDirectory(newpath);
        foreach (SftpFile file in _sftpClient.ListDirectory(oldPath))
        {
            newPath = Path.Join(newpath, file.Name).Replace('\\', '/');
            oldP = Path.Join(oldPath, file.Name).Replace('\\', '/');
            Console.WriteLine($"Copying {oldP} to {newPath}");
            if (file.Name != "." && file.Name != "..")
            {
                if (file.IsDirectory)
                {
                    _sftpClient.CreateDirectory(newPath);
                    CopyDirectory(newPath, oldP, showProgress);
                }
                else
                {
                    CopyFile(oldP, newPath, showProgress);
                }
            }
            if (showProgress)
            {
                Program.ClearCurrentConsoleLine();
                Console.CursorTop -= 1;
            }
            Program.ClearCurrentConsoleLine();
        }
    }

    public void Move(string oldPath, string newPath, bool copy, bool showProgress)
    {
        if (showProgress) Console.CursorVisible = false;
        newPath = Path.Combine(_sftpClient.WorkingDirectory, newPath);
        newPath = newPath.Replace('\\', '/');
        
        if (!Exists(oldPath))
        {
            Program.Error(8, "Remote path does not exist");
            return;
        }
        if (Exists(newPath))
        {
            Program.Error(9, "New remote path already exists");
            return;
        }
        if (!copy)
        {
            _sftpClient.RenameFile(oldPath, newPath);
            Console.WriteLine($"Moved {oldPath} to {newPath}");
        }
        else
        {
            Console.WriteLine($"Copying {oldPath} to {newPath}");
            if (!IsDir(oldPath))
            {
                CopyFile(oldPath, newPath, showProgress);
                if (showProgress) Program.ClearCurrentConsoleLine();
            }
            else CopyDirectory(newPath, Path.Join(_sftpClient.WorkingDirectory, oldPath).Replace('\\', '/'), showProgress);
            Program.ClearCurrentConsoleLine();
            Console.WriteLine($"Copied {oldPath} to {newPath}");
        }
        if (showProgress) Console.CursorVisible = true;
    }

    public void List()
    {
        Console.WriteLine($"Listing {_sftpClient.WorkingDirectory}:");
        int fileCounter = 0;
        int directoryCounter = 0;
        int totalCounter = 0;
        long totalSize = 0;
        foreach (SftpFile sftpFile in _sftpClient.ListDirectory("."))
        {
            if (sftpFile.Name == "." || sftpFile.Name == "..") continue;
            var gs = GetSize(sftpFile.FullName);
            totalSize += gs.Item1;
            totalCounter += gs.Item2;
            if (sftpFile.IsDirectory)
            {
                directoryCounter++;
                Console.WriteLine($" / {sftpFile.LastAccessTime.ToString(CultureInfo.CurrentCulture)} {sftpFile.Name} {FormatSize(gs.Item1)}");
            }
            else if (sftpFile.IsRegularFile)
            {
                fileCounter++;
                Console.WriteLine($" - {sftpFile.LastAccessTime.ToString(CultureInfo.CurrentCulture)} {sftpFile.Name} {FormatSize(gs.Item1)}");
            }
        }
        Console.WriteLine($"{fileCounter} files & {directoryCounter} directories ({fileCounter + directoryCounter} in total / " +
                          $"{totalCounter} including subdirectories). {FormatSize(totalSize)}");
    }

    bool IsDir(string remotePath)
    {
        return _sftpClient.GetAttributes(remotePath).IsDirectory;
    }

    (long, int) GetSize(SftpFile file)
    {
        long size = 0;
        int searched = 1;
        try
        {
            if (file.IsDirectory) {
                foreach (SftpFile sftpFile in _sftpClient.ListDirectory(file.FullName))
                {
                    if (sftpFile.Name == "." || sftpFile.Name == "..") continue;
                    var gs = GetSize(sftpFile);
                    size += gs.Item1;
                    searched += gs.Item2;
                }
            }
        } catch (SftpPathNotFoundException exception) {}
        catch (SftpPermissionDeniedException exception) {}
        size += file.Attributes.Size;
        return (size, searched);
    }

    (long, int) GetSize(string remotePath) => GetSize(_sftpClient.Get(remotePath));
    
    void DeleteDirectory(string path)
    {
        foreach (SftpFile file in _sftpClient.ListDirectory(path))
        {
            if ((file.Name != ".") && (file.Name != ".."))
            {
                if (file.IsDirectory)
                {
                    DeleteDirectory(file.FullName);
                }
                else
                {
                    _sftpClient.DeleteFile(file.FullName);
                }
            }
        }

        _sftpClient.DeleteDirectory(path);
    }

    public void Delete(string remotePath)
    {
        remotePath = Path.Combine(_sftpClient.WorkingDirectory, remotePath);
        remotePath = remotePath.Replace('\\', '/');
        if (!Exists(remotePath))
        {
            Program.Error(8, "Remote path does not exist");
            return;
        }
        bool isDir = IsDir(remotePath);
        var gs = GetSize(remotePath);
        while (true)
        {
            if (isDir) Console.Write($"Do you want to delete {remotePath} (=> {gs.Item2}, {FormatSize(gs.Item1)})? [Y/N] ");
            else Console.Write($"Do you want to delete {remotePath} (=> {FormatSize(gs.Item1)})? [Y/N] ");
            var key = Console.ReadKey(true);
            Program.ClearCurrentConsoleLine();
            if (key.KeyChar == 'Y' || key.KeyChar == 'y') break;
            if (key.KeyChar == 'N' || key.KeyChar == 'n')
            {
                Console.WriteLine("Aborted");
                return;
            }
        }
        if (isDir) DeleteDirectory(remotePath);
        else _sftpClient.Delete(remotePath);
        Console.WriteLine($"Deleted {remotePath}");
    }

    public string? ChangeDirectory(string path)
    {
        if (!Exists(path) || !_sftpClient.GetAttributes(path).IsDirectory)
        {
            Program.Error(8, "Remote path does not exist or is not a directory");
            return null;
        }
        _sftpClient.ChangeDirectory(path);
        return _sftpClient.WorkingDirectory;
    }

}