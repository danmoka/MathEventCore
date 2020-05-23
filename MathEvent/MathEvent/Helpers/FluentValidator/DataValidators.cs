using FluentValidation;
using MathEvent.Models;
using MathEvent.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Data;
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
                .Must((fooArgs, start) => IsCorrectStart(fooArgs.SectionId, start)).WithMessage((fooArgs, start) => CreateErrorMessage(fooArgs.SectionId, start));
        }

        private string CreateErrorMessage(int? sectionId, DateTime start)
        {
            if (sectionId != null)
            {
                var section = _db.Sections.Where(s => s.Id == sectionId).SingleOrDefault();

                if (section != null)
                {
                    return $"Дата выходит за рамки секции: {section.Start} - {section.End}";
                }             
            }

            return "Дата выходит за рамки секции";
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

    public class SectionValidator : AbstractValidator<SectionViewModel>
    {
        private readonly ApplicationContext _db;

        public SectionValidator(ApplicationContext db)
        {
            _db = db;

            RuleFor(s => s.Name)
                .NotNull().WithMessage("Введите название")
                .NotEmpty().WithMessage("Название не должно быть пустым")
                .MinimumLength(3).WithMessage("Минимальная длина 3 символа")
                .MaximumLength(300).WithMessage("Маскимальная длина 300 символов");

            RuleFor(s => s.Location)
                .NotNull().WithMessage("Введите адрес")
                .NotEmpty().WithMessage("Адрес не должн быть пустым")
                .MinimumLength(3).WithMessage("Минимальная длина 3 символа")
                .MaximumLength(400).WithMessage("Маскимальная длина 400 символов");

            RuleFor(s => s.Start)
                .NotNull().WithMessage("Введите дату")
                .NotEmpty().WithMessage("Дата не должна быть пустой")
                .GreaterThan(DateTime.Now).WithMessage("Дата меньше текущей")
                .Must((fooArgs, start) => IsCorrectStart(fooArgs.ConferenceId, start)).WithMessage((fooArgs, start) => CreateErrorMessage(fooArgs.ConferenceId, start))
                .Must((fooArgs, start) => IsStartLessThanEnd(fooArgs.End, start)).WithMessage("Дата начала больше даты конца");

            RuleFor(s => s.End)
                .NotNull().WithMessage("Введите дату")
                .NotEmpty().WithMessage("Дата не должна быть пустой")
                .GreaterThan(DateTime.Now).WithMessage("Дата меньше текущей")
                .Must((fooArgs, end) => IsCorrectEnd(fooArgs.ConferenceId, end)).WithMessage((fooArgs, end) => CreateErrorMessage(fooArgs.ConferenceId, end))
                .Must((fooArgs, end) => IsEndGreaterThanStart(fooArgs.Start, end)).WithMessage("Дата конца меньше даты начала");
        }

        private string CreateErrorMessage(int conferenceId, DateTime time)
        {
            var conference = _db.Conferences.Where(c => c.Id == conferenceId).SingleOrDefault();

            if (conference != null)
            {
                return $"Дата выходит за рамки конференции: {conference.Start} - {conference.End}";
            }

            return "Дата выходит за рамки конференции";
        }

        private bool IsStartLessThanEnd(DateTime end, DateTime start)
        {
            return start < end;
        }

        private bool IsCorrectStart(int conferenceId, DateTime start)
        {
            var conference = _db.Conferences.Where(c => c.Id == conferenceId).SingleOrDefault();

            if (conference != null)
            {
                return start > conference.Start && start < conference.End;
            }

            return false;
        }

        private bool IsEndGreaterThanStart(DateTime start, DateTime end)
        {
            return end > start;
        }

        private bool IsCorrectEnd(int conferenceId, DateTime end)
        {
            var conference = _db.Conferences.Where(c => c.Id == conferenceId).SingleOrDefault();

            if (conference != null)
            {
                return end > conference.Start && end < conference.End;
            }

            return false;
        }
    }

    public class ConferenceValidator : AbstractValidator<ConferenceViewModel>
    {
        public ConferenceValidator()
        {
            RuleFor(c => c.Name)
                .NotNull().WithMessage("Введите название")
                .NotEmpty().WithMessage("Название не должно быть пустым")
                .MinimumLength(3).WithMessage("Минимальная длина 3 символа")
                .MaximumLength(300).WithMessage("Маскимальная длина 300 символов");

            RuleFor(c => c.Location)
                .NotNull().WithMessage("Введите адрес")
                .NotEmpty().WithMessage("Адрес не должн быть пустым")
                .MinimumLength(3).WithMessage("Минимальная длина 3 символа")
                .MaximumLength(400).WithMessage("Маскимальная длина 400 символов");

            RuleFor(c => c.Start)
                .NotNull().WithMessage("Введите дату")
                .NotEmpty().WithMessage("Дата не должна быть пустой")
                .GreaterThan(DateTime.Now).WithMessage("Дата меньше текущей")
                .Must((fooArgs, start) => IsStartLessThanEnd(fooArgs.End, start)).WithMessage("Дата начала больше даты конца");

            RuleFor(c => c.End)
                .NotNull().WithMessage("Введите дату")
                .NotEmpty().WithMessage("Дата не должна быть пустой")
                .GreaterThan(DateTime.Now).WithMessage("Дата меньше текущей")
                .Must((fooArgs, end) => IsEndGreaterThanStart(fooArgs.Start, end)).WithMessage("Дата конца меньше даты начала");
        }

        private bool IsStartLessThanEnd(DateTime end, DateTime start)
        {
            return start < end;
        }

        private bool IsEndGreaterThanStart(DateTime start, DateTime end)
        {
            return end > start;
        }
    }
}
