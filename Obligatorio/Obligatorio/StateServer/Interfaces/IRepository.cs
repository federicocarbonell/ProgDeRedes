using System.Collections.Generic;
using System.Linq;

namespace StateServer.Interfaces
{
    public interface IRepository<T>
    {
        List<T> GetAll();
        IRepository<T> GetInstance();
        T Get(int id);
        void Add(T entity);
        void Update(int id, T newEntity);
        void Delete(int id);
        void BuyGame(int id, string buyer) { }
    }
}
