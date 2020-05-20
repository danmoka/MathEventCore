using MathEvent.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MathEvent.Helpers.Db
{
    public class DbService
    {
        private readonly ApplicationContext _db;

        public DbService(ApplicationContext db)
        {
            _db = db;
        }

        public Section GetSection(int id)
        {
            return _db.Sections.Where(s => s.Id == id).SingleOrDefault();
        }

        public List<Section> GetSections()
        {
            return _db.Sections.ToList();
        }
    }
}
