using System.Collections.Generic;

namespace TestTask
{
    /// <summary>
    /// Интерфейс хранилища для работы с LetterStats
    /// </summary>
    internal interface ILetterAnalysisStorage
    {
        void HandleText(string text);

        void IncStatistic(LetterStats letterStats);

        IList<LetterStats> GetStatistics();
    }
}
