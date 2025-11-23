using System.Threading.Tasks;

namespace ReActionAI.Integration.Revit.Services
{
    public interface IChatClient
    {
        Task<string> SendAsync(string prompt);
    }
}
