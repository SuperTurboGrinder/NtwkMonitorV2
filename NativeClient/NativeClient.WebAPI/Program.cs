using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

namespace NativeClient.WebAPI
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            BuildWebHost(args).Run();
        }

        static IWebHost BuildWebHost(string[] args)
        {
            return WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .UseUrls("http://localhost:5438/")
                .Build();
        }
    }
}