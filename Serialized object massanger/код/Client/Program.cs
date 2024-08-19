
using System.ComponentModel.Design;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Xml.Serialization;

namespace Client
{
    internal class Program
    {
        static TcpClient client = null;
        static int Main(string[] args)
        {
            while (true)
            {
                int port=0;
                IPAddress ip = null;


                try
                {
                    bool isCorrect = false;
                    while (!isCorrect)
                    {
                        try
                        {
                            Console.Write("Enter server IP: ");
                            ip = IPAddress.Parse(Console.ReadLine());

                            Console.Write("Enter port: ");
                            port = int.Parse(Console.ReadLine());

                  
                            client = new TcpClient(ip.ToString(), port);
                            isCorrect = true;
                        }
                        catch
                        {
                            Console.WriteLine("Enter parametr are not valid. Try again.");
                            continue;
                        }
                    }
 
                    Menu.client = client;
                    Menu.key = new byte[8]; // 32 байта для 256-битного ключа
                    client.GetStream().Read(Menu.key, 0, 8);
                    while (true)
                    {
                        Task.Run(() => ReceiveMessagesAsync());//прослушка сообщений

                        Console.WriteLine(@"Choose the action:
1. Get bat object;
2. Send your own object;
3. Close the app.");
                        string key = Console.ReadLine();
                        if (key == "1")
                        {
                            try
                            {
                                string file = Menu.GetFiles();
                                Menu.Get(file);
                            }
                            catch (IOException ioex)
                            {
                                Console.WriteLine(ioex.Message);
                                Console.WriteLine("Client'll try to reconnect.");
                                Thread.Sleep(5000);
                                break;
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(DateTime.Now.ToString() + ex.Message);
                                client.Close();
                                Console.ReadKey();
                                return -2;
                            }
                        }

                        else if (key == "2")
                        {
                            try
                            {
                                Menu.Send();
                            }
                            catch (IOException ioex)
                            {
                                Console.WriteLine(ioex.Message);
                                Console.WriteLine("Client'll try to reconnect.");
                                Thread.Sleep(5000);
                                break;
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(DateTime.Now.ToString() + ex.Message);
                                //client.Close();
                                Console.ReadKey();
                                //return -2;
                            }

                        }
                        else if (key == "3")
                        {
                            Menu.client.Dispose();
                            return 0;
                        }
                        else
                        {
                            Console.WriteLine("Incorrect choice. Try again.");
                        }
                    }

                }
                catch (Exception e)
                {
                    Console.WriteLine("Exception: {0}", e);
                    Thread.Sleep(5000);
                    continue;
                }
            }
        }
        //ПРИЕМКА
        public static void ReceiveMessagesAsync()
        {
            try
            {
                while (true)
                {
                    byte[] bytes = new byte[1024];
                    // Асинхронно читаем сообщение от сервера
                    client.GetStream().Read(bytes, 0, bytes.Length);
                    if(bytes.Where(x=>x!=(byte)0)?.Count()==0)
                    {
                        continue;
                    }
                    string receivedMessage = Menu.Decrypt(Encoding.UTF8.GetString(bytes),Menu.key).Trim('\0');
                    if (receivedMessage.Contains("#SEND"))
                    {
                        Console.WriteLine("Received from server!");
                        receivedMessage = receivedMessage.Substring(0, receivedMessage.Length - 5);
                        // Обработка принятого сообщения
                        BATObj bATObj;
                        XmlSerializer xml = new XmlSerializer(typeof(BATObj));
                        using (FileStream fileStream = File.Create("received_data.xml"))
                        {

                            using (StreamWriter fsr = new StreamWriter(fileStream))
                            {
                                fsr.Write(receivedMessage);
                            }

                        }
                     
                    }
                    else if (receivedMessage.Contains("#GET"))
                    {
                        if (receivedMessage.Contains("#GETFILE"))
                        {
                            byte[] buffer = Encoding.UTF8.GetBytes(Menu.Encrypt("#OK",Menu.key));
                            receivedMessage = receivedMessage.Substring(0, receivedMessage.Length - 8);
                            client.GetStream().Write(buffer,0,buffer.Length);
                        }
                        else if (receivedMessage.Contains("#GET#END"))
                        {
                            //byte[] buffer = Encoding.UTF8.GetBytes("#OK");
                            //client.GetStream().Write(buffer, 0, buffer.Length);

                        }
                        else
                        {
                            receivedMessage = receivedMessage.Substring(0, receivedMessage.Length - 4);
                        }
                        Menu.recieved = receivedMessage;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error receiving message from server: " + ex.Message);
            }
            finally
            {
                // Здесь можно выполнить необходимые действия при завершении операции приема сообщений
            }
        }
    }
}
