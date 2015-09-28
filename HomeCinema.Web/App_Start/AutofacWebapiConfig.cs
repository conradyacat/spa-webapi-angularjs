using System.Data.Entity;
using System.Reflection;
using System.Web.Http;
using Autofac;
using Autofac.Integration.WebApi;
using HomeCinema.Data;
using HomeCinema.Data.Infrastructure;
using HomeCinema.Data.Repositories;
using HomeCinema.Services;
using HomeCinema.Services.Abstract;

namespace HomeCinema.Web.App_Start
{
    public class AutofacWebapiConfig
    {
        public static IContainer Container;

        public static int HomeCinemaContext { get; private set; }

        public static void Initialize(HttpConfiguration config)
        {
            Initialize(config, RegisterServices(new ContainerBuilder()));
        }

        public static void Initialize(HttpConfiguration config, IContainer container)
        {
            config.DependencyResolver = new AutofacWebApiDependencyResolver(container);
        }

        private static IContainer RegisterServices(ContainerBuilder builder)
        {
            builder.RegisterApiControllers(Assembly.GetExecutingAssembly());

            // EF HomeCinemaContext
            builder.RegisterType<HomeCinemaContext>().As<DbContext>().InstancePerRequest();
            builder.RegisterType<DbFactory>().As<IDbFactory>().InstancePerRequest();
            builder.RegisterType<UnitOfWork>().As<IUnitOfWork>().InstancePerRequest();
            builder.RegisterGeneric(typeof(EntityBaseRepository<>)).As(typeof(IEntityBaseRepository<>)).InstancePerRequest();

            // services
            builder.RegisterType<EncryptionService>().As<IEncryptionService>().InstancePerRequest();
            builder.RegisterType<MembershipService>().As<IMembershipService>().InstancePerRequest();

            Container = builder.Build();

            return Container;
        }
    }
}