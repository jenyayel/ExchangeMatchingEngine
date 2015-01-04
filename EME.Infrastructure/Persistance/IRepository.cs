using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EME.Infrastructure.Persistance
{
    public interface IRepository<T> 
    {
        IQueryable<T> Query();

        void Add(T item);

        void Remove(T item);
    }
}
