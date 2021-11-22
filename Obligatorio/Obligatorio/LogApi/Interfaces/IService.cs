using System.Collections.Generic;
using System.Threading.Tasks;

namespace LogApi.Interfaces
{
    public interface IService
    {
        Task<List<string>> GetMessages(string user, string action, string date);
    }
}
