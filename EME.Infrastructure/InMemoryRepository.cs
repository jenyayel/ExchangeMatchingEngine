using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EME.Infrastructure
{
    public class InMemoryRepository<T> : IRepository<T>
    {
        List<T> m_items;

        public InMemoryRepository()
        {
            m_items = new List<T>();
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

            m_items.Remove(item);
        }
    }
}
