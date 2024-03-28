using System;
using System.Net;
using System.Net.Sockets;
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

            // Создаем сокет
            Socket clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            // Подключаемся к серверу
            clientSocket.Connect(new IPEndPoint(ipAddress, port));

            // Буфер для данных
            byte[] buffer = new byte[1024];

            string request = "";

            // Ввод пользователем действия
            Console.WriteLine("Enter action (1 - get a file, 2 - create a file, 3 - delete a file, exit - stop the server):");
            string action = Console.ReadLine();

            if (action.ToLower() == "exit")
            {
                // Отправляем команду "exit" на сервер и закрываем клиентский сокет
                clientSocket.Send(Encoding.UTF8.GetBytes("exit"));
                clientSocket.Shutdown(SocketShutdown.Both);
                clientSocket.Close();
                return;
            }

            // Создание запроса в зависимости от действия
            switch (action)
            {
                case "1":
                    Console.WriteLine("Enter filename:");
                    string filenameGet = Console.ReadLine();
                    request = "GET " + filenameGet;
                    break;
                case "2":
                    Console.WriteLine("Enter filename:");
                    string filenamePut = Console.ReadLine();
                    Console.WriteLine("Enter file content:");
                    string fileContent = Console.ReadLine();
                    request = "PUT " + filenamePut + " " + fileContent;
                    break;
                case "3":
                    Console.WriteLine("Enter filename:");
                    string filenameDelete = Console.ReadLine();
                    request = "DELETE " + filenameDelete;
                    break;
                default:
                    Console.WriteLine("Invalid action.");
                    return;
            }

            // Отправка запроса на сервер
            clientSocket.Send(Encoding.UTF8.GetBytes(request));

            // Получение ответа от сервера
            int bytesReceived = clientSocket.Receive(buffer);  // Получаем ответ от сервера и сохраняем количество принятых байт
            string response = Encoding.UTF8.GetString(buffer, 0, bytesReceived); // Преобразуем полученный массив байтов с начала и до конца в строку

            // Обработка ответа
            string[] responseParts = response.Split(' ');
            int statusCode = int.Parse(responseParts[0]);

            switch (statusCode)
            {
                case 200:
                    if (action == "1")
                    {
                        string fileContentResponse = "";
                        for (int i = 1; i < responseParts.Length; i++)
                        {
                            fileContentResponse += responseParts[i] + " ";
                        }

                        Console.WriteLine("The content of the file is: " + fileContentResponse);
                    }
                    else if (action == "2")
                    {
                        Console.WriteLine("The response says that the file was created!");
                    }
                    else if (action == "3")
                    {
                        Console.WriteLine("The response says that the file was successfully deleted!");
                    }
                    break;
                case 404:
                    Console.WriteLine("The response says that the file was not found!");
                    break;
                case 403:
                    Console.WriteLine("The response says that creating the file was forbidden!");
                    break;

                default:
                    Console.WriteLine("Unknown response from server.");
                    break;
            }


            // Закрываем сокет
            clientSocket.Shutdown(SocketShutdown.Both);
            clientSocket.Close();
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error: " + ex.Message);
        }
    }
}
