using Newtonsoft.Json;
using System;
using System.Text;

namespace MyBookDatabase
{
    class Program
    {
        private static NationalDietLibrarySearchAPI ndlsApi = new();
        private static List<BookDataFormat> BookDataList = new();
        private static ConsoleMode Mode = ConsoleMode.Scan;
        private static string? DB_FilePath;

        static void Main(string[] args)
        {
            if (args.Length > 1)
            {
                Console.WriteLine("The argument is incorrect.");
                return;
            }

            if (args.Length == 0)
            {
                Console.Write("Please specify the data file.\n>>");
                DB_FilePath = Console.ReadLine() ?? string.Empty;
            }
            else DB_FilePath = args[0];

            if (!File.Exists(DB_FilePath))
            {
                Console.WriteLine("The data file was not found.");
                if (Judgmenter("Do you want to create a new data file?") ?? false)
                {
                    Console.Write("Enter the file name.\n>>");
                    var path = Console.ReadLine();
                    if (File.Exists(path))
                    {
                        Console.WriteLine("The file already exists.");
                        return;
                    }
                    if (path == null)
                    {
                        Console.WriteLine("Enter the file name.");
                        return;
                    }
                    using (var file = File.Create(path)) { }
                    DB_FilePath = path;
                }
                else return;
            }

            try
            {
                using (var sr = new StreamReader(DB_FilePath, Encoding.GetEncoding("UTF-8")))
                {
                    BookDataList = JsonConvert.DeserializeObject<List<BookDataFormat>>(sr.ReadToEnd()) ?? new();
                }
            }
            catch
            {
                Console.WriteLine("Failed to load. The file may have been modified, or it may be a different file.");
                return;
            }

            Console.WriteLine("My Book Database Console");
            while (true)
            {
                Console.Write($"Console Mode : {Mode} >>");
                var readText = Console.ReadLine()?.ToLower();

                switch (readText)
                {
                    case null:
                        continue;
                    case "exit":
                    case OperationJan.exit:
                        if (Judgmenter("\nDo you want to finish? Contents that are not saved will be lost.") ?? false)
                            return;
                        continue;
                    case "help":
                    case OperationJan.help:
                        Console.Write(
                            $"{ConvertOperationJan(OperationJan.ok)    } : ok\n" +
                            $"{ConvertOperationJan(OperationJan.cancel)} : cancel\n" +
                            $"{ConvertOperationJan(OperationJan.exit)  } : exit\n" +
                            $"{ConvertOperationJan(OperationJan.help)  } : help\n" +
                            $"{ConvertOperationJan(OperationJan.scan)  } : scan\n" +
                            $"{ConvertOperationJan(OperationJan.insert)} : insert\n" +
                            $"{ConvertOperationJan(OperationJan.list)  } : list\n" +
                            $"{ConvertOperationJan(OperationJan.count) } : count\n" +
                            $"{ConvertOperationJan(OperationJan.save)  } : save\n" +
                            $"{ConvertOperationJan(OperationJan.remove)} : remove\n");
                        continue;
                    case "scan":
                    case OperationJan.scan:
                        Mode = ConsoleMode.Scan;
                        continue;
                    case "insert":
                    case OperationJan.insert:
                        Mode = ConsoleMode.Insert;
                        continue;
                    case "list":
                    case OperationJan.list:
                        foreach (var data in BookDataList) Console.WriteLine($"ISBN : {data.ISBN}");
                        continue;
                    case "count":
                    case OperationJan.count:
                        Console.WriteLine($"List count : {BookDataList.Count}");
                        continue;
                    case "save":
                    case OperationJan.save:
                        try
                        {
                            using (var writer = new StreamWriter(DB_FilePath, false, Encoding.GetEncoding("UTF-8")))
                                writer.Write(JsonConvert.SerializeObject(BookDataList));
                            Console.WriteLine("Saved.");
                        }
                        catch
                        {
                            Console.WriteLine("Failed to write to file.");
                        }
                        continue;
                    case "remove":
                    case OperationJan.remove:
                        Console.Write("Enter the ISBN to be removed from the list.\n>>");
                        var read = Console.ReadLine();
                        if (!long.TryParse(read, out long isbm) || read.Length != 13)
                        {
                            Console.WriteLine("The entered string is not ISBN.");
                            continue;
                        }
                        var deleteIndex = BookDataList.Select(p => p.ISBN.Replace("-","")).ToList().IndexOf(read);
                        if (deleteIndex == -1)
                        {
                            Console.WriteLine("This ISBN was not found from List.");
                            continue;
                        }
                        NationalDietLibrarySearchAPI.Show(BookDataList[deleteIndex]);
                        if (Judgmenter("\nDo you want to delete this content?") ?? false)
                        {
                            BookDataList.RemoveAt(deleteIndex);
                            Console.WriteLine("Done.");
                        }
                        continue;
                }

                var T_BookData = ndlsApi.GetData(readText);
                if (T_BookData == null)
                {
                    Console.WriteLine("The command or ISBN was not found.");
                    Beep.Error();
                    continue;
                }
                var bookData = (BookDataFormat)T_BookData;

                switch (Mode)
                {
                    case ConsoleMode.Scan:
                        NationalDietLibrarySearchAPI.Show(bookData);
                        break;
                    case ConsoleMode.Insert:
                        if (!BookDataList.Select(p => p.ISBN).Contains(bookData.ISBN))
                        {
                            BookDataList.Add(bookData);
                            NationalDietLibrarySearchAPI.Show(bookData);
                            Console.WriteLine($"Added to the list. {BookDataList.Count - 1} => {BookDataList.Count}");
                        }
                        else
                        {
                            Console.WriteLine("The loaded ISBN already exists in the list.");
                            Beep.Error();
                            continue;
                        }
                        break;
                }

                Beep.Normal();
                Console.WriteLine();
            }
        }

        private static bool? Judgmenter(string text)
        {
            Console.Write($"{text}(y/n) or (ok/cansel)\n>>");
            switch (Console.ReadLine())
            {
                case "y":
                case OperationJan.ok:
                    return true;
                case "n":
                case OperationJan.cancel:
                    Console.WriteLine("Canceled.");
                    return false;
                default:
                    Console.WriteLine("An invalid string has been entered.");
                    return null;
            }
        }

        private static string ConvertOperationJan(string operationJan) =>
                operationJan.Insert(8, "-").Insert(13, "-");

        enum ConsoleMode
        {
            Scan,
            Insert,
        }

        private static class OperationJan
        {
            public const string ok     = "2022020000004";
            public const string cancel = "2022020000011";
            public const string exit   = "2022020100001";
            public const string help   = "2022020100018";
            public const string scan   = "2022020100025";
            public const string insert = "2022020100032";
            public const string list   = "2022020100049";
            public const string count  = "2022020100056";
            public const string save   = "2022020100063";
            public const string remove = "2022020100070";
        }
    }
}