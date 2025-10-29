using System.Threading.Tasks;

namespace ReActionAI.Abstractions
{
    public interface IKnowledgeBase
    {
        Task StoreAsync(string key, string value);
        Task<string?> RetrieveAsync(string key);
    }
}