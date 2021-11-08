﻿using System.Linq;

namespace StateServer.Interfaces
{
    interface IRepository<T>
    {
        IQueryable<T> GetAll();
        T Get(int id);
        void Add(T entity);
        void Update(int id, T newEntity);
        void Delete(int id);

    }
}