using System.Net.Sockets;
using System.Net;
using System.Xml.Serialization;
using System.IO.Pipes;
using System.Text;
using System.Net.NetworkInformation;

namespace Server
{
    internal class Program
    {
        static List<Controlling> clients = new List<Controlling>();
        static void Main(string[] args)
        {
            TcpListener server = null;
            try
            {
                string command = "";
                while (true)
                {
                    Console.WriteLine("Enter a command text:");
                    command = Console.ReadLine();


                    if (command != "")
                    {
                        break;
                    }
                    else
                    {
                        Console.Clear();
                        Console.WriteLine("Entered parametrs are invalid");
                    }
                }
                BATObj server1 = new BATObj();
                server1.command = "echo " + command;
                XmlSerializer xml = new XmlSerializer(typeof(BATObj));
                using (FileStream fs = new FileStream("data.xml", FileMode.Create))
                {
                    xml.Serialize(fs, server1);
                }
                bool isCorrect = false;

                while (!isCorrect)
                {
                    try
                    {
                        Console.Write("Enter server port: ");
                        int port = int.Parse(Console.ReadLine());
                        string myip = "192.168.56.1";// "192.168.213.183"; //getIP();
                        IPAddress localAddr = IPAddress.Parse(myip);
                        Console.WriteLine(myip);
                        server = new TcpListener(localAddr, port);
                        isCorrect = true;
                    }
                    catch
                    {
                        Console.WriteLine("Enter parametr are not valid. Try again.");
                    }
                }


                // Начинаем прослушивать входящие соединения
                server.Start();

                // Принимаем клиента
                Console.WriteLine("Сервер запущен...");
                while (true)
                {
                    try
                    {
                        TcpClient client = server.AcceptTcpClient();
                        Task.Run(() => connect(ref server, client));
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                        return;
                    }
                }
            }
            catch (SocketException e)
            {
                Console.WriteLine("SocketException: {0}", e);
            }
            finally
            {
                // Останавливаем сервер
                server.Stop();
            }
        }
        public static void connect(ref TcpListener server, TcpClient client)
        {
            Controlling ethernet = new Controlling(client);
            clients.Add(ethernet);
            Console.WriteLine($"Клиент {client.Client.RemoteEndPoint} подключен!");
            while (true)
            {
                try
                {

                    // Получаем сетевой поток для чтения и записи
                    {
                        byte[] buf = new byte[1024];
                        client.GetStream().Read(buf, 0, buf.Length);
                        string type = ethernet.Decrypt(Encoding.UTF8.GetString(buf.ToArray()), ethernet.key);
                        // Отправляем файл клиенту
                        if (type.Contains("#GET"))
                        {
                            try
                            {
                                string file = ethernet.GetFiles();
                                ethernet.GetFile(file);
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(ex.Message);

                                clients.Remove(ethernet);
                                server.Stop();
                                return;
                            }

                        }
                        else if (type.Contains("#SEND"))
                        {
                            try
                            {
                                ethernet.Send(client, clients);

                            }
                            catch (OutOfMemoryException ex)
                            {
                                Console.WriteLine(ex.Message);

                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(ex.Message);
                            }
                        }

                    }
                }

                catch (SocketException e)
                {
                    Console.WriteLine("SocketException: {0}", e);
                    clients.Remove(ethernet);
                    client!.Close();
                    return;
                }
                catch (ObjectDisposedException odex)
                {
                    Console.WriteLine(odex.Message);
                    clients.Remove(ethernet);
                    client!.Close();
                    return;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("пумпумпум... " + ex.Message);
                    clients.Remove(ethernet);
                    client!.Close();
                    return;
                }
            }

        }

        public static string getIP()
        {
            NetworkInterface[] networkInterfaces = NetworkInterface.GetAllNetworkInterfaces();

            foreach (NetworkInterface networkInterface in networkInterfaces)
            {
                // Проверяем, что сетевой интерфейс работает и это беспроводной интерфейс
                if (networkInterface.OperationalStatus == OperationalStatus.Up &&
                    (networkInterface.NetworkInterfaceType == NetworkInterfaceType.Wireless80211 ||
                     networkInterface.NetworkInterfaceType == NetworkInterfaceType.Ethernet))
                {
                    Console.WriteLine($"Сетевой интерфейс: {networkInterface.Name}");

                    // Получаем свойства IP-адреса
                    IPInterfaceProperties ipProperties = networkInterface.GetIPProperties();

                    foreach (UnicastIPAddressInformation ip in ipProperties.UnicastAddresses)
                    {
                        // Фильтруем только IPv4 адреса
                        if (ip.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                        {
                            return ip.Address.ToString();
                        }
                    }
                }
            }
            return "Invalid not found";
        }
    }
}
