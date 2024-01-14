using Microsoft.Extensions.Configuration;
using NHibernate;
using NHibernate.Cfg.MappingSchema;
using NHibernate.Mapping.ByCode;
using System;
using NHibernate.Cfg;
using NHibernate.Driver;
using NHibernate.Dialect;

namespace EF04.Shared
{
    internal class Program
    {
        public static void Main(String[] args)
        {
            RetriveData();
            Console.ReadKey();
        }

        private static void RetriveData()
        {
            using (var session = CreateSession())
            {
                using (var transaction = session.BeginTransaction())
                {
                    var wallets = session.Query<Wallet>();
                    foreach (var wallet in wallets)
                    {
                        Console.WriteLine(wallet);
                    }
                }
            }
        }

        private static void RetriveDataById()
        {
            using (var session = CreateSession())
            {
                using (var transaction = session.BeginTransaction())
                {
                    int walletIdToFind = 2;
                    var wallet = session.Query<Wallet>().FirstOrDefault(x => x.Id == walletIdToFind);
                    Console.WriteLine(wallet);
                }
            }
        }
        private static void InsertWallet()
        {
            using (var session = CreateSession())
            {
                using (var transaction = session.BeginTransaction())
                {
                    var walletToAdd = new Wallet { Holder = "Smith", Balance = 0 };
                    session.Save(walletToAdd);
                    Console.WriteLine(walletToAdd);
                    transaction.Commit();

                }
            }
        }
        private static void UpdateWallet()
        {
            using (var session = CreateSession())
            {
                using (var transaction = session.BeginTransaction())
                {
                    var idToUpdate = 10;
                    var wallet = session.Get<Wallet>(idToUpdate);
                    wallet.Balance = 99m;
                    session.Update(wallet);
                    Console.WriteLine(wallet);
                    transaction.Commit();
                }
            }
        }

        private static void DeleteWallet()
        {
            using (var session = CreateSession())
            {
                using (var transaction = session.BeginTransaction())
                {
                    var idToDelete = 10;

                    var wallet = session.Get<Wallet>(idToDelete);
                    session.Delete(wallet);
                    transaction.Commit();
                }
            }
        }
        private static void TransferAndTransactionAndConsistency()
        {
            using (var session = CreateSession())
            {
                using (var transaction = session.BeginTransaction())
                {
                    var idFrom = 3;
                    var idTo = 2;
                    var amountToTarnsfer = 1000;

                    var walletFrom = session.Get<Wallet>(idFrom);
                    var walletTo = session.Get<Wallet>(idTo);

                    walletFrom.Balance -= amountToTarnsfer;
                    walletTo.Balance += amountToTarnsfer;
                    session.Update(walletFrom);
                    session.Update(walletTo);
                    transaction.Commit();   
                
                }
            }
        }



        private static ISession CreateSession()
        {
            var config = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
            var constr = config.GetSection("constr").Value;

            var mapper = new ModelMapper();

            //list all of type mappings from assembly
            mapper.AddMappings(typeof(Wallet).Assembly.ExportedTypes);

            //combile class mapping
            HbmMapping domainMapping = mapper.CompileMappingForAllExplicitlyAddedEntities();

            //Allow the application to specify prop and mapping documents to be used when creating
            var hbConfig = new Configuration();

            //setting from app to nhibernate
            hbConfig.DataBaseIntegration(c =>
            {
                //strategy to interact with provider
                c.Driver<MicrosoftDataSqlClientDriver>();

                //dialect nhibernate uses to build syntaxt to rdbms
                c.Dialect<MsSql2012Dialect>();

                //connectionString
                c.ConnectionString = constr;

                // log sql statment to console
                c.LogSqlInConsole = true;

                //format logged sql statment
                c.LogFormattedSql = true;
            });
            hbConfig.AddMapping(domainMapping);

            //instantitate a new IsessionFactory (use Props , settings and mapping)
            var sessionFactory = hbConfig.BuildSessionFactory();

            var session = sessionFactory.OpenSession();

            return session;
        }
    }
}