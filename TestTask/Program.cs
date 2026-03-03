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
                using (IReadOnlyStream inputStream2 = GetInputStream(args[1]))
                {
                    IList<LetterStats> singleLetterStats = FillSingleLetterStats(inputStream1);
                    IList<LetterStats> doubleLetterStats = FillDoubleLetterStats(inputStream2);

                    RemoveCharStatsByType(singleLetterStats, CharType.Vowel);
                    RemoveCharStatsByType(doubleLetterStats, CharType.Consonants);

                    PrintStatistic(singleLetterStats);
                    PrintStatistic(doubleLetterStats);
                }
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
            catch (IndexOutOfRangeException)
            {
                Console.WriteLine($"Ошибка, Передано {args.Length} аргументов командой строки вместо 2!");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Возникла непредвиденная ошибка! \nТекст ошибки: " + ex.ToString());
            }

            Console.WriteLine("\n");
            Console.Write("Нажмите на любую кнопку, чтобы закрыть программу: ");
            Console.ReadKey();
        }

        /// <summary>
        /// Ф-ция возвращает экземпляр потока с уже загруженным файлом для последующего посимвольного чтения.
        /// </summary>
        /// <param name="fileFullPath">Полный путь до файла для чтения</param>
        /// <returns>Поток для последующего чтения.</returns>
        private static IReadOnlyStream GetInputStream(string fileFullPath)
        {
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
                    previousChar = c;
                    hasPair = true;
                    continue;
                }

                char upperFirstChar = char.ToUpper(previousChar);
                char upperSecondChar = char.ToUpper(c);

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
        private static void RemoveCharStatsByType(IList<LetterStats> letters, CharType charType)
        {
            switch (charType)
            {
                case CharType.Consonants:
                {
                    RemoveAllConsonants(letters);
                    break;
                }
                case CharType.Vowel:
                {
                    RemoveAllVowels(letters);
                    break;
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
        private static void PrintStatistic(IEnumerable<LetterStats> letters)
        {
            // TODO : Выводить на экран статистику. Выводить предварительно отсортировав по алфавиту!
            throw new NotImplementedException();
        }

        /// <summary>
        /// Удаляет все согласные из списка статистики
        /// </summary>
        /// <param name="letters">список статистики</param>
        private static void RemoveAllConsonants(IList<LetterStats> letters)
        {
            letters = letters.Where(x => VOWELS.Contains(x.Letter[0])).ToList();
        }

        /// <summary>
        /// Удаляет все гласные из списка статистики
        /// </summary>
        /// <param name="letters">список статистики</param>
        private static void RemoveAllVowels(IList<LetterStats> letters)
        {
            letters = letters.Where(x => !VOWELS.Contains(x.Letter[0])).ToList();
        }
    }
}
