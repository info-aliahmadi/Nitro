using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nitro.Core.Data.Domain;
using Nitro.Infrastructure.Data;

namespace Nitro.Test.Helpers
{
    public static class Utilities
    {
        #region snippet1

        public static void InitializeDbForTests(ApplicationDbContext db)
        {
            db.Category.AddRange(GetSeedingMessages());
            db.SaveChanges();
        }

        public static void ReinitializeDbForTests(ApplicationDbContext db)
        {
            db.Category.RemoveRange(db.Category);
            InitializeDbForTests(db);
        }

        public static List<Category> GetSeedingMessages()
        {
            return new List<Category>()
            {
                new Category() {Title = "Category Test 1"},
                new Category() {Title = "Category Test 2"},
                new Category() {Title = "Category Test 3"}
            };
        }

        #endregion
    }
}
