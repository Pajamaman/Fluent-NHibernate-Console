using System;

using FluentNHibernate.Automapping;
using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using FluentNHibernate.Conventions.Helpers;
using FluentNHibernateConsole.Entities;
using NHibernate;
using NHibernate.Cfg;
using NHibernate.Tool.hbm2ddl;

namespace FluentNHibernateConsole
{
    public class Program
    {
        // Adds sample data to our database and writes the data to the console
        static void Main()
        {
            // Create a session factory
            var sessionFactory = CreateSessionFactory();

            // Open a session
            using ( var session = sessionFactory.OpenSession() )
            {
                // Begin a transaction
                using ( var transaction = session.BeginTransaction() )
                {
                    // Create a couple of Stores each with some Products and Employees
                    var barginBasin = new Store { Name = "Bargin Basin" };
                    var superMart = new Store { Name = "SuperMart" };

                    var potatoes = new Product { Name = "Potatoes", Price = 3.60 };
                    var fish = new Product { Name = "Fish", Price = 4.49 };
                    var milk = new Product { Name = "Milk", Price = 0.79 };
                    var bread = new Product { Name = "Bread", Price = 1.29 };
                    var cheese = new Product { Name = "Cheese", Price = 2.10 };
                    var waffles = new Product { Name = "Waffles", Price = 2.41 };

                    var daisy = new Employee { FirstName = "Daisy", LastName = "Harrison" };
                    var jack = new Employee { FirstName = "Jack", LastName = "Torrance" };
                    var sue = new Employee { FirstName = "Sue", LastName = "Walkters" };
                    var bill = new Employee { FirstName = "Bill", LastName = "Taft" };
                    var joan = new Employee { FirstName = "Joan", LastName = "Pope" };

                    // Add Products to the Stores
                    // The Store-Product relationship is many-to-many
                    AddProductsToStore( barginBasin, potatoes, fish, milk, bread, cheese );
                    AddProductsToStore( superMart, bread, cheese, waffles );

                    // Add Employees to the Stores
                    // The Store-Employee relationship is one-to-many
                    AddEmployeesToStore( barginBasin, daisy, jack, sue );
                    AddEmployeesToStore( superMart, bill, joan );

                    // Save the session
                    session.SaveOrUpdate( barginBasin );
                    session.SaveOrUpdate( superMart );

                    // Commit the transaction
                    transaction.Commit();
                }

                // Begin a transaction
                using ( var transaction = session.BeginTransaction() )
                {
                    var stores = session.CreateCriteria( typeof( Store ) )
                        .List<Store>();

                    foreach ( var store in stores )
                    {
                        WriteStorePretty( store );
                    }

                    // Commit the transaction
                    transaction.Commit();
                }
            }

            Console.ReadKey();
        }

        // Returns our session factory
        private static ISessionFactory CreateSessionFactory()
        {
            return Fluently.Configure()
                .Database( CreateDbConfig )
                .Mappings( m => m
                    .AutoMappings.Add( CreateMappings() ) )
                .ExposeConfiguration( DropCreateSchema )
                .BuildSessionFactory();
        }

        // Returns our database configuration
        private static MsSqlConfiguration CreateDbConfig()
        {
            return MsSqlConfiguration
                .MsSql2008
                .ConnectionString( c => c
                    .Server( "localhost\\SQLEXPRESS" )
                    .Database( "DaveTest" )
                    .TrustedConnection() );
        }

        // Returns our mappings
        private static AutoPersistenceModel CreateMappings()
        {
            return AutoMap
                .Assembly( System.Reflection.Assembly.GetCallingAssembly() )
                .Where( t => t
                    .Namespace == "FluentNHibernateConsole.Entities" )
                .Conventions.Setup( c => c
                    .Add( DefaultCascade.SaveUpdate() ) );
        }

        // Drops and creates the database schema
        private static void DropCreateSchema( Configuration cfg )
        {
            new SchemaExport( cfg )
                .Create( false, true );
        }

        // Writes the store data, along with its associated products and employees, to the console
        private static void WriteStorePretty( Store store )
        {
            Console.WriteLine( store.Name );
            Console.WriteLine( " Products:" );

            foreach ( var product in store.Products )
            {
                Console.WriteLine( " " + product.Name );
            }

            Console.WriteLine( " Staff:" );

            foreach ( var employee in store.Staff )
            {
                Console.WriteLine( " " + employee.FirstName + " " + employee.LastName );
            }

            Console.WriteLine();
        }

        // Adds any products that we pass in to the store that we pass in
        public static void AddProductsToStore( Store store, params Product[] products )
        {
            foreach ( var product in products )
            {
                store.AddProduct( product );
            }
        }

        // Adds any employees that we pass in to the store that we pass in
        public static void AddEmployeesToStore( Store store, params Employee[] employees )
        {
            foreach ( var employee in employees )
            {
                store.AddEmployee( employee );
            }
        }
    }
}
