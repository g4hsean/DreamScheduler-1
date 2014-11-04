﻿using Neo4jClient;
using Ninject.Activation;
using Ninject.Modules;

using System;
using System.Collections.Generic;
using System.Configuration;


namespace DreamSchedulerApplication.App_Start.Modules
{
    public class Neo4jModule : NinjectModule
    {
        /// <summary>Loads the module into the kernel.</summary>
        public override void Load()
        {
            Bind<IGraphClient>().ToMethod(InitNeo4JClient).InSingletonScope();
        }

        private static IGraphClient InitNeo4JClient(IContext context)
        {
            var graphClient = new GraphClient(new Uri("http://localhost:7474/db/data"));
            graphClient.Connect();

            return graphClient;
        }
    }
}