using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MathEvent.Helpers.Email;
using MathEvent.Models;
using MathEvent.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MathEvent.Controllers
{
    [Authorize]
    public class EmailController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationContext _db;
        private readonly EmailSender _emailSender;

        public EmailController(UserManager<ApplicationUser> userManager, ApplicationContext db, EmailConfiguration er)
        {
            _userManager = userManager;
            _emailSender = new EmailSender(er);
            _db = db;
        }

        [HttpGet]
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

            if (performance == null)
            {
                return RedirectToAction("Error500", "Error");
            }

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

        [HttpGet]
        public IActionResult EmailConfirmMessage()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> ConfirmEmailRequest()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return RedirectToAction("Error500", "Error");
            }
            
            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var callbackUrl = Url.Action(
                "ConfirmEmailResponse",
                "Email",
                new { userId = user.Id, code = code },
                protocol: HttpContext.Request.Scheme);

            var emailMessage = new Message(new string[] { user.Email }, "Подтвердите аккаунт",
                $"Подтвердите регистрацию, перейдя по ссылке: <a href='{callbackUrl}'>link</a>");
            await _emailSender.SendEmailAsync(emailMessage);

            return RedirectToAction("EmailConfirmMessage", "Email");
        }

        [HttpGet]
        public async Task<IActionResult> ConfirmEmailResponse(string userId, string code)
        {
            if (userId == null || code == null)
            {
                return RedirectToAction("Error500", "Error");
            }

            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                return RedirectToAction("Error500", "Error");
            }

            var result = await _userManager.ConfirmEmailAsync(user, code);

            if (result.Succeeded)
                return RedirectToAction("Index", "Home");
            else
                return RedirectToAction("Error500", "Error");
        }
    }
}