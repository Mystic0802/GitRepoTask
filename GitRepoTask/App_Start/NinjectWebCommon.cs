using GitRepoTask.Services;
using Ninject.Web.Common;
using Ninject.Web.Mvc;
using Ninject;
using System.Web.Mvc;
using System.Net.Http;
using System.Runtime.Caching;

namespace GitRepoTask.App_Start
{
    public class NinjectWebCommon
    {
        private static readonly Bootstrapper bootstrapper = new Bootstrapper();

        public static void Start()
        {
            var kernel = CreateKernel();
            DependencyResolver.SetResolver(new NinjectDependencyResolver(kernel));
        }

        private static IKernel CreateKernel()
        {
            var kernel = new StandardKernel();
            RegisterServices(kernel);
            return kernel;
        }

        private static void RegisterServices(IKernel kernel)
        {
            kernel.Bind<HttpClient>().ToMethod(ctx =>
            {
                var client = new HttpClient();
                client.DefaultRequestHeaders.Add("User-Agent", "request");
                return client;
            }).InSingletonScope();
            kernel.Bind<MemoryCache>().ToConstant(MemoryCache.Default);
            kernel.Bind<IRestService>().To<RestService>().InRequestScope();
            kernel.Bind<IGithubService>().To<GithubService>().InRequestScope();
            kernel.Bind<ILoggingService>().To<LoggingService>().InRequestScope();
            kernel.Bind<IValidationService>().To<ValidationService>().InRequestScope();
            kernel.Unbind<ModelValidatorProvider>();
        }
    }
}