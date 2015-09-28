namespace HomeCinema.Data.Infrastructure
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly IDbFactory _dbFactory;
        private HomeCinemaContext _dbContext;

        public UnitOfWork(IDbFactory dbFactory)
        {
            _dbFactory = dbFactory;
        }

        public HomeCinemaContext DbContext => _dbContext ?? (_dbContext = _dbFactory.Init());

        public void Commit()
        {
            DbContext.Commit();
        }
    }
}
