using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace DapperMapper
{
    internal interface IRepository<T>
    {
        T Insert(T entity);
        void Update(T entity);
        void Delete(T entity);
        T FindByID(object idvalue);
        IEnumerable<T> SearchBy(Expression<Func<T, bool>> predicate);
        IEnumerable<T> GetAll();
    }
}
