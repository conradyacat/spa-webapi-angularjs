using System.Web.Http;
using HomeCinema.Web.App_Start;
using HomeCinema.Web.Infrastructure.Mappings;

namespace HomeCinema.Web
{
    public class Bootstrapper
    {
        public static void Run()
        {
            // configure autofac
            AutofacWebapiConfig.Initialize(GlobalConfiguration.Configuration);

            //configure automapper
            AutoMapperConfiguration.Configure();
        }
    }
}
