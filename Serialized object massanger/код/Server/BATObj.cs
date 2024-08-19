using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.SymbolStore;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    public class BATObj
    {
        public string command = "";
        public string machineName = "";

        public BATObj()
        {
            machineName = Environment.MachineName;
        }
        public void CreateAndExecuteBatFile(string filePath)
        {
            try
            {
                // Создаем и записываем команду в BAT-файл
                using (StreamWriter writer = new StreamWriter(filePath))
                {
                    writer.WriteLine(this.command);
                }

                // Убедимся, что файл создан
                if (File.Exists(filePath))
                {
                    Console.WriteLine($"BAT file created at {filePath}");

                    // Запускаем BAT-файл
                    ProcessStartInfo processInfo = new ProcessStartInfo(filePath);
                    processInfo.CreateNoWindow = true;
                    processInfo.UseShellExecute = false;

                    using (Process process = Process.Start(processInfo))
                    {
                        process.WaitForExit();
                        Console.WriteLine("BAT file executed.");
                    }
                }
                else
                {
                    Console.WriteLine("Error: BAT file was not created.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred: " + ex.Message);
            }
        }
    }
}
