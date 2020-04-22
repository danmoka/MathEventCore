using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MathEvent.Helpers.Email
{
    interface IEmailSender
    {
        Task SendEmailAsync(Message message);
    }
}
