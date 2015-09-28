using System;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Linq.Expressions;
using HomeCinema.Data.Infrastructure;
using HomeCinema.Entities;

namespace HomeCinema.Data.Repositories
{
    public class EntityBaseRepository<T> : IEntityBaseRepository<T> where T : class, IEntityBase, new()
    {
        private HomeCinemaContext _dbContext;

        protected IDbFactory DbFactory { get; }

        protected HomeCinemaContext DbContext => _dbContext ?? (_dbContext = DbFactory.Init());

        public EntityBaseRepository(IDbFactory dbFactory)
        {
            DbFactory = dbFactory;
        }

        public IQueryable<T> All => GetAll();

        public void Add(T entity)
        {
            DbEntityEntry dbEntityEntry = DbContext.Entry<T>(entity);
            DbContext.Set<T>().Add(entity);
        }

        public IQueryable<T> AllIncluding(params Expression<Func<T, object>>[] includeProperties)
        {
            IQueryable<T> query = DbContext.Set<T>();
            foreach (var includeProperty in includeProperties)
            {
                query = query.Include(includeProperty);
            }

            return query;
        }

        public void Delete(T entity)
        {
            DbEntityEntry dbEntityEntry = DbContext.Entry<T>(entity);
            dbEntityEntry.State = EntityState.Deleted;
        }

        public void Edit(T entity)
        {
            DbEntityEntry dbEntityEntry = DbContext.Entry<T>(entity);
            dbEntityEntry.State = EntityState.Modified;
        }

        public IQueryable<T> FindBy(Expression<Func<T, bool>> predicate)
        {
            return DbContext.Set<T>().Where(predicate);
        }

        public IQueryable<T> GetAll()
        {
            return DbContext.Set<T>();
        }

        public T GetSingle(int id)
        {
            return GetAll().FirstOrDefault(x => x.ID == id);
        }
    }
}
