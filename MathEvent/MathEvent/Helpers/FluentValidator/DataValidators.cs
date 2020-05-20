using FluentValidation;
using MathEvent.Models;
using MathEvent.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MathEvent.Helpers.FluentValidator
{
    public class PerformanceValidator : AbstractValidator<PerformanceViewModel>
    {
        private readonly ApplicationContext _db;
        public PerformanceValidator(ApplicationContext db)
        {
            _db = db;

            RuleFor(p => p.Name)
                .NotNull().WithMessage("Введите название")
                .NotEmpty().WithMessage("Название не должно быть пустым")
                .MinimumLength(3).WithMessage("Минимальная длина 3 символа")
                .MaximumLength(400).WithMessage("Маскимальная длина 400 символов");

            RuleFor(p => p.Type)
                .NotNull().WithMessage("Выберите тип")
                .NotEmpty().WithMessage("Выберите тип");

            RuleFor(p => p.KeyWords)
                .MinimumLength(3).WithMessage("Минимальная длина 3 символа")
                .MaximumLength(250).WithMessage("Маскимальная длина 250 символов");

            RuleFor(p => p.Annotation)
                .NotNull().WithMessage("Введите описание")
                .NotEmpty().WithMessage("Описание не должно быть пустым")
                .MinimumLength(3).WithMessage("Минимальная длина 3 символа")
                .MaximumLength(400).WithMessage("Маскимальная длина 400 символов");

            RuleFor(p => p.Location)
                .NotNull().WithMessage("Введите адрес")
                .NotEmpty().WithMessage("Адрес не должен быть пустым")
                .MinimumLength(3).WithMessage("Минимальная длина 3 символа")
                .MaximumLength(400).WithMessage("Маскимальная длина 400 символов");

            RuleFor(p => p.Start)
                .NotNull().WithMessage("Введите дату")
                .NotEmpty().WithMessage("Дата не должна быть пустой")
                .GreaterThan(DateTime.Now).WithMessage("Дата меньше текущей")
                .Must((fooArgs, start) => IsCorrectStart(fooArgs.SectionId, start)).WithMessage("Дата выходит за рамки секции");
        }

        private bool IsCorrectStart(int? sectionId, DateTime start)
        {
            if (sectionId == null)
            {
                return true;
            }

            var section = _db.Sections.Where(s => s.Id == sectionId).SingleOrDefault();

            if (section != null)
            {
                return start >= section.Start && start <= section.End;
            }

            return false;
        }
    }
}
