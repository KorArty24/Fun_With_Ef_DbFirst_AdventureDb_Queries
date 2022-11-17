// See https://aka.ms/new-console-template for more information
using EFCore_DBLibrary;
using FunWithEFAdventureReverse.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;

namespace FunWithEFAdventureReverse {
    internal class Program
    {
        private static DbContextOptionsBuilder<AdWorksContext> _optionsBuilder;
        private static IConfigurationRoot _configuration;

        static void Main(string[] args)
        {
            var dict = new Dictionary<int, Action>();
            BuildConfiguration();
            BuildOptions();
            registerMethods(ref dict);
            returnAction(dict, dict.Count).Invoke();

        }

        private static void registerMethods(ref Dictionary<int, Action> _dict)
        {
            _dict.Add(1, () => ListAllSalesMen());
            _dict.Add(2, () => ReturnUniqueJobTitles());
            _dict.Add(3, () => ListPeople());
            _dict.Add(4, () => ListEmployee());
            _dict.Add(5, () => ListAllSalesMen());
            _dict.Add(6, () => ReturnUniqueJobTitles());
            _dict.Add(7, () => ReturnTotalFreight());
            _dict.Add(8, () => ReturnSubtotal());
            _dict.Add(9, () => ReturnProductionInventory());
            _dict.Add(10, () => ReturnLastNamesWith_L());
            _dict.Add(11, () => FindTheSumOfSubtotal());
            _dict.Add(12, () => FindTheEmployeesForEachCity());
            _dict.Add(13, () => FindTotalSalesForEachYear());
            _dict.Add(14, () => FindTotalSalesForEachYearFilteredByYear());
            _dict.Add(15, () => FindManagersInEachDepartment());
            _dict.Add(16, () => CompoundSelectWithMultipleWhere());
            _dict.Add(18, () => FindTheComboContactTypeName());
            _dict.Add(17, () => NestedGroupQueries());
        }

        static Action returnAction(IDictionary<int, Action> dic, int key)
        {
            return dic[key];
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
                var salespeople = db.SalesPeople.Include(x => x.BusinessEntity).ThenInclude(y => y.BusinessEntity).ThenInclude(be => be.Customers).
                    AsNoTracking().Select(e => new
                    {
                        entId = e.BusinessEntityId,
                        addconin = e.BusinessEntity.BusinessEntity.AdditionalContactInfo,
                        cust = e.BusinessEntity.BusinessEntity.Customers.ToList(),
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
                var pdoductlinesubset = db.Products.Where(pr => !pr.SellStartDate.Equals(null)).Select(p => new
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
                    SalesorderId = so.SalesOrderId,
                    CustomerId = so.CustomerId,
                    OrderDate = so.OrderDate,
                    Subtotal = so.SubTotal,
                    TaxRate = Convert.ToDouble(so.TaxAmt * 100 / so.SubTotal) //considered the faster way than loading in memory and then calculating see msdn "Performance in EF.Core"
                }).OrderBy(x => x.Subtotal).Where(x => x.TaxRate > 8.1).Take(300).ToList();
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
                var prodlinesubset = db.Employees.AsNoTracking().Select(em => em.JobTitle).Distinct().OrderBy(x => x).Take(200).ToList();
            }
        }
        #endregion

        #region average_sum_and_subtotal
        /// <summary>
        /// find the average and the sum of the subtotal for every customer.
        /// Return customerid, average and sum of the subtotal. 
        /// Grouped the result on customerid and salespersonid.
        /// Sort the result on customerid column in descending order.
        /// </summary>
        private static void ReturnSubtotal()
        {
            using (var db = new AdWorksContext(_optionsBuilder.Options))
            {
                var result = from so in db.SalesOrderHeaders.AsNoTracking()
                             group so by new
                             {
                                 CusId = so.CustomerId,
                                 SaOPersonId = so.SalesPersonId
                             } into sgroup
                             orderby sgroup.Key.SaOPersonId descending
                             select new
                             {
                                 CusId = sgroup.Key.CusId,
                                 SaOPersonId = sgroup.Key.SaOPersonId,
                                 Subtotal = sgroup.Sum(x => x.SubTotal),
                                 Average = sgroup.Average(x => x.SubTotal)
                             };
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
                }).OrderBy(order => order.Key);
            }
        }
        #endregion

        #region get ACH more 500
        /// <summary>
        /// retrieve total quantity of each productid which are in shelf of 'A' or 'C' or 'H'. 
        /// Filter the results for sum quantity is more than 500. Return productid and sum of the quantity
        /// </summary>
        private static void ReturnProductionInventory()
        {
            using (var db = new AdWorksContext(_optionsBuilder.Options))
            {
                char[] headers = { 'A', 'C', 'H' };
                string[] dord = { "A", "C", "S" };
                var cond = dord.Where(x => x.StartsWithAnyFromArr(headers));
                ////used extension method to generilize solution and avoid many &&
                // var result = db.ProductInventories.AsNoTracking().Select(x=>x.Shelf.StartsWithAnyFromArr(headers));
                var result = db.ProductInventories.Where(p => p.Shelf.Equals("A") || p.Shelf.Equals("C") || p.Shelf.Equals("H")).
                    GroupBy(x => x.ProductId, x => x.Quantity, (prodid, quanty) => new
                    {
                        Key = prodid,
                        totalquantity = quanty.Sum(a => (decimal)a)
                    }).Where(x => x.totalquantity > 500).Select(x => new
                    {
                        ProductId = x.Key,
                        Total = x.totalquantity
                    }).ToList();

            }
        }
        #endregion

        #region FindLinLastName
        /// <summary>
        /// find the persons whose last name starts with letter 'L'. Return BusinessEntityID, FirstName, LastName, and PhoneNumber.
        /// Sort the result on lastname and firstname
        /// </summary>
        private static void ReturnLastNamesWith_L()
        {
            using (var db = new AdWorksContext(_optionsBuilder.Options))
            {
                string schar = "L";
                var query = db.People.Include(p => p.PersonPhones).AsNoTracking().Where(x => x.LastName.StartsWith(schar)).Select(per => new
                {
                    BusinessEntityId = per.BusinessEntityId,
                    FirstName = per.FirstName,
                    LastName = per.LastName,
                    PhoneNum = per.PersonPhones.Select(x => x.PhoneNumber).FirstOrDefault()
                }).OrderBy(x => x.LastName).ThenBy(x => x.FirstName).ToList();
            }
        }
        #endregion

        #region FindTheSumOfSubtotal
        /// <summary>
        ///  find the sum of subtotal column. Group the sum on distinct salespersonid and customerid. 
        ///  Rolls up the results into subtotal and running total. 
        ///  Return salespersonid, customerid and sum of subtotal column i.e. sum_subtotal. 
        /// </summary>
        private static void FindTheSumOfSubtotal()
        {
            using (var db = new AdWorksContext(_optionsBuilder.Options))
            {
                var query = from header in db.SalesOrderHeaders.AsNoTracking()
                            group header by new
                            {
                                SalesPersonId = header.SalesPersonId,
                                CustomerId = header.CustomerId
                            } into salesgroup
                            select new
                            {
                                SalesPersonId = salesgroup.Key.SalesPersonId,
                                CustomerId = salesgroup.Key.CustomerId,
                                SumSubtotal = salesgroup.Sum(x => x.SubTotal)
                            };
                var result = query.ToList();
            }
        }
        #endregion

        #region FindTheEmployeesForEachCity
        /// <summary>
        /// write a query to retrieve the number of employees for each City.
        /// Return city and number of employees. Sort the result in ascending order on city
        /// </summary>
        private static void FindTheEmployeesForEachCity()
        {

            using (var db = new AdWorksContext(_optionsBuilder.Options))
            {
                var query = from address in db.BusinessEntityAddresses.AsNoTracking().Include(a => a.Address)
                            group address by new
                            {
                                City = address.Address.City,
                            } into addrgroup
                            select new
                            {
                                City = addrgroup.Key.City,
                                NumOfEmployees = addrgroup.Select(x => x.Address.City).Count()
                            };
                var result = query.ToList();
            }
        }
        #endregion

        #region FindTotalSalesFoeEachYear
        /// <summary>
        /// write a query in SQL to retrieve the total sales for each year. Return the year part of order date and total due amount. 
        /// Sort the result in ascending order on year part of order date
        /// </summary>
        private static void FindTotalSalesForEachYear()
        {

            using (var db = new AdWorksContext(_optionsBuilder.Options))
            {
                var query = db.SalesOrderHeaders.AsNoTracking().GroupBy(s => s.OrderDate.Year, s => s.TotalDue, (year, total) => new
                {
                    Year = year,
                    OrderAmount = total.Sum()
                }).Select(x => new
                {
                    Year = x.Year,
                    OrderAmount = x.OrderAmount
                }).OrderBy(x => x.Year).ToList();

            }
        }
        #endregion

        #region FindTotalSalesFoEachYearFiltered
        /// <summary>
        /// write a query to retrieve the total sales for each year. to retrieve the total sales for each year. 
        /// Filter the result set for those orders where order year is on or before 2016.
        /// Return the year part of orderdate and total due amount. Sort the result in ascending order on year part of order date.
        /// </summary>
        private static void FindTotalSalesForEachYearFilteredByYear()
        {

            using (var db = new AdWorksContext(_optionsBuilder.Options))
            {
                var query = db.SalesOrderHeaders.AsNoTracking().Where(x => x.OrderDate.Year <= 2016).GroupBy(s => s.OrderDate.Year, s => s.TotalDue, (year, total) => new
                {
                    Year = year,
                    OrderAmount = total.Sum()
                }).Select(x => new
                {
                    Year = x.Year,
                    OrderAmount = x.OrderAmount
                }).OrderBy(x => x.Year).ToList();

            }
        }
        #endregion

        #region FindManagersInEachDepartment()
        /// <summary>
        ///  to find the contacts who are designated as a manager in various departments.
        ///  Returns ContactTypeID, name. Sort the result set in descending order.
        /// </summary>
        private static void FindManagersInEachDepartment()
        {
            using (var db = new AdWorksContext(_optionsBuilder.Options))
            {
                var query = db.ContactTypes.Where(x => x.Name.Contains("Manager")).Select(per => new
                {
                    ContactTypeId = per.ContactTypeId,
                    Name = per.Name,
                    ModifiedDate = per.ModifiedDate
                }).OrderByDescending(x=>x.ModifiedDate).ToList();
            }
        }
            #endregion

                #region FindManagersInEachDepartment()
        /// <summary>
        ///  write a query in SQL to make a list of contacts who are designated as 'Purchasing Manager'.
        ///  Return BusinessEntityID, LastName, and FirstName columns. Sort the result set in ascending order of LastName, and FirstName.
        /// </summary>
        private static void FindPurchasingManagers()
        {
            using (var db = new AdWorksContext(_optionsBuilder.Options))
            {
                var query = db.BusinessEntityContacts.AsNoTracking().Include(x => x.Person).Include(x=>x.ContactType).Where(x => x.ContactType.Name.Contains("Purchasing Manager")).
                    OrderBy(x => x.Person.FirstName).ThenBy(x => x.Person.LastName).
                    Select(per => new
                    {
                        BusinessEntityId = per.BusinessEntityId,
                        Last_Name = per.Person.LastName,
                        First_Name = per.Person.FirstName
                    }).ToList();
            }
        }
            #endregion

        #region FindManagersInEachDepartment()
        /// <summary>
        ///  to find the contacts who are designated as a manager in various departments.
        ///  Returns ContactTypeID, name. Sort the result set in descending order.
        /// </summary>
        private static void FindSales()
        {
            using (var db = new AdWorksContext(_optionsBuilder.Options))
            {
                var query = db.BusinessEntityContacts.AsNoTracking().Include(x => x.Person).Include(x=>x.ContactType).Where(x => x.ContactType.Name.Contains("Purchasing Manager")).OrderBy(x => x.Person.FirstName).ThenBy(x => x.Person.LastName).
                    Select(per => new
                    {
                        BusinessEntityId = per.BusinessEntityId,
                        PersonId = per.PersonId,
                        rowguid = per.Rowguid,
                        ModifiedDate = per.ModifiedDate
                    }).ToList();
            }
        }
        #endregion

        #region FindTheComboContactTypeName
        /// <summary>
        /// From the following table write a query in SQL to count the number of contacts for combination of each type and name. 
        /// Filter the output for those who have 100 or more contacts. 
        /// Return ContactTypeID and ContactTypeName and BusinessEntityContact.
        /// Sort the result set in descending order on number of contacts.
        /// </summary>
        private static void FindTheComboContactTypeName()
        {
            using (var db = new AdWorksContext(_optionsBuilder.Options))
            {
                var query = from ec in db.BusinessEntityContacts.AsNoTracking().Include(c=>c.ContactType)
                            group ec by new
                            {
                                ContactTypeId = ec.ContactType.ContactTypeId,
                                Name = ec.ContactType.Name
                            } into grp
                            where grp.Count() >100
                            select new
                            {
                                Key = grp.Key.ContactTypeId,
                                Key2 = grp.Key.Name,
                                Count = grp.Count()
                            };
                var res = query.ToList();
            }
        }
        #endregion

        #region LINQ101PostalCode() 
        /// <summary>
        ///  
        /// </summary>
        private static void NestedGroupQueries()
        {
            using (var db = new AdWorksContext(_optionsBuilder.Options))
            {
                var customerOrderGroups = from p in db.People.AsNoTracking().AsEnumerable()
                                          select (
                                          p.LastName,
                                          TypeGroups: from cmr in p.Customers
                                                      group cmr by cmr.Store.Name into gr
                                                      select (
                                                          Name: gr.Key,
                                                          ValGroups: from x in gr
                                                                     group x by x.Store.Name into ng
                                                                     select (Key: ng.Key, Val: ng)));
                var dump = customerOrderGroups.ToList();
            }
        }
        #endregion

        #region LINQ101 CompoundSelectWithMultipleWhere
        /// <summary>
        /// Some inherintedly meaningless query just for the sake of syntaxic procedure drilling.
        /// </summary>
        private static void CompoundSelectWithMultipleWhere()
        {
            using (var db = new AdWorksContext(_optionsBuilder.Options))
            {
                var query = from entity in db.BusinessEntities.AsNoTracking()
                            where entity.BusinessEntityId > 50 && entity.BusinessEntityId < 60
                            from p in db.People.AsNoTracking()
                            where p.LastName.Contains("W")
                            select new
                            {
                                EntityId = entity.BusinessEntityId,
                                CustomersCount = p.Customers.Count()
                            };
                query.ToList();
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

