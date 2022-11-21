# Fun_With_Ef_DbFirst_AdventureDb_Queries
Strictly speaking, this is still CodeFirst, /DbFirst idiomatically speaking. / 
This is the reversed engineered version of the Micosoft's demo AdventureWorks database. I reversed engineered it to create Entity Framework model and DbContext. 
The aim of that is to create a sandbox for writing and experimenting with dbqueries in EntityFramework and LINQ.
Download your own copy of AdventureWorks, restore it to SQL Serv database, adjust the connection string and play. 
Output of the queries to the console is not working. Instead, I start a function from Main and check the result through Visual Studio's build-in IEnumerable viewer. 
For some SQL queries (especially Views and analytical functions: rollup, etc) it is not quite easy to find their counterparts in LINQ2Db +EF. Perhaps it is better to stick to the raw SQL queries in such cases. 
