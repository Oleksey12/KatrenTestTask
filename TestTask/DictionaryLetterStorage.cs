using System.Collections.Generic;
using System.Linq;

namespace TestTask
{
	/// <summary>
	/// Контейнер для подсчёта количества вхождений текста с помощью словаря
	/// </summary>
	public class DictionaryLetterStorage : ILetterAnalysisStorage
	{
		private readonly Dictionary<string, LetterStats> _data = new Dictionary<string, LetterStats>();

		/// <summary>
		/// Обрабатывает статистику по введённому тексту 
		/// </summary>
		///<param name="text"></param>
		public void HandleText(string text)
		{
			if (_data.TryGetValue(text, out LetterStats data))
			{
				IncStatistic(data);
			} 
			else
			{
				data = new LetterStats
				{
					Letter = text.ToString(),
					Count = 1
				};
				_data.Add(text, data);
			}
		}

		/// <summary>
		/// Метод увеличивает счётчик вхождений по переданной структуре.
		/// </summary>
		/// <param name="letterStats"></param>
		public void IncStatistic(LetterStats letterStats)
		{
			letterStats.Count++;
		}

		/// <summary>
		/// Возвращает список с данными вхождений текста
		/// </summary>
		/// <returns>Список с данными вхождений текста</returns>
		public IList<LetterStats> GetStatistics() => _data.Values.ToList();
	}
}