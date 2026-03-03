using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace TestTask
{
    public class Program
    {
        private const string VOWELS = "AEIOUYАОЕЯЁЭЫУИЮaeiouyаоеяёэыуию";

        /// <summary>
        /// Программа принимает на входе 2 пути до файлов.
        /// Анализирует в первом файле кол-во вхождений каждой буквы (регистрозависимо). Например А, б, Б, Г и т.д.
        /// Анализирует во втором файле кол-во вхождений парных букв (не регистрозависимо). Например АА, Оо, еЕ, тт и т.д.
        /// По окончанию работы - выводит данную статистику на экран.
        /// </summary>
        /// <param name="args">Первый параметр - путь до первого файла.
        /// Второй параметр - путь до второго файла.</param>
        static void Main(string[] args)
        {
            try
            {
                using (IReadOnlyStream inputStream1 = GetInputStream(args[0]))
                {
                    IList<LetterStats> singleLetterStats = FillSingleLetterStats(inputStream1);
                    singleLetterStats = RemoveCharStatsByType(singleLetterStats, CharType.Vowel);

                    Console.WriteLine($"Результаты анализа файла {args[0]}\n");
                    PrintStatistic(singleLetterStats);
                }

                Console.WriteLine("\n");

                using (IReadOnlyStream inputStream2 = GetInputStream(args[1]))
                {
                    IList<LetterStats> doubleLetterStats = FillDoubleLetterStats(inputStream2);
                    doubleLetterStats = RemoveCharStatsByType(doubleLetterStats, CharType.Consonants);

                    Console.WriteLine($"Результаты анализа файла {args[1]}\n");
                    PrintStatistic(doubleLetterStats);
                }
            }
            catch (IndexOutOfRangeException)
            {
                Console.WriteLine($"Ошибка, Передано {args.Length} аргументов командой строки вместо 2!");
            }
            catch (ArgumentNullException ex)
            {
                Console.WriteLine(ex.Message);
            }
            catch (DirectoryNotFoundException ex)
            {
                Console.WriteLine(ex.Message);
            }
            catch (FileNotFoundException ex)
            {
                Console.WriteLine(ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Возникла непредвиденная ошибка! \nТекст ошибки: " + ex.ToString());
            }

            Console.WriteLine("\n");
            Console.Write("Нажмите на любую кнопку, чтобы закрыть программу...");
            Console.ReadKey();
        }

        /// <summary>
        /// Ф-ция возвращает экземпляр потока с уже загруженным файлом для последующего посимвольного чтения.
        /// </summary>
        /// <param name="fileFullPath">Полный путь до файла для чтения</param>
        /// <returns>Поток для последующего чтения.</returns>
        private static IReadOnlyStream GetInputStream(string fileFullPath)
        {
            if (fileFullPath == null)
            {
                throw new ArgumentNullException("Передано null вместо названия файла");
            }

            if (!Directory.Exists(Path.GetDirectoryName(fileFullPath)))
            {
                throw new DirectoryNotFoundException($"Директории с файлом {fileFullPath} не существует");
            }

            if (!File.Exists(fileFullPath))
            {
                throw new FileNotFoundException($"Файла в пути {fileFullPath} не существует");
            }

            return new ReadOnlyStream(fileFullPath);
        }

        /// <summary>
        /// Ф-ция считывающая из входящего потока все буквы, и возвращающая коллекцию статистик вхождения каждой буквы.
        /// Статистика РЕГИСТРОЗАВИСИМАЯ!
        /// </summary>
        /// <param name="stream">Стрим для считывания символов для последующего анализа</param>
        /// <returns>Коллекция статистик по каждой букве, что была прочитана из стрима.</returns>
        private static IList<LetterStats> FillSingleLetterStats(IReadOnlyStream stream)
        {
            stream.ResetPositionToStart();
            ILetterAnalysisStorage storage = new DictionaryLetterStorage();

            while (!stream.IsEof)
            {
                char c = stream.ReadNextChar();

                if (!char.IsLetter(c))
                {
                    continue;
                }

                string letterText = c.ToString();
                storage.HandleText(letterText);
            }

            return storage.GetStatistics();
        }

        /// <summary>
        /// Ф-ция считывающая из входящего потока все буквы, и возвращающая коллекцию статистик вхождения парных букв.
        /// В статистику должны попадать только пары из одинаковых букв, например АА, СС, УУ, ЕЕ и т.д.
        /// Статистика - НЕ регистрозависимая!
        /// </summary>
        /// <param name="stream">Стрим для считывания символов для последующего анализа</param>
        /// <returns>Коллекция статистик по каждой букве, что была прочитана из стрима.</returns>
        private static IList<LetterStats> FillDoubleLetterStats(IReadOnlyStream stream)
        {
            stream.ResetPositionToStart();
            ILetterAnalysisStorage storage = new DictionaryLetterStorage();

            bool hasPair = false;
            char previousChar = ' ';

            while (!stream.IsEof)
            {
                char c = stream.ReadNextChar();
                if (!char.IsLetter(c))
                {
                    hasPair = false;
                    continue;
                }

                if (!hasPair)
                {
                    previousChar = char.ToUpper(c);
                    hasPair = true;
                    continue;
                }

                char upperFirstChar = previousChar;
                char upperSecondChar = char.ToUpper(c);
                previousChar = upperSecondChar;

                if (upperFirstChar != upperSecondChar)
                {
                    continue;
                }

                string letterText = string.Concat(upperFirstChar, upperSecondChar);
                storage.HandleText(letterText);
            }

            return storage.GetStatistics();
        }

        /// <summary>
        /// Ф-ция перебирает все найденные буквы/парные буквы, содержащие в себе только гласные или согласные буквы.
        /// (Тип букв для перебора определяется параметром charType)
        /// Все найденные буквы/пары соответствующие параметру поиска - удаляются из переданной коллекции статистик.
        /// </summary>
        /// <param name="letters">Коллекция со статистиками вхождения букв/пар</param>
        /// <param name="charType">Тип букв для анализа</param>
        private static IList<LetterStats> RemoveCharStatsByType(IList<LetterStats> letters, CharType charType)
        {
            switch (charType)
            {
                case CharType.Consonants:
                {
                    return RemoveAllConsonants(letters);
                }
                case CharType.Vowel:
                {
                    return RemoveAllVowels(letters);
                }
                default:
                {
                    return letters;
                }
            }
        }

        /// <summary>
        /// Ф-ция выводит на экран полученную статистику в формате "{Буква} : {Кол-во}"
        /// Каждая буква - с новой строки.
        /// Выводить на экран необходимо предварительно отсортировав набор по алфавиту.
        /// В конце отдельная строчка с ИТОГО, содержащая в себе общее кол-во найденных букв/пар
        /// </summary>
        /// <param name="letters">Коллекция со статистикой</param>
        private static void PrintStatistic(IEnumerable<LetterStats> letters, int columnLength = 15)
        {
            IEnumerable<LetterStats> sortedLetters = letters.OrderBy(x => x.Letter);

            Console.WriteLine($"|{new string('-', columnLength)}|{new string('-', columnLength)}|");
            Console.WriteLine($"|{"Буква".PadRight(columnLength)}|{"Кол-во".PadRight(columnLength)}|");
            foreach (LetterStats letterData in sortedLetters)
            {
                Console.WriteLine($"|{new string('-', columnLength)}|{new string('-', columnLength)}|");
                string letterText = letterData.Letter;
                string countText = letterData.Count.ToString();

                Console.WriteLine($"|{letterText.PadRight(columnLength)}|{countText.PadRight(columnLength)}|");
            }

            Console.WriteLine($"|{new string('-', columnLength)}|{new string('-', columnLength)}|");
        }

        /// <summary>
        /// Удаляет все согласные из списка статистики
        /// </summary>
        /// <param name="letters">список статистики</param>
        private static IList<LetterStats> RemoveAllConsonants(IList<LetterStats> letters)
        {
            return letters.Where(x => VOWELS.Contains(x.Letter[0])).ToList();
        }

        /// <summary>
        /// Удаляет все гласные из списка статистики
        /// </summary>
        /// <param name="letters">список статистики</param>
        private static IList<LetterStats> RemoveAllVowels(IList<LetterStats> letters)
        {
            return letters.Where(x => !VOWELS.Contains(x.Letter[0])).ToList();
        }
    }
}
