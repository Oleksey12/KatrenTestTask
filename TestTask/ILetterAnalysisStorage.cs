using System.Collections.Generic;

namespace TestTask
{
    /// <summary>
    /// Интерфейс контейнера для подсчёта количества вхождений текста
    /// </summary>
    internal interface ILetterAnalysisStorage
    {
        void HandleText(string text);

        void IncStatistic(LetterStats letterStats);

        IList<LetterStats> GetStatistics();
    }
}
