namespace TestTask
{
    /// <summary>
    /// Интерфейс фабрики классов для работы с потоками
    /// </summary>
    internal interface IReadOnlyStreamFactory
    {
        IReadOnlyStream GetInputStream(string fileFullPath);
    }
}
