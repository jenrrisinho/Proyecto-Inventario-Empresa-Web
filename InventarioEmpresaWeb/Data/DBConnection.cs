using Microsoft.Extensions.Configuration;
using MongoDB.Driver;

namespace InventarioEmpresaWeb.Data
{
    /// <summary>
    /// Clase central de conexión a MongoDB. Se registra como Singleton en
    /// Program.cs y se inyecta (Dependency Injection) en cada Controller
    /// que necesite acceso a la base de datos.
    /// </summary>
    public class DBConnection
    {
        public IMongoDatabase Database { get; }

        public DBConnection(IConfiguration configuration)
        {
            string connectionString = configuration.GetConnectionString("MongoDB");
            string databaseName = configuration["MongoDBSettings:DatabaseName"];

            var client = new MongoClient(connectionString);
            Database = client.GetDatabase(databaseName);
        }
    }
}
