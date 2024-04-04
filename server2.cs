using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

class Program
{
    private const string FileMappingsPath = "file_mappings.txt";

    static void Main(string[] args)
    {
        StartServer();
    }

    static void StartServer()
    {
        try
        {
            Dictionary<string, string> fileMappings = LoadFileMappings();

            IPAddress ipAddress = IPAddress.Parse("127.0.0.1");
            int port = 8888;

            Socket serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            serverSocket.Bind(new IPEndPoint(ipAddress, port));
            serverSocket.Listen(10);

            Console.WriteLine("Server started!");

            while (true)
            {
                Socket clientSocket = serverSocket.Accept();
                Task.Run(() => HandleClient(clientSocket, fileMappings));
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error: " + ex.Message);
        }
    }

    static void HandleClient(Socket clientSocket, Dictionary<string, string> fileMappings)
    {
        try
        {
            byte[] buffer = new byte[1024];
            int bytesReceived = clientSocket.Receive(buffer);
            string request = Encoding.UTF8.GetString(buffer, 0, bytesReceived);

            string[] requestParts = request.Split(' ');
            string action = requestParts[0];
            string response = "";

            switch (action.ToLower())
            {
                case "put":
                    HandlePutRequest(clientSocket, requestParts, fileMappings, ref response);
                    break;
                case "get":
                    HandleGetRequest(clientSocket, requestParts, fileMappings, ref response);
                    break;
                case "delete":
                    HandleDeleteRequest(clientSocket, requestParts, fileMappings, ref response);
                    break;
                case "exit":
                    response = "Exiting server...";
                    break;
                default:
                    response = "400 Bad Request";
                    break;
            }

            clientSocket.Send(Encoding.UTF8.GetBytes(response));
            clientSocket.Shutdown(SocketShutdown.Both);
            clientSocket.Close();
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error handling client: " + ex.Message);
        }
    }

    static void HandlePutRequest(Socket clientSocket, string[] requestParts, Dictionary<string, string> fileMappings, ref string response)
    {
        bool tfFlag = false;
        string fileName = requestParts[1];
        foreach (string valName in fileMappings.Values)
        {
            if (valName == fileName)
            {
                response = "403 This name is already used";
                tfFlag = true;
                return;
            }
        }
        if (!tfFlag)
        {
            string fileId = Guid.NewGuid().ToString();
            string fileName4 = requestParts[1];
            string fileExtension = requestParts[2];
            string serverFilePath = Path.Combine("data", fileName4 + fileExtension);
            long fileSize = long.Parse(requestParts[3]);

            using (FileStream fileStream = new FileStream(serverFilePath, FileMode.Create, FileAccess.Write))
            {
                byte[] buffer4 = new byte[1024];
                long totalBytesReceived = 0;
                int bytesRead;
                while (totalBytesReceived < fileSize && (bytesRead = clientSocket.Receive(buffer4)) > 0)
                {
                    fileStream.Write(buffer4, 0, bytesRead);
                    totalBytesReceived += bytesRead;
                }
            }

            fileMappings[fileId] = fileName;
            SaveFileMappings(fileMappings);

            response = $"200 {fileId}";
        }
    }

    static void HandleGetRequest(Socket clientSocket, string[] requestParts, Dictionary<string, string> fileMappings, ref string response)
    {
        string getMethod = requestParts[1].ToLower();
        string fileIdentifierOrName = requestParts[2];

        string serverFilePath6 = "";

        if (getMethod == "by_id")
        {
            if (!fileMappings.ContainsKey(fileIdentifierOrName))
            {
                response = "404 Not Found";
                return;
            }
            string value = fileMappings.FirstOrDefault(x => x.Key == fileIdentifierOrName).Value;
            serverFilePath6 = Path.Combine("data", value);
        }
        else if (getMethod == "by_name")
        {
            if (!fileMappings.ContainsValue(fileIdentifierOrName))
            {
                response = "404 Not Found";
                return;
            }
            string value = fileMappings.FirstOrDefault(x => Path.GetFileName(x.Value) == Path.GetFileName(fileIdentifierOrName)).Value;
            serverFilePath6 = Path.Combine("data", value);
        }
        else
        {
            response = "400 Bad Request";
            return;
        }

        if (!(fileMappings.ContainsValue(fileIdentifierOrName) || fileMappings.ContainsKey(fileIdentifierOrName)))
        {
            response = "404 Not Found";
            return;
        }

        string extention = "";
        string[] fileNames = Directory.GetFiles("data");

        foreach (string fil in fileNames)
        {
            if (Path.GetFileNameWithoutExtension(fil) == Path.GetFileNameWithoutExtension(serverFilePath6))
            {
                extention = Path.GetExtension(fil);
            };
        }

        string fileName6 = Path.GetFileNameWithoutExtension(serverFilePath6);
        serverFilePath6 = Path.Combine("data", fileName6 + extention);
        long fileSize6 = new FileInfo(serverFilePath6).Length;
        byte[] fileData = File.ReadAllBytes(serverFilePath6);

        response = $"200 {fileName6} {extention} {fileSize6}";
        clientSocket.Send(Encoding.UTF8.GetBytes(response));
        clientSocket.Send(fileData);
    }

    static void HandleDeleteRequest(Socket clientSocket, string[] requestParts, Dictionary<string, string> fileMappings, ref string response)
    {
        string deleteFileBy = requestParts[1];
        string fileIdentifierOrNameDelete = requestParts[2];

        string serverFilePathDelete = "";
        if (deleteFileBy.ToLower() == "by_id")
        {
            if (!fileMappings.ContainsKey(fileIdentifierOrNameDelete))
            {
                response = "404 Not Found";
                return;
            }
            string value = fileMappings.FirstOrDefault(x => x.Key == fileIdentifierOrNameDelete).Value;
            serverFilePathDelete = Path.Combine("data", value);
        }
        else if (deleteFileBy.ToLower() == "by_name")
        {
            if (!fileMappings.ContainsValue(fileIdentifierOrNameDelete))
            {
                response = "404 Not Found";
                return;
            }
            string value = fileMappings.FirstOrDefault(x => x.Value == fileIdentifierOrNameDelete).Value;
            serverFilePathDelete = Path.Combine("data", value);
        }
        else
        {
            response = "400 Bad Request";
            return;
        }

        if (!(fileMappings.ContainsValue(fileIdentifierOrNameDelete) || fileMappings.ContainsKey(fileIdentifierOrNameDelete)))
        {
            response = "404 Not Found";
            return;
        }
        string extentionDelete = "";
        string[] fileName2 = Directory.GetFiles("data");

        foreach (string fil in fileName2)
        {
            if (Path.GetFileNameWithoutExtension(fil) == Path.GetFileNameWithoutExtension(serverFilePathDelete))
            {
                extentionDelete = Path.GetExtension(fil);
            };
        }
        serverFilePathDelete = serverFilePathDelete + extentionDelete;

        File.Delete(serverFilePathDelete);
        if (deleteFileBy.ToLower() == "by_name")
        {
            string key2 = fileMappings.FirstOrDefault(x => x.Value == fileIdentifierOrNameDelete).Key;
            if (key2 != null)
            {
                fileMappings.Remove(key2);
                SaveFileMappings(fileMappings);
            }
        }
        else if (deleteFileBy.ToLower() == "by_id")
        {
            string key2 = fileMappings.FirstOrDefault(x => x.Key == fileIdentifierOrNameDelete).Key;
            if (key2 != null)
            {
                fileMappings.Remove(key2);
                SaveFileMappings(fileMappings);
            }
        }

        response = "200 Deleted";
    }

    static Dictionary<string, string> LoadFileMappings()
    {
        Dictionary<string, string> fileMappings = new Dictionary<string, string>();

        if (File.Exists(FileMappingsPath))
        {
            string[] lines = File.ReadAllLines(FileMappingsPath);
            foreach (string line in lines)
            {
                string[] parts = line.Split(',');
                if (parts.Length == 2)
                {
                    fileMappings[parts[0]] = parts[1];
                }
            }
        }

        return fileMappings;
    }

    static void SaveFileMappings(Dictionary<string, string> fileMappings)
    {
        List<string> lines = new List<string>();
        foreach (var mapping in fileMappings)
        {
            lines.Add($"{mapping.Key},{mapping.Value}");
        }

        File.WriteAllLines(FileMappingsPath, lines);
    }
}


