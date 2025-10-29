using System.Threading.Tasks;

namespace ReActionAI.Abstractions
{
    public interface IChatService
    {
        Task<string> AskAsync(string input);
    }
}