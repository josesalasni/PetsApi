using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using petsapi.Models;

namespace petsapi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            BuildWebHost(args).Run();
        }

        public static IWebHost BuildWebHost(string[] args) 
        {
            
            var host = WebHost.CreateDefaultBuilder(args)
                //.UseUrls("http://127.0.0.1:5000") //Development
                .UseStartup<Startup>()
                .Build();

            using (var scope = host.Services.CreateScope())
            {  
                var db = scope.ServiceProvider.GetService<TodoContext>();
                
                db.Database.Migrate();
            }

            return host;
        }
    }
}
