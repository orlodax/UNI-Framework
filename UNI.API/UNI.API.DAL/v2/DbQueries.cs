using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UNI.API.DAL.v2
{
    public class DbQueries
    {
        private readonly string connectionString;
        protected ILogger logger;

        public DbQueries(string connectionString)
        {
            this.connectionString = connectionString;

            using var loggerFactory = LoggerFactory.Create(loggingBuilder => loggingBuilder
                                                        .SetMinimumLevel(LogLevel.Trace)
                                                        .AddConsole());
            logger = loggerFactory.CreateLogger<DbQueries>();
        }
    }
}
