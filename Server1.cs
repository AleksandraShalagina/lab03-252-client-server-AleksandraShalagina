using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;

class Program
{
    static void Main(string[] args)
    {
        StartServer();
    }

    static void StartServer()
    {
        try
        {
            // Устанавливаем адрес и порт сервера
            IPAddress ipAddress = IPAddress.Parse("127.0.0.1");
            int port = 8888;

            // Создаем сокет
            Socket serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            // Привязываем сокет к адресу и порту
            serverSocket.Bind(new IPEndPoint(ipAddress, port));

            // Начинаем прослушивание
            serverSocket.Listen(10); // Начинаем прослушивание клиентских подключений с максимальной длиной очереди ожидания 10

            Console.WriteLine("Server started!");

            while (true)
            {
                // Принимаем клиентский сокет
                Socket clientSocket = serverSocket.Accept();

                // Буфер для данных
                byte[] buffer = new byte[1024];

                // Получаем данные от клиента
                int bytesReceived = clientSocket.Receive(buffer);
                string request = Encoding.UTF8.GetString(buffer, 0, bytesReceived);

                // Обработка запроса
                if (request.ToLower() == "exit")
                {
                    // Если получена команда "exit", завершаем работу сервера
                    Console.WriteLine("Exiting server...");
                    break;
                }

                string[] requestParts = request.Split(' ');
                string action = requestParts[0];
                string filename = requestParts[1];
                string response = "";

                switch (action)
                {
                    case "GET":
                        if (!File.Exists($"data/{filename}"))
                        {
                            response = "404"; // Файл не существует, поэтому отправляем код 404
                        }
                        else
                        {
                            string fileContent = File.ReadAllText($"data/{filename}");
                            response = $"200 {fileContent}";
                        }
                        break;
                    case "PUT":

                        if (File.Exists($"data/{filename}"))
                        {
                            response = "403"; // Файл существует, поэтому отправляем код 403
                        }
                        else
                        {
                            // Считываем содержимое файла из запроса (начиная со второго элемента requestParts)
                            string fileContent = "";
                            for (int i = 2; i < requestParts.Length; i++)
                            {
                                fileContent += requestParts[i] + " ";
                            }

                            // Создаем или перезаписываем файл с указанным содержимым
                            File.WriteAllText($"data/{filename}", fileContent);
                            response = "200"; // Отправляем код 200, чтобы сообщить об успешном создании файла
                        }

                        break;



                    case "DELETE":
                        
                        if (!File.Exists($"data/{filename}"))
                        {
                            response = "404"; // Файл не существует, поэтому отправляем код 404
                        }
                        else
                        {
                            File.Delete($"data/{filename}");
                            response = "200";
                        }

                        break;
                    default:
                        response = "400";
                        break;
                }

                // Отправляем ответ клиенту
                clientSocket.Send(Encoding.UTF8.GetBytes(response));

                // Закрываем соединение с клиентом
                clientSocket.Shutdown(SocketShutdown.Both);
                clientSocket.Close();
            }

            // Закрываем основной сокет
            serverSocket.Close();
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error: " + ex.Message);
        }
    }
}
