using Newtonsoft.Json;
using System;
using System.Text;
using System.Text.RegularExpressions;

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
                            $"{ConvertOperationJan(OperationJan.check) } : check\n" +
                            $"{ConvertOperationJan(OperationJan.list)  } : list\n" +
                            $"{ConvertOperationJan(OperationJan.sort)  } : sort\n" +
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
                    case "check":
                    case OperationJan.check:
                        Mode = ConsoleMode.Check;
                        continue;
                    case "list":
                    case OperationJan.list:
                        foreach (var data in BookDataList)
                            Console.WriteLine(
                                $"{data.ISBN} => " +
                                $"{data.Title} => " +
                                $"{string.Join(" / ", data.Publishers ?? new string[0])} => " +
                                $"{string.Join(" / ", data.Series_Title)} => " +
                                $"{string.Join(" / ", data.Creators ?? new string[0])}");
                        continue;
                    case "sort":
                    case OperationJan.sort:
                        Sort();
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
                        var deleteIndex = BookDataList.Select(p => p.ISBN_ID).ToList().IndexOf(isbm);
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
                        if (!BookDataList.Select(p => p.ISBN_ID).Contains(bookData.ISBN_ID))
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
                    case ConsoleMode.Check:
                        Console.WriteLine("\n" + ndlsApi.GetDataString(readText));
                        break;
                }

                Beep.Normal();
                Console.WriteLine();
            }
        }

        private static void Sort()
        {
            var TitleGroup = BookDataList.GroupBy(p => Regex.Replace(p.Title_Trans_Meta ?? string.Empty, @"\s", string.Empty)).OrderBy(p => p.Key);
            var bookGroup = new List<BookGroupFormat>();
            foreach (var group in TitleGroup)
            {
                var title = group.Key;
                var publisher = MostValue(group, p => p.Publishers_Trans?.FirstOrDefault());
                var series = MostValue(group, p => p.Series_Title_Trans);
                var creator = MostValue(group, p => p.Creators_Trans?.FirstOrDefault());
                var bookData = group.ToArray();
                bookGroup.Add(new BookGroupFormat()
                {
                    Publisher = publisher,
                    Series = series,
                    Creator = creator,
                    Title = title,
                    Books = bookData
                });
            }

            var bookSortData = new List<BookDataFormat>();
            var tempSortData = bookGroup.OrderBy(p => p.Publisher).ThenBy(p => p.Series).ThenBy(p => p.Creator);
            foreach (var books in tempSortData)
                bookSortData.AddRange(books.Books.OrderBy(p => int.Parse(Regex.Replace(p.Volume_Trans ?? "0", @"[^0-9]", ""))));

            Console.WriteLine("\nAll database information");
            Console.WriteLine($"Total Book Title Count : {TitleGroup.Count()}");
            Console.WriteLine($"Total Book Count       : {bookSortData.Count()}\n");

            if (Judgmenter("Do you want the sort to be reflected in the list? It will not be saved.") ?? false)
            {
                BookDataList = bookSortData;
                Console.WriteLine("Done.");
            }
        }

        private static string MostValue(IGrouping<string, BookDataFormat> group, Func<BookDataFormat, string?> func)
        {
            var count = new Dictionary<string, int>();
            foreach (var item in group)
            {
                var data = func(item);
                if (data == null) continue;
                if (!count.ContainsKey(data)) count.Add(data, 1);
                else count[data]++;
            }
            return count.OrderByDescending(p => p.Value).FirstOrDefault().Key;
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

        public struct BookGroupFormat
        {
            public string? Publisher;
            public string? Series;
            public string? Creator;
            public string? Title;
            public BookDataFormat[] Books;
        }

        enum ConsoleMode
        {
            Scan,
            Insert,
            Check
        }

        private static class OperationJan
        {
            public const string ok     = "2022020000004";
            public const string cancel = "2022020000011";
            public const string exit   = "2022020100001";
            public const string help   = "2022020100018";
            public const string scan   = "2022020100025";
            public const string insert = "2022020100032";
            public const string check  = "2022020100087";
            public const string list   = "2022020100049";
            public const string sort   = "2022020100094";
            public const string count  = "2022020100056";
            public const string save   = "2022020100063";
            public const string remove = "2022020100070";

        }
    }
}