using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Reflection.PortableExecutable;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.Marshalling;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Client
{
    internal static class Menu
    {
        internal static TcpClient client;
        private static byte[] buffer = new byte[1024];
        internal static string recieved = "";

        internal static byte[] key = new byte[32];
        internal static void Get(string file)
        {
            buffer = new byte[1024];
            buffer = Encoding.UTF8.GetBytes(Encrypt(file + "#GET", key));
            client.GetStream().Write(buffer, 0, buffer.Length);

            buffer = new byte[1024];

            string r = "";
            while (true)
            {
                if (recieved != "")
                {
                    r = recieved;
                    break;
                }
            }
            recieved = "";
            if (r.Contains("#ERROR"))
            {
                Console.WriteLine("Object is not gotten.");
                return;
            }
            // Получаем сетевой поток для чтения данных от сервера
            // Создаем файл для сохранения данных

            using (FileStream fileStream = File.Create("received_data.xml"))
            using (StreamWriter fsr = new StreamWriter(fileStream))
            {
                // Читаем данные из потока и записываем в файл
                fsr.Write(r);
                fsr.Flush();
                Console.WriteLine("Данные успешно получены и сохранены в файл 'received_data.xml'");
            }

            XmlSerializer xml = new XmlSerializer(typeof(BATObj));
            using (FileStream fs = new FileStream("received_data.xml", FileMode.Open))
            {
                try
                {
                    BATObj sc = (BATObj)xml.Deserialize(fs);
                    sc.CreateAndExecuteBatFile("example.bat");
                    fs.Flush();
                }catch(Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
              
            }

        }
        internal static string GetFiles()
        {
            buffer = new byte[1024];
            buffer = Encoding.UTF8.GetBytes(Encrypt("#GET", key));
            client.GetStream().Write(buffer, 0, buffer.Length);
            List<string> r = new List<string>();
            while (true)
            {
                if (recieved != "")
                {
                    if (recieved.Contains("#GET#END"))
                    {
                        recieved = recieved.Substring(0, recieved.Length - 8);
                        r.Add(recieved);
                        recieved = "";
                        break;
                    }
                    else
                    {
                        r.Add(recieved);
                        recieved = "";
                    }
                }
            }
            while (true)
            {
                Console.WriteLine("Choose one object:");
                for (int i = 0; i < r.Count; i++)
                {
                    Console.WriteLine($"{i + 1}: {r[i]}");
                }
                string index = Console.ReadLine();
                int a = 0;
                if (int.TryParse(index, out a) && a - 1 >= 0 && a - 1 < r.Count)
                {
                    return r[a - 1];
                }
                Console.WriteLine("Incorrect choice. Try again");
            }
        }
        internal static void Send()
        {
            buffer = new byte[1024];
            buffer = Encoding.UTF8.GetBytes(Encrypt("#SEND", key));
            client.GetStream().Write(buffer, 0, buffer.Length);
            try
            {
                buffer = new byte[1024];
                XmlSerializer xml = new XmlSerializer(typeof(BATObj));
                BATObj sc;
                using (FileStream fs = new FileStream("received_data.xml", FileMode.Open))
                {
                    sc = (BATObj)xml.Deserialize(fs);
                    Console.WriteLine("Current object: ");
                    sc.command = "echo "+sc.machineName;
                    fs.Flush();
                  
                }
                using (FileStream fs = new FileStream("received_data.xml", FileMode.Create))
                {
                    xml.Serialize(fs, sc);
                }
                string text = string.Concat(File.ReadAllText("received_data.xml", Encoding.UTF8).Trim('\0'), "#SEND");
                buffer = new byte[1024];
                buffer = Encoding.UTF8.GetBytes(Encrypt(text, key));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                buffer = new byte[1024];
                buffer = Encoding.UTF8.GetBytes(Encrypt("#ERROR", key));
                client.GetStream().Write(buffer, 0, buffer.Length);
                return;
            }
            client.GetStream().Write(buffer, 0, buffer.Length);

        }
        public static string Encrypt(string data, byte[] key)
        {
            //string data = "GeeksForGeeks Text";
            string answer = "";
            byte[] privateKeyBytes = key;
            byte[] publicKeyBytes = { };
            publicKeyBytes = key;
            byte[] inputByteArray = System.Text.Encoding.UTF8.GetBytes(data);
            using (DESCryptoServiceProvider provider = new DESCryptoServiceProvider())
            {
                var memoryStream = new MemoryStream();
                var cryptoStream = new CryptoStream
                    (memoryStream,provider.CreateEncryptor(publicKeyBytes, privateKeyBytes),CryptoStreamMode.Write);
                cryptoStream.Write(inputByteArray, 0, inputByteArray.Length);
                cryptoStream.FlushFinalBlock();
                answer = Convert.ToBase64String(memoryStream.ToArray());
            }
            return answer;
        }
        public static string Decrypt(string data, byte[] key)
        {
            data = data.Trim('\0');
            string answer = "";
            byte[] privateKeyBytes = { };
            privateKeyBytes = key;
            byte[] publicKeyBytes = { };
            publicKeyBytes = key;
            byte[] inputByteArray = new byte[data.Replace(" ", "+").Length];
            inputByteArray = Convert.FromBase64String(data.Replace(" ", "+"));
            using (DESCryptoServiceProvider provider = new DESCryptoServiceProvider())
            {
                var memoryStream = new MemoryStream();
                var cryptoStream = new CryptoStream(memoryStream,
                provider.CreateDecryptor(publicKeyBytes, privateKeyBytes),
                CryptoStreamMode.Write);
                cryptoStream.Write(inputByteArray, 0, inputByteArray.Length);
                cryptoStream.FlushFinalBlock();
                answer = Encoding.UTF8.GetString(memoryStream.ToArray());
            }
            return answer;
        }


        
    }
}
