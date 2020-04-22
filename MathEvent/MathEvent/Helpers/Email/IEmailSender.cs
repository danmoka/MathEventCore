using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MathEvent.Helpers.Email
{
    interface IEmailSender
    {
        void SendEmail(Message message);
    }
}
