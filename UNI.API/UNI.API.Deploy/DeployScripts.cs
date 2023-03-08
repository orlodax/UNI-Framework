using Renci.SshNet;

namespace UNI.API.Deploy;

public class DeployScripts
{
    public static void Deploy(ConfigInfo ci)
    {
        foreach (var host in ci.Hosts!)
        {
            Console.WriteLine($"Starting deploy on {host}");

            KeyboardInteractiveAuthenticationMethod kauth = new(ci.Username);
            PasswordAuthenticationMethod pauth = new(ci.Username, ci.Password);
            List<AuthenticationMethod> authMethods = new() { kauth, pauth };
            ConnectionInfo connectionInfo = new(host, ci.Username, authMethods.ToArray());

            // connect to hosting server
            using SshClient? client = new(connectionInfo);
            client.ErrorOccurred += Client_ErrorOccurred;

            client.Connect();

            // stop service
            Console.WriteLine($"Stopping service {ci.ServiceName}");

            using (var cmd = client.RunCommand($"echo \"{ci.Password}\" | sudo -S systemctl stop {ci.ServiceName}"))
            {
                if (cmd.ExitStatus == 0)
                    Console.WriteLine(cmd.Result);
                else
                    Console.WriteLine(cmd.Error);
            }

            Console.WriteLine();

            // delete old files
            Console.WriteLine("Replacing old files...");

            using (var cmd = client.RunCommand($"echo \"{ci.Password}\" | sudo -S rm -r {ci.DestinationPath}/*"))
            {
                if (cmd.ExitStatus == 0)
                    Console.WriteLine(cmd.Result);
                else
                    Console.WriteLine(cmd.Error);
            }

            Console.WriteLine();

            // copy new files
            SftpClient sftpClient = new(client.ConnectionInfo);
            sftpClient.Connect();

            UploadDirectory(sftpClient, ci.Source!, ci.DestinationPath!);

            sftpClient.Disconnect();

            Console.WriteLine();

            // restart service
            Console.WriteLine($"Restarting service {ci.ServiceName}");

            using (var cmd = client.RunCommand($"echo \"{ci.Password}\" | sudo -S systemctl start {ci.ServiceName}"))
            {
                if (cmd.ExitStatus == 0)
                    Console.WriteLine(cmd.Result);
                else
                    Console.WriteLine(cmd.Error);
            }

            client.Disconnect();

            Console.WriteLine($"Deploy completed on {host}.");
        }
    }

    private static void UploadDirectory(SftpClient client, string localPath, string remotePath)
    {
        Console.WriteLine("Uploading directory {0} to {1}", localPath, remotePath);

        IEnumerable<FileSystemInfo> infos = new DirectoryInfo(localPath).EnumerateFileSystemInfos();
        foreach (FileSystemInfo info in infos)
        {
            if (info.Attributes.HasFlag(FileAttributes.Directory))
            {
                string subPath = remotePath + "/" + info.Name;
                if (!client.Exists(subPath))
                    client.CreateDirectory(subPath);

                UploadDirectory(client, info.FullName, subPath + "/");
            }
            else
            {
                using FileStream fileStream = new(info.FullName, FileMode.Open);
                Console.WriteLine("Uploading {0} ({1:N0} bytes)", info.FullName, ((FileInfo)info).Length);

                client.UploadFile(fileStream, remotePath + "/" + info.Name);
            }
        }
    }

    private static void Client_ErrorOccurred(object? sender, Renci.SshNet.Common.ExceptionEventArgs e)
    {
        Console.WriteLine(e.Exception?.Message);
        Console.WriteLine(e.Exception?.InnerException?.Message);
        Console.ReadLine();
    }
}
