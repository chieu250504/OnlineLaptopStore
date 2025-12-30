using Laptop88_3.Data;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Security.Claims;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;


namespace Laptop88_3
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            AntiForgeryConfig.UniqueClaimTypeIdentifier = ClaimTypes.NameIdentifier;
            // ✅ Sử dụng DropCreateDatabaseAlways để test Seed
            Database.SetInitializer(new DbInitializer());

            // ✅ Ép EF chạy Seed ngay
            using (var context = new Laptop88_3.Models.AppDbContext())
            {
                context.Database.Initialize(force: true);
            }

        }
    }
}
