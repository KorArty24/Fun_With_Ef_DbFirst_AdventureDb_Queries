// See https://aka.ms/new-console-template for more information
using EFCore_DBLibrary;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;

namespace FunWithEFAdventureReverse { 
    internal class Program
    {
        private static DbContextOptionsBuilder<AdWorksContext> _optionsBuilder;
        private static IConfigurationRoot _configuration;
        static void Main(string[] args)
        {
            BuildConfiguration();
            BuildOptions();
            ListPeople();
            //ListAllSalesMen();

        }
        static void BuildConfiguration()
        {
            var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json", optional: true,
            reloadOnChange: true);
            _configuration = builder.Build();
        }
        static void BuildOptions()
        {
            _optionsBuilder = new DbContextOptionsBuilder<AdWorksContext>();
            _optionsBuilder.UseSqlServer(_configuration.GetConnectionString(
            "AdventureWorks"));
        }

        static void ListPeople()
        {
            using (var db = new AdWorksContext(_optionsBuilder.Options))
            {
                var salespeople = db.SalesPeople.Include(x => x.BusinessEntity).ThenInclude(y => y.BusinessEntity).ThenInclude(be=>be.Customers).
                    AsNoTracking().Select(e=> new
                    {
                        entId = e.BusinessEntityId,
                        addconin = e.BusinessEntity.BusinessEntity.AdditionalContactInfo,
                        cust= e.BusinessEntity.BusinessEntity.Customers.ToList(),
                        birtdat = e.BusinessEntity.BirthDate
                    }).ToList();

                var result = salespeople.SelectMany(item => item.cust).Select(x => x.CustomerId);
               //foreach (var sp in salespeople)
               // { 
               //     Console.WriteLine($"ID: {sp.entId}\t|AddContIn: {sp.addconin}\t|EntId:{sp.cust}\t" +
               //     $"|BirthDate: {sp.birtdat}\t|AddContIn: {res.}\t|EntId:{sp.cust}\t");
               // }
            }
            
        }
        static void FunWithThenInclude() 
        { 
            using (var db = new AdWorksContext(_optionsBuilder.Options))
            {
                var query = db.SalesPeople.Include(sp => sp.BusinessEntity).ThenInclude(be => be.JobCandidates).Take(4);
                //var query2 = db.SalesPeople.Include(sp => sp.BusinessEntity).Where(sp => sp.BusinessEntity.JobCandidates.Sum(jk => jk.BusinessEntityId) > 8).ToList();
                foreach (var person in query) 
                {
                    Console.WriteLine($"{person.BusinessEntityId}");
                }
            }
        }
        private static string GetSalespersonDetail(SalesPerson sp)
        {
            return $"ID: {sp.BusinessEntityId}\t|TID: {sp.TerritoryId}\t|Quota:{sp.SalesQuota}\t" +
            $"|Bonus: {sp.Bonus}\t|YTDSales: {sp.SalesYtd}\t|Name: \t" +
            $"{sp.BusinessEntity?.BusinessEntity?.FirstName ?? ""}, " +
            $"{sp.BusinessEntity?.BusinessEntity?.LastName ?? ""}";
        }
        
        static void ListAllSalesMen()
        {
             using (var db = new AdWorksContext(_optionsBuilder.Options))
            {
                var salespeople = db.SalesPeople.Include(x => x.BusinessEntity).ThenInclude(y => y.BusinessEntity).AsNoTracking().ToList();
                foreach (var salesperson in salespeople)
                {
                    Console.WriteLine(GetSalespersonDetail(salesperson));
                }
            }
        }
    }
}
