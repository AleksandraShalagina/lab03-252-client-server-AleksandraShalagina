using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;

class Program
{
    static void Main(string[] args)
    {
        StartClient();
    }

    static void StartClient()
    {
        try
        {
            // Устанавливаем адрес и порт сервера
            IPAddress ipAddress = IPAddress.Parse("127.0.0.1");
            int port = 8888;

            while (true)
            {
                // Создаем сокет
                Socket clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                // Подключаемся к серверу
                clientSocket.Connect(new IPEndPoint(ipAddress, port));

                // Буфер для данных
                byte[] buffer = new byte[1024];

                string request = "";

                // Ввод пользователем действия
                Console.WriteLine("Enter action (PUT, GET, DELETE, exit):");
                string action = Console.ReadLine();

                if (action.ToLower() == "exit")
                {
                    // Отправляем команду "exit" на сервер и закрываем клиентский сокет
                    clientSocket.Send(Encoding.UTF8.GetBytes("exit"));
                    clientSocket.Shutdown(SocketShutdown.Both);
                    clientSocket.Close();
                    break;
                }

                // Создание запроса в зависимости от действия
                switch (action.ToUpper())
                {
                    case "PUT":

                        // Запрос пользователя о пути к файлу для загрузки
                        Console.WriteLine("Enter the file path with extention to upload:");
                        string fileName = Console.ReadLine();
                        string directoryPath = "C:\\Users\\aleks\\source\\Repos\\клиент2\\клиент2\\bin\\Debug\\data"; // Путь к папке, где находится файл
                        string filePath = Path.Combine(directoryPath, fileName);
                        // Проверка существования указанного файла
                        if (!File.Exists(filePath))
                        {
                            Console.WriteLine("File not found.");
                            return;
                        }

                        // Запрос имени файла для сохранения на сервере
                        Console.WriteLine("Enter the file name for the server without extention(press Enter to generate a unique name):");
                        string saveFileName2 = Console.ReadLine().Trim();

                        // Если имя файла не указано, генерируем уникальное имя
                        if (string.IsNullOrWhiteSpace(saveFileName2))
                        {
                            saveFileName2 = Guid.NewGuid().ToString();
                        }
                        FileInfo fileInfo = new FileInfo(filePath);
                        long fileSize = fileInfo.Length;

                        string savefileName2 = Path.GetFileName(saveFileName2);
                        string fileExtension = Path.GetExtension(filePath);
                        

                        using (FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                        {
                            // Формируем запрос в формате "PUT <fileName> <fileSize>"
                            string request4 = $"PUT {saveFileName2} {fileExtension} {fileSize}";
                            byte[] requestBuffer = Encoding.UTF8.GetBytes(request4);

                            // Отправляем запрос на сервер
                            clientSocket.Send(requestBuffer);

                            // Отправляем содержимое файла
                            int bytesRead;
                            while ((bytesRead = fileStream.Read(buffer, 0, buffer.Length)) > 0)
                            {
                                clientSocket.Send(buffer, 0, bytesRead, SocketFlags.None);
                            }
                        }
                        // Получение ответа от сервера
                        int bytesReceived4 = clientSocket.Receive(buffer);
                        string response4 = Encoding.UTF8.GetString(buffer, 0, bytesReceived4);
                        string[] responseParts4 = response4.Split(' ');
                        if (responseParts4[0] == "200")
                        {
                            Console.WriteLine($"File saved on the hard drive {responseParts4[1]}");
                        }
                        else if (responseParts4[0] == "403")
                        {
                            Console.WriteLine("This name is already used");
                        }
                        break;

                     

                    case "GET":
                        
                        Console.WriteLine("Enter GET method (BY_ID or BY_NAME):");
                        string getMethod = Console.ReadLine().ToUpper();
                        // Запрос пользователя о пути к файлу для загрузки
                        Console.WriteLine("Enter the file identifier or name:");
                        string fileIdentifierOrName = Console.ReadLine();

                        // Формируем запрос на сервер
                        string getRequest = $"GET {getMethod} {fileIdentifierOrName}";
                        clientSocket.Send(Encoding.UTF8.GetBytes(getRequest));

                        // Получение ответа от сервера
                        int bytesReceived = clientSocket.Receive(buffer);
                        string response = Encoding.UTF8.GetString(buffer, 0, bytesReceived);
                        string[] responseParts = response.Split(' ');

                        int statusCode = int.Parse(responseParts[0]);

                        if (statusCode == 200)
                        {
                            // Получаем имя файла и его расширение от сервера
                            string fileName5 = responseParts[1];
                            string fileExtension5 = responseParts[2];

                            // Получаем размер файла
                            long fileSize5 = long.Parse(responseParts[3]);

                            // Читаем данные файла из ответа сервера
                            byte[] fileData = new byte[fileSize5];
                            clientSocket.Receive(fileData);

                            // Спрашиваем пользователя, под каким именем сохранить файл
                            string saveFileName = GetUniqueFileName(fileName5, fileExtension5);

                            // Если пользователь не ввел имя файла, отменяем сохранение
                            if (string.IsNullOrWhiteSpace(saveFileName))
                            {
                                Console.WriteLine("Saving file cancelled.");
                                break;
                            }

                            // Сохраняем файл на клиенте
                            string saveFilePath = Path.Combine("data", saveFileName + fileExtension5);
                            File.WriteAllBytes(saveFilePath, fileData);

                            Console.WriteLine($"File saved to: {saveFilePath}");
                        }
                        else if (statusCode == 404)
                        {
                            Console.WriteLine("File not found on the server.");
                        }
                        else
                        {
                            Console.WriteLine("Error: " + response);
                        }

                        break;



                    case "DELETE":
                        Console.WriteLine("Enter DELETE method (BY_ID or BY_NAME):");
                        string deleteMethod = Console.ReadLine().ToUpper();
                        Console.WriteLine("Enter file identifier or name:");
                        string fileIdentifierOrNameDelete = Console.ReadLine();
                        request = $"DELETE {deleteMethod} {fileIdentifierOrNameDelete}";
                        ;
                        // Отправка запроса на сервер
                        clientSocket.Send(Encoding.UTF8.GetBytes(request));

                        // Получение ответа от сервера
                        int bytesReceived3 = clientSocket.Receive(buffer);
                        string response3 = Encoding.UTF8.GetString(buffer, 0, bytesReceived3);
                        Console.WriteLine("Response from server: " + response3);

                        break;
                    default:
                        Console.WriteLine("Invalid action.");
                        continue;
                }




                // Закрываем сокет
                clientSocket.Shutdown(SocketShutdown.Both);
                clientSocket.Close();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error: " + ex.Message);
        }
    }
    static string GetUniqueFileName(string fileName, string fileExtension)
    {
        Console.WriteLine("Enter the file name whitout extention to save (or press Enter to cancel):");
        string saveFileName = Console.ReadLine().Trim();

        // Если пользователь не ввел имя файла, отменяем сохранение
        if (string.IsNullOrWhiteSpace(saveFileName))
        {
            return null;
        }

        // Проверяем, существует ли файл с таким именем в папке downloads
        string saveFilePath = Path.Combine("downloads", saveFileName + fileExtension);
        if (File.Exists(saveFilePath))
        {
            Console.WriteLine("A file with the same name already exists. Please choose a different name.");
            return GetUniqueFileName(fileName, fileExtension);
        }

        return saveFileName;
    }
}
