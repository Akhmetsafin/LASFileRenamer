using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace LASFileRenamer
{
    internal class Program
    {
        static void Main(string[] args)
        {


            Console.WriteLine("Введите путь к папке с LAS-файлами:");

            string folderPath = (Console.ReadLine() ?? "").Trim().Trim('"');

            if (Directory.Exists(folderPath))
            {
                FileInfo latestFile = new DirectoryInfo(folderPath)
                    .EnumerateFiles()
                    .Where(file => file.Extension.Equals(
                        ".las", StringComparison.OrdinalIgnoreCase))
                    .OrderByDescending(file => file.CreationTime)
                    .FirstOrDefault();

                if (latestFile != null)
                {
                    Console.WriteLine($"Найден файл: {latestFile.Name}");
                    Console.WriteLine($"Изменён: {latestFile.LastWriteTime}");
                    string stopDepth = "";

                    foreach (string line in File.ReadLines(latestFile.FullName))
                    {
                        Match match = Regex.Match(
                            line,
                            @"^\s*STOP\s*\.\s*M\s+([+-]?\d+(?:[.,]\d+)?)\s*:",
                            RegexOptions.IgnoreCase);

                        if (match.Success)
                        {
                            stopDepth = match.Groups[1].Value.Replace('.', ',');
                            break;
                        }
                    }

                    if (stopDepth != "")
                    {
                        Console.WriteLine($"Конечная глубина из LAS: {stopDepth} м");
                    }
                    else
                    {
                        Console.WriteLine("Конечная глубина STOP не найдена.");
                    }
                    /*
                    Console.WriteLine("Введите новое имя без расширения .las:");
                    string newName = (Console.ReadLine() ?? "").Trim();
                    */
                    if (string.IsNullOrWhiteSpace(stopDepth))
                    {
                        Console.WriteLine("Переименование остановлено: глубина не найдена.");
                        Console.ReadKey();
                        return;
                    }

                    Console.WriteLine("Введите постоянную часть имени без глубины и расширения:");
                    string header = (Console.ReadLine() ?? "").Trim();

                    string newName = "";

                    if (!string.IsNullOrWhiteSpace(header))
                    {
                        newName = header.TrimEnd('_') + "_" + stopDepth + "м";
                    }
                    if (string.IsNullOrWhiteSpace(newName) ||
                        newName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0 ||
                        newName.EndsWith(".") ||
                        newName.EndsWith(" "))
                    {
                        Console.WriteLine("Недопустимое имя файла.");
                    }
                    else
                    {
                        string newPath = Path.Combine(folderPath, newName + ".las");

                        if (File.Exists(newPath))
                        {
                            Console.WriteLine("Файл с таким именем уже существует.");
                        }
                        else
                        {
                            Console.WriteLine($"Новое имя: {newName}.las");
                            Console.WriteLine("Для переименования введите ДА:");

                            string answer = (Console.ReadLine() ?? "").Trim();

                            if (answer.Equals("ДА", StringComparison.OrdinalIgnoreCase))
                            {
                                try
                                {
                                    latestFile.MoveTo(newPath);
                                    Console.WriteLine("Файл успешно переименован!");

                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine("Не удалось переименовать: " + ex.Message);
                                }
                            }
                            else
                            {
                                Console.WriteLine("Переименование отменено.");
                            }


                        }
                    }
                }
                else
                {
                    Console.WriteLine("Такой папки нет. Проверьте путь.");
                }

                Console.WriteLine("Нажмите любую клавишу для выхода.");
                Console.ReadKey();
            }
        }
    }
}
