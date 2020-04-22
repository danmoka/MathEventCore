using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MathEvent.Helpers.Email;
using MathEvent.Models;
using MathEvent.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MathEvent.Controllers
{
    public class EmailController : Controller
    {
        private readonly ApplicationContext _db;
        private readonly EmailSender _emailSender;

        public EmailController(ApplicationContext db, EmailConfiguration er)
        {
            _emailSender = new EmailSender(er);
            _db = db;
        }

        public async Task<IActionResult> Index(int performanceId)
        {
            var performance = await _db.Performances.Where(p => p.Id == performanceId)
                .Include(p => p.Creator)
                .SingleOrDefaultAsync();

            if (performance == null)
            {
                return RedirectToAction("Error500", "Error");
            }

            var email = performance.Creator.UserName;

            var emailSendMessageViewModel = new EmailSendMessageViewModel
            {
                PerformanceId = performanceId,
                CreatorEmail = email
            };

            return View(emailSendMessageViewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Send(EmailSendMessageViewModel model)
        {
            var creatorEmail = model.CreatorEmail;
            var content = model.Content;
            var userEmail = model.UserEmail;

            var userMessage = model.Message;
            var performance = await _db.Performances.Where(p => p.Id == model.PerformanceId).SingleOrDefaultAsync();
            var message = $"Сообщение от пользователя MathEvent!\n";
            
            if (performance != null)
            {
                message += $"Отправлено со страницы события: \"{performance.Name}\"\n";
            }

            message += $"Сообщение: {userMessage}\n";
            message += $"Для ответа пользователю используйте: {userEmail}\n";
            message += $"На данное сообщение не отвечайте!";

            try
            {
                var emailMessage = new Message(new string[] { creatorEmail }, content, message);
                await _emailSender.SendEmailAsync(emailMessage);
            }
            catch
            {
                return RedirectToAction("Error500", "Error");
            }

            return RedirectToAction("Card", "Performance", new { id = model.PerformanceId });
        }
    }
}