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

namespace MathEvent.Controllers
{
    /// <summary>
    /// Контроллер действий с аккаунтом
    /// </summary>
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ApplicationContext _db;
        /// <summary>
        /// Сервис отправки сообщений по электронной почте
        /// </summary>
        private readonly EmailService _emailSender;

        public AccountController(UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager, ApplicationContext db, EmailConfiguration ec)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _db = db;
            _emailSender = new EmailService(ec);
        }

        /// <summary>
        /// Возвращает страницу кабинета 
        /// </summary>
        /// <returns>Страница кабинета</returns>
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
                }

                ModelState.AddModelError(string.Empty, "Неудачная попытка регистрации");
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
        public async Task<IActionResult> Login(
            [Bind("UserName", "Password", "RememberMe", "ReturnUrl")] LoginViewModel model)
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

        /// <summary>
        /// Возвращает страницу редактирования данных "о себе"
        /// </summary>
        /// <returns>Страница редактирования данных "о себе"</returns>
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Edit()
        {
            var model = new AccountViewModel();
            var user = await _userManager.GetUserAsync(User);

            if (user != null)
            {
                model.Name = user.Name;
                model.Surname = user.Surname;
                model.UserInfo = user.Info;
            }

            return View(model);
        }

        /// <summary>
        /// Изменяет данные пользователя
        /// </summary>
        /// <param name="model">Аккаунт модель данных пользователя</param>
        /// <returns>Кабинет пользователя или страница редактирования данных "о себе", если модель не валидна</returns>
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            [Bind("Name", "Surname", "UserInfo", "ReturnUrl")]AccountViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);
                
                if (user != null)
                {
                    user.Name = model.Name;
                    user.Surname = model.Surname;
                    user.Info = model.UserInfo;
                    await _userManager.UpdateAsync(user);

                    return RedirectToAction("Index", "Account");
                }   
            }

            return View(model);
        }

        /// <summary>
        /// Возвращает страницу ввода Email для смены пароля
        /// </summary>
        /// <returns>Страница ввода Email</returns>
        [HttpGet]
        [AllowAnonymous]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        /// <summary>
        /// Отправляет сообщение о смене пароля на электронную почту пользователя
        /// </summary>
        /// <param name="model">Модель, создержащая Email пользователя</param>
        /// <returns>Информационное представление, которое сообщает о том, 
        ///             что нужно проверить электронную почту</returns>
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword([Bind("Email")] ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ModelState.AddModelError(string.Empty, "Проверьте введенные данные");

                return View(model);
            }

            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user == null)
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

        /// <summary>
        /// Предоставляет страницу ввода нового пароля
        /// </summary>
        /// <param name="code">Код, отправленный на электронную почту пользователя</param>
        /// <returns>Страница ввода нового пароля</returns>
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

        /// <summary>
        /// Меняет пароль пользователя
        /// </summary>
        /// <param name="model">Модель данных для смены пароля</param>
        /// <returns>Если пользователь не обнаружен, то возвращаем страницу о том, что пароль сброшен.
        ///             Это не позволит злоумышленнику узнать о том какие email'ы есть на сайте</returns>
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

            ModelState.AddModelError(string.Empty, "Не удалось сменить пароль");

            return View(model);
        }

    }
}