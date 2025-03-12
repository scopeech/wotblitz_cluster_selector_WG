using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;

class WotBlitzClusterSelector
{
    private Dictionary<string, string> servers = new Dictionary<string, string>()
    {
        { "EU_C0 (Нидерланды)", "login0.wotblitz.eu" },
        { "EU_C1 (Нидерланды)", "login1.wotblitz.eu" },
        { "EU_C2 (Германия)", "login2.wotblitz.eu" },
        { "EU_C3 (Польша)", "login3.wotblitz.eu" },
        { "EU_C4 (Казахстан)", "login4.wotblitz.eu" }
    };

    private string replacementIp = "0.0.0.0";
    private string hostsFilePath = @"C:\Windows\System32\drivers\etc\hosts";

    public void UpdateHostsFile(List<string> selectedServers)
    {
        if (!File.Exists(hostsFilePath))
        {
            Console.WriteLine($"Файл {hostsFilePath} не найден.");
            return;
        }

        var unselectedServers = servers.Keys.Except(selectedServers).ToList();
        var lines = ReadHostsFile();
        WriteHostsFile(lines, unselectedServers);

        Console.WriteLine("Сервера успешно обновлены!");
        System.Threading.Thread.Sleep(3000);
    }

    private List<string> ReadHostsFile()
    {
        return File.ReadAllLines(hostsFilePath).ToList();
    }

    private void WriteHostsFile(List<string> lines, List<string> unselectedServers)
    {
        using (StreamWriter writer = new StreamWriter(hostsFilePath))
        {
            foreach (string line in lines)
            {
                if (!servers.Values.Any(s => line.Contains(s)))
                {
                    writer.WriteLine(line);
                }
            }

            foreach (string server in unselectedServers)
            {
                writer.WriteLine($"{replacementIp} {servers[server]}");
            }
        }
    }

    public void ShowMenu()
    {
        Console.Clear();
        Console.WriteLine("Выберите серверы для разблокировки:");
        int index = 0; // Начать с 0
        foreach (var server in servers.Keys)
        {
            Console.WriteLine($"{index}. {server}");
            index++;
        }
        Console.WriteLine($"{index}. Разблокировать все сервера");
        Console.WriteLine($"{index + 1}. Тестировать серверы по задержке");
        Console.WriteLine($"{index + 2}. Открыть hosts с помощью блокнота");
        Console.WriteLine("Ваш выбор: (можно ввести сервера через запятую, чтобы разблокировать 2 сервера или более)");

        string input = Console.ReadLine();
        var choices = input.Split(',').Select(x => x.Trim()).ToList();

        if (choices.Contains((servers.Count).ToString())) // Разблокировать все
        {
            UpdateHostsFile(servers.Keys.ToList());
        }
        else if (choices.Contains((servers.Count + 1).ToString())) // Тест пинга
        {
            PingAllServers();
        }
        else if (choices.Contains((servers.Count + 2).ToString())) // Открыть hosts в Notepad
        {
            OpenHostsInNotepad();
        }
        else
        {
            List<string> selectedServers = new List<string>();
            foreach (var choice in choices)
            {
                if (int.TryParse(choice, out int num) && num >= 0 && num < servers.Count) // Индексы с 0
                {
                    selectedServers.Add(servers.Keys.ElementAt(num));
                }
            }

            UpdateHostsFile(selectedServers);
        }
    }

    private void PingAllServers()
    {
        Console.WriteLine("Тест пинга серверов...");
        foreach (var server in servers)
        {
            var pingResult = PingServer(server.Value);
            Console.WriteLine($"{server.Key}: {pingResult}");
        }
        Console.WriteLine("Ожидание 5 секунд.");
        System.Threading.Thread.Sleep(5000);
    }

    private string PingServer(string serverUrl)
    {
        try
        {
            Ping ping = new Ping();
            PingReply reply = ping.Send(serverUrl, 1000);

            if (reply.Status == IPStatus.Success)
            {
                return $"Доступен, {reply.RoundtripTime} мс";
            }
            else
            {
                return "Недоступен";
            }
        }
        catch (Exception)
        {
            return "Ошибка при пинге";
        }
    }

    // Новый метод для открытия файла hosts в Notepad с правами администратора
    private void OpenHostsInNotepad()
    {
        try
        {
            if (!IsAdministrator())
            {
                // Повторный запуск с правами администратора
                RunAsAdmin();
            }
            else
            {
                Process.Start("notepad.exe", hostsFilePath);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при открытии Notepad: {ex.Message}");
        }
    }

    // Метод проверки прав администратора
    private bool IsAdministrator()
    {
        var identity = System.Security.Principal.WindowsIdentity.GetCurrent();
        var principal = new System.Security.Principal.WindowsPrincipal(identity);
        return principal.IsInRole(System.Security.Principal.WindowsBuiltInRole.Administrator);
    }

    // Метод для перезапуска программы с правами администратора
    private void RunAsAdmin()
    {
        ProcessStartInfo psi = new ProcessStartInfo
        {
            FileName = Process.GetCurrentProcess().MainModule.FileName,
            UseShellExecute = true,
            Verb = "runas"
        };

        try
        {
            Process.Start(psi);
        }
        catch
        {
            Console.WriteLine("Запуск с правами администратора отменен.");
        }
    }
}

class Program
{
    static void Main()
    {
        if (!IsAdministrator())
        {
            RunAsAdmin();
            return;
        }

        WotBlitzClusterSelector manager = new WotBlitzClusterSelector();
        while (true)
        {
            manager.ShowMenu();
        }
    }

    static bool IsAdministrator()
    {
        var identity = System.Security.Principal.WindowsIdentity.GetCurrent();
        var principal = new System.Security.Principal.WindowsPrincipal(identity);
        return principal.IsInRole(System.Security.Principal.WindowsBuiltInRole.Administrator);
    }

    static void RunAsAdmin()
    {
        ProcessStartInfo psi = new ProcessStartInfo
        {
            FileName = Process.GetCurrentProcess().MainModule.FileName,
            UseShellExecute = true,
            Verb = "runas"
        };

        try
        {
            Process.Start(psi);
        }
        catch
        {
            Console.WriteLine("Запуск с правами администратора отменен.");
        }
    }
}
