namespace ReActionAI.KB
{
    public interface IRagStore
    {
        // id остаётся необязательным параметром (по умолчанию null),
        // но без использования nullable-аннотаций, чтобы не было CS8632.
        void Index(string text, string id = null);

        string Query(string query, int topK = 5);
    }
}
