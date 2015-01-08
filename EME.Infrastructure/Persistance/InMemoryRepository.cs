using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EME.Infrastructure.Persistance
{
    public class InMemoryRepository<T> : IRepository<T>
    {
        BlockingCollection<T> m_items;

        public InMemoryRepository()
        {
            m_items = new BlockingCollection<T>();
        }

        public IQueryable<T> Query()
        {
            return m_items.AsQueryable();
        }

        public void Add(T item)
        {
            if (item == null) throw new ArgumentNullException("item");

            m_items.Add(item);
        }

        public void Remove(T item)
        {
            if (item == null) throw new ArgumentNullException("item");

            m_items.TryTake(out item);
        }

        public void Update(T item)
        {
            if (item == null) throw new ArgumentNullException("item");

            // there is no reason to implement update, because the referenced item is already updated
            // side note: trickier to implement in generics, T must be defined with BaseEntity
        }
    }
}
