using System.Threading.Tasks;

namespace ReActionAI.Modules.RevitChatGPT.Services
{
    public interface IChatClient
    {
        Task<string> SendAsync(string prompt);
    }
}
