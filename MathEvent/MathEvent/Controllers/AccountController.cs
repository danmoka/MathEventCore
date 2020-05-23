using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MathEvent.Helpers;
using MathEvent.Helpers.Email;
using MathEvent.Models;
using MathEvent.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Bcpg;

namespace MathEvent.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ApplicationContext _db;
        private readonly EmailSender _emailSender;

        public AccountController(UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager, ApplicationContext db, EmailConfiguration ec)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _db = db;
            _emailSender = new EmailSender(ec);
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            var conferences = await _db.Conferences.Where(c => c.ManagerId == user.Id)
                .Include(c => c.Manager)
                .Include(c => c.Sections)
                .ThenInclude(s => s.Performances).ToListAsync();

            var performances = await _db.Performances.Where(p => p.CreatorId == user.Id && p.SectionId == null).ToListAsync();
            ViewBag.Performances = performances;

            var subsribedPerformances = await _db.ApplicationUserPerformances
                .Where(ap => ap.ApplicationUserId == user.Id)
                .Include(p => p.Performance).ToListAsync();
            ViewBag.SubscribedPerformances = subsribedPerformances;

            var foreignPerformances = new List<Performance>();
            var foreignSections = _db.Sections.Where(s => s.ManagerId != user.Id)
                .Include(s => s.Performances);

            foreach (var section in foreignSections)
            {
                foreach (var performance in section.Performances)
                {
                    if (performance.CreatorId == user.Id)
                    {
                        foreignPerformances.Add(performance);
                    }
                }
            }

            ViewBag.ForeignPerformances = foreignPerformances;

            return View(conferences);
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(
            [Bind("Email", "Name", "Surname", "Password", "PasswordConfirm")] RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    Name = model.Name,
                    Surname = model.Surname,
                };

                user.DataPath = UserDataPathWorker.CreateNewUserPath(user.Id);
                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    if (!UserDataPathWorker.CreateDirectory(user.DataPath))
                    {
                        await _userManager.DeleteAsync(user);
                        return RedirectToAction("Error500", "Error");
                    }

                    await _userManager.AddToRoleAsync(user, "user");
                    await _signInManager.SignInAsync(user, false);

                    return RedirectToAction("ConfirmEmailRequest", "Email");
                    //return RedirectToAction("Index", "Home");
                }


                foreach (var error in result.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();

            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult Login(string returnUrl = null)
        {
            return View(new LoginViewModel { ReturnUrl = returnUrl });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login([Bind("UserName", "Password", "RememberMe", "ReturnUrl")] LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(model.UserName, model.Password, model.RememberMe, false);

                if (result.Succeeded)
                {
                    if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
                        return Redirect(model.ReturnUrl);

                    return RedirectToAction("Index", "Home");
                }

                ModelState.AddModelError("", "Неправильный логин и (или) пароль");
            }

            return View(model);
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Edit()
        {
            var user = await _userManager.GetUserAsync(User);
            var model = new AccountViewModel
            {
                Name = user.Name,
                Surname = user.Surname,
                Info = user.Info
            };

            return View(model);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit([Bind("Name", "Surname", "UserInfo", "ReturnUrl")]AccountViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction("Error500", "Error");
            }

            var user = await _userManager.GetUserAsync(User);
            // если user null?

            user.Name = model.Name;
            user.Surname = model.Surname;
            user.Info = model.Info;
            await _userManager.UpdateAsync(user);

            if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
                return Redirect(model.ReturnUrl);

            return RedirectToAction("Index", "Account");
        }


        [HttpGet]
        [AllowAnonymous]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword([Bind("Email")] ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction("Error500", "Error");
            }

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null /*|| !(await _userManager.IsEmailConfirmedAsync(user))*/)
            {
                return View("ForgotPasswordConfirmation");
            }

            var body = string.Empty;

            using (StreamReader sr = new StreamReader(
                UserDataPathWorker.GetRootPath(
                    UserDataPathWorker.ConcatPaths(UserDataPathWorker.GetEmailTemplatesDirectory(), "EmailConfirmMessage.html"))))
            {
                body = await sr.ReadToEndAsync();
            }

            var code = await _userManager.GeneratePasswordResetTokenAsync(user);
            var callbackUrl = Url.Action("ResetPassword", "Account", new { userId = user.Id, code = code }, protocol: HttpContext.Request.Scheme);

            body = body.Replace("{Link}", callbackUrl);
            body = body.Replace("{SocialMedia}", "в разработке");
            body = body.Replace("{Content}", "Смена пароля");
            body = body.Replace("{ButtonName}", "Сменить");
            body = body.Replace("{MainText}", "Для смены пароля нажмите кнопку ниже");
            body = body.Replace("{FailText}", "Если сменить пароль не удалось, то скопируйте и вставьте в браузер ссылку ниже:");
            body = body.Replace("{Date}", DateTime.Now.Year.ToString());

            var message = new Message(new string[] { model.Email }, "Смена пароля", body);
            await _emailSender.SendEmailAsync(message);

            return View("ForgotPasswordConfirmation");
            
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ResetPassword(string code = null)
        {
            
            if (code == null)
            {
                return RedirectToAction("Error404", "Error");
            }

            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(
            [Bind("Email", "Password", "PasswordConfirm", "Code")] ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            await _signInManager.SignOutAsync();

            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user == null)
            {
                return View("ResetPasswordConfirmation");
            }

            var result = await _userManager.ResetPasswordAsync(user, model.Code, model.Password);

            if (result.Succeeded)
            {
                return View("ResetPasswordConfirmation");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }

    }
}