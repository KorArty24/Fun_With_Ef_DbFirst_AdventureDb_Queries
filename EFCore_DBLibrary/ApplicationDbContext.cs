using Microsoft.EntityFrameworkCore;
using System;

namespace EFCore_DBLibrary
{
    public class ApplicationDbContext : DbContext
    {
        //Add a default constructor if scaffolding is needed
        public ApplicationDbContext() { }

        //Add the complex constructor for allowing Dependency Injection
        public ApplicationDbContext(DbContextOptions options)
            : base(options)
        {
            //intentionally empty. 
        }

    }
}
