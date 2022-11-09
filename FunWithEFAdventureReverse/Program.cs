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
            //ReturnPercentageOfTax();
            //UniqueJobTitles();
            //ListPeople();
            //ListEmployee();
            //ListAllSalesMen();
           // ReturnUniqueJobTitles();
            ReturnTotalFreight();

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
            _optionsBuilder.UseSqlServer(_configuration.GetConnectionString("AdventureWorks"));
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
                //foreach(var person in result  )
            }
            
        }
        #region ListEmployee
        static void ListEmployee()
        {
            using (var db = new AdWorksContext(_optionsBuilder.Options))
            {
                var employee = db.Employees.Select(e => e);
                foreach (var person in employee)
                {
                    Console.WriteLine(GetEmployeeDetail(person));
                }
            }
        }
        #endregion
        #region FunWithThenInclude()
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
        #endregion
       

        #region ListAllSalesMen
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
        #endregion

        private static void ReturnSubset()
        {
            using (var db = new AdWorksContext(_optionsBuilder.Options))
            {
                var salespeopleSubs = db.People.Select(sm => new
                {
                    FirstName = sm.FirstName,
                    LastName = sm.LastName,
                    EmployeeId = sm.BusinessEntityId 
                }).Take(100);
            }
        }

        #region productline
        /// <summary>
        /// Returns only the rows for product that have a sellstartdate that is not NULL and a productline of 'T'.
        /// Return productid, productnumber, and name. 
        /// </summary>
        private static void ReturnProductLineT() 
        {
            using (var db = new AdWorksContext(_optionsBuilder.Options))
            {
                var pdoductlinesubset = db.Products.Where(pr=>!pr.SellStartDate.Equals(null)).Select(p => new
                {
                    Name = p.Name,
                    productnumber = p.ProductNumber,
                    ProdId = p.ProductId 
                }).AsNoTracking().Take(100);
            }
        }
        #endregion

        #region taxpercentage
        /// <summary>
        ///  From the following table write a query in SQL to return all rows from the salesorderheader table in Adventureworks 
        ///  database and calculate the percentage of tax on the subtotal have decided.
        ///  Return salesorderid, customerid, orderdate, subtotal, percentage of tax column.
        ///  Arranged the result set in ascending order on subtotal.
        ///  (taxamt*100)/subtotal
        /// </summary>
        private static void ReturnPercentageOfTax()
        {
            using (var db = new AdWorksContext(_optionsBuilder.Options))
            {
                var pdoductlinesubset = db.SalesOrderHeaders.AsNoTracking().Select(so => new
                {
                    SalesorderId=so.SalesOrderId,
                    CustomerId=so.CustomerId,
                    OrderDate = so.OrderDate,
                    Subtotal = so.SubTotal,
                    TaxRate = Convert.ToDouble(so.TaxAmt*100/so.SubTotal) //considered the faster way than loading in memory and then calculating see msdn "Performance in EF.Core"
                }).OrderBy(x=>x.Subtotal).Where(x => x.TaxRate > 8.1).Take(300).ToList();
            }
        }
        #endregion

        #region uniquejobtitles
        /// <summary>
        /// create a list of unique jobtitles in the employee table in Adventureworks database. Return jobtitle column and arranged the resultset in ascending order.
        /// </summary>
        private static void ReturnUniqueJobTitles() 
        {
            using (var db = new AdWorksContext(_optionsBuilder.Options))
            {
                var prodlinesubset = db.Employees.AsNoTracking().Select(em => em.JobTitle).Distinct().OrderBy(x=>x).Take(200).ToList();
            }
        }
        #endregion

        #region calculate the total fregith
        /// <summary>
        /// calculate the total freight paid by each customer. Return customerid and total freight. Sort the output in ascending order on customerid.
        /// </summary>
        private static void ReturnTotalFreight()
        {
            using (var db = new AdWorksContext(_optionsBuilder.Options))
            {
                var prodlinesubset = db.SalesOrderHeaders.GroupBy(order => order.CustomerId, order => order.Freight, (customer, totalfreight) => new
                {
                    Key = customer,
                    Total = totalfreight.Sum()
                }).OrderBy(order=>order.Key);
            }
        }
        #endregion

        #region ConsoleOutputFunctions
        private static string GetSalespersonDetail(SalesPerson sp)
        {
            return $"ID: {sp.BusinessEntityId}\t|TID: {sp.TerritoryId}\t|Quota:{sp.SalesQuota}\t" +
            $"|Bonus: {sp.Bonus}\t|YTDSales: {sp.SalesYtd}\t|Name: \t" +
            $"{sp.BusinessEntity?.BusinessEntity?.FirstName ?? ""}, " +
            $"{sp.BusinessEntity?.BusinessEntity?.LastName ?? ""}";
        }

        private static string GetEmployeeDetail(Employee em)
        {
            return $"NationalID: {em.NationalIdnumber}\t|BirthDate: {em.BirthDate}\t|BEnId:{em.BusinessEntityId}\t" +
            $"|JobTitle: {em.JobTitle}\t|HireDate: {em.HireDate}\t|";
        }
        #endregion
    }
}
