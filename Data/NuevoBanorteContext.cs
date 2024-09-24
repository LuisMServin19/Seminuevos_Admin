using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Serfitex.Models;
using WebApp.Models;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using Serfitex.Data;
using System.Data;
using WebApp.Models;

namespace Serfitex.Data
{
    public class NuevoBanorteContext : DbContext
    {
        public NuevoBanorteContext(DbContextOptions<NuevoBanorteContext> options)
            : base(options)
        {
        }

        public DbSet<WebApp.Models.TAUsuarios>? TAUsuarios { get; set; }

        public DbSet<WebApp.Models.TAMenu>? TAMenu { get; set; }

        public DbSet<WebApp.Models.TAPerfilMenu>? TAPerfilMenu { get; set; }

    }
}