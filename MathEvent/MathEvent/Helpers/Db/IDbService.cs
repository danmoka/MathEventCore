using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MathEvent.Helpers.Db
{
    interface IDbService
    {
        object GetObjectById(string tableName, string columnName, int id);
    }
}
