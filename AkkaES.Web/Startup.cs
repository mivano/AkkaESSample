using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.Configuration;
using Akka.Persistence;
using Akka.Persistence.Query;
using Akka.Streams.Actors;
using Akka.Streams.Dsl;
using AkkaES.Business.Customers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Reactive.Streams;

namespace AkkaES.Web
{
    
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            var config = ConfigurationFactory.ParseString(@"
                akka {
                  actor : {
                    debug : {
                      unhandled : on
                      receive : on
                      autoreceive : on
                      lifecycle : on
                      event-stream : on
                    }
                  }

                  stdout-loglevel : Error
                  loglevel : DEBUG
                  log-config-on-start : off
                  loggers=[""Akka.Logger.Serilog.SerilogLogger, Akka.Logger.Serilog""]

                  persistence{

                    journal {
                        plugin = ""akka.persistence.journal.sql-server""
                        sql-server {
                            class = ""Akka.Persistence.SqlServer.Journal.SqlServerJournal, Akka.Persistence.SqlServer""
                            schema-name = dbo
                            auto-initialize = on
                            connection-string = ""Server=(localdb)\\mssqllocaldb;Initial Catalog=AkkaESSample;Connection Timeout=3;Persist Security Info=True;Trusted_Connection=True;MultipleActiveResultSets=true;""
                        }
                    } 
    
                    snapshot-store {
                        plugin = ""akka.persistence.snapshot-store.sql-server""
                        sql-server {
                            class = ""Akka.Persistence.SqlServer.Snapshot.SqlServerSnapshotStore, Akka.Persistence.SqlServer""
                            schema-name = dbo
                            connection-timeout = 10s    
                            table-name = ""Snapshots""
                            auto-initialize = on
                            connection-string = ""Server=(localdb)\\mssqllocaldb;Initial Catalog=AkkaESSample;Connection Timeout=3;Persist Security Info=True;Trusted_Connection=True;MultipleActiveResultSets=true;""
                        }
                    }

                  }
                }
            ");

            var akkaSystem = ActorSystem.Create("akkaes", config);
            akkaSystem.ActorOf<CustomerCoordinator>("CustomerCoordinator");
            akkaSystem.ActorOf<CustomersView>("CustomersView");


            services.AddSingleton(typeof(ActorSystem), (serviceProvider) => akkaSystem);

            services.AddMvc(options =>
            {
                options.RespectBrowserAcceptHeader = true;

                options.OutputFormatters.Add(new XmlSerializerOutputFormatter());
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMvc();
        }
    }
}
