using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Reflection.PortableExecutable;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Server
{
    internal class Controlling
    {
        internal TcpClient client;
        byte[] buffer;
        internal byte[] key;
        public Controlling(TcpClient client)
        {
            this.client = client;
            using (Aes aes = Aes.Create())
            {
                aes.GenerateKey();
                key = aes.Key;
            }
            client.GetStream().Write(key, 0, 8);
            key = key.Take(8).ToArray();
        }
        internal void Send(TcpClient client, List<Controlling> clients)
        {
            buffer = new byte[1024];
            client.GetStream().Read(buffer, 0, buffer.Length);
            string newObj = Decrypt(Encoding.UTF8.GetString(buffer), key).Trim('\0');//получаем новый объект для рассылки остальным клиентам
            if (newObj.Contains("#ERROR"))
            {
                Console.WriteLine($"Client {client.Client.RemoteEndPoint} tryes send object, but operation is falled");
            }
            try
            {
                foreach (var cl in clients)
                {
                    if (client == cl.client) { continue; }
                    try
                    {
                        string s = cl.Encrypt(newObj, cl.key);
                        buffer = new byte[1024];
                        buffer = Encoding.UTF8.GetBytes(s);
                        cl.client.GetStream().Write(buffer, 0, buffer.Length);
                    }
                    catch (ObjectDisposedException odex)
                    {
                        Console.WriteLine(odex.Message);
                        cl?.client?.Close();
                        //Console.ReadKey();
                    }
                    catch (ArgumentNullException anex)
                    {
                        Console.WriteLine(anex.Message);
                        cl?.client?.Close();
                        //Console.ReadKey();
                    }
                    catch (InvalidOperationException ioex)
                    {
                        Console.WriteLine(ioex.Message);
                        cl?.client?.Close();
                        //Console.ReadKey();
                    }
                    catch (IOException IOex)
                    {
                        Console.WriteLine(IOex.Message);
                        cl?.client?.Close();
                    }
                }
            }
            catch (ObjectDisposedException odex)
            {
                Console.WriteLine(odex.Message);
                Console.ReadKey();
            }
            catch (ArgumentNullException anex)
            {
                Console.WriteLine(anex.Message);
                Console.ReadKey();
            }
            catch (InvalidOperationException ioex)
            {
                Console.WriteLine(ioex.Message);
                Console.ReadKey();
            }
        }
        internal void GetFile(string file)
        {
            try
            {
                buffer = new byte[1024];
                try
                {
                    using (FileStream fileStream = File.OpenRead(file))
                    {
                        fileStream.Read(buffer, 0, buffer.Length);
                        string Object = string.Concat(Encoding.UTF8.GetString(buffer).Trim('\0'), "#GET");
                        string s = Encrypt(Object, key);
                        buffer = new byte[1024];
                        buffer = Encoding.UTF8.GetBytes(s);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    buffer = new byte[1024];
                    buffer = Encoding.UTF8.GetBytes(Encrypt("#ERROR#GET", key));
                    client.GetStream().Write(buffer, 0, buffer.Length);
                    return;
                }
                client.GetStream().Write(buffer, 0, buffer.Length);
                //fileStream.CopyTo(this.client.GetStream());
                Console.WriteLine($"Данные отправлены на клиент {client.Client.RemoteEndPoint}.");
            }
            catch (Exception ex)
            {
                Console.WriteLine(DateTime.Now.ToString() + ex.Message);
                Console.ReadKey();
            }
        }
     
        internal string GetFiles()
        {
            List<string> files = Directory.GetFiles(Environment.CurrentDirectory).Where(x => x.Contains(".xml")).ToList();
            foreach (var file in files)
            {
                buffer = files.Last() == file ? Encoding.UTF8.GetBytes(Encrypt(file + "#GET#END", key)) : Encoding.UTF8.GetBytes(Encrypt(file + "#GETFILE", key));
                client.GetStream().Write(buffer, 0, buffer.Length);
                Console.WriteLine($"file {file}");
                buffer = new byte[1024];
                if (files.Last() != file) { client.GetStream().Read(buffer, 0, buffer.Length); }
                if (Decrypt(Encoding.UTF8.GetString(buffer), key).Contains("#OK"))
                { }
            }
            buffer = new byte[1024];
            client.GetStream().Read(buffer, 0, buffer.Length);
            string f = Decrypt(Encoding.UTF8.GetString(buffer), key).Trim('\0');
            f = f.Substring(0, f.Length - 4);
            return f;
        }
        public string Encrypt(string data, byte[] key)
        {          
            string answer = "";
            byte[] privateKeyBytes = key;
            byte[] publicKeyBytes = { };
            publicKeyBytes = key;
            byte[] inputByteArray = System.Text.Encoding.UTF8.GetBytes(data);
            using (DESCryptoServiceProvider provider = new DESCryptoServiceProvider())
            {
                var memoryStream = new MemoryStream();
                var cryptoStream = new CryptoStream
                    (memoryStream, provider.CreateEncryptor(publicKeyBytes, privateKeyBytes), CryptoStreamMode.Write);
                cryptoStream.Write(inputByteArray, 0, inputByteArray.Length);
                cryptoStream.FlushFinalBlock();
                answer = Convert.ToBase64String(memoryStream.ToArray());
            }
            return answer;
        }
        public string Decrypt(string data, byte[] key)
        {
            data = data.Trim('\0');
            string answer = "";
            byte[] privateKeyBytes = { };
            privateKeyBytes = key;
            byte[] publicKeyBytes = { };
            publicKeyBytes = key;
            byte[] inputByteArray = new byte[data.Length];
            inputByteArray = Convert.FromBase64String(data);
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
