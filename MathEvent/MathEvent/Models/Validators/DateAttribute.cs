using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace MathEvent.Models.Validators
{
    public class DateAttribute : ValidationAttribute
    {
        public DateAttribute()
        {
            ErrorMessage = "Имя и пароль не должны совпадать!";
        }
        public override bool IsValid(object value)
        {
            Section p = value as Section;

            if (p.Start > p.End)
            {
                return false;
            }
            return true;
        }
    }
}
