using System;
using System.Collections.Generic;
using System.Linq;
using MathEvent.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace MathEvent.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FilterController : ControllerBase
    {
        [HttpPost]
        [Route("type")]
        public FilterCardViewModel FilterByType(
            [Bind("Cards", "FilterParameter")] FilterCardViewModel filterViewModel)
        {
            if (!ModelState.IsValid)
            {
                filterViewModel.Cards = new List<PerformanceViewModel>();

                return filterViewModel;
            }

            filterViewModel.Cards = filterViewModel.Cards.Where(c => c.Type == filterViewModel.FilterPatameter).ToList();

            if (filterViewModel.Cards == null)
            {
                filterViewModel.Cards = new List<PerformanceViewModel>();
            }

            return filterViewModel;
        }

        [HttpPost]
        [Route("period")]
        public  FilterCardViewModel FilterByPeriod(
            [Bind("Cards", "FilterParameter")] FilterCardViewModel filterViewModel)
        {
            if (!ModelState.IsValid)
            {
                filterViewModel.Cards = new List<PerformanceViewModel>();

                return filterViewModel;
            }

            var period = filterViewModel.FilterPatameter;
            switch (period)
            {
                case "Сегодня":
                    filterViewModel.Cards = filterViewModel.Cards.Where(p => p.Start.Day == DateTime.Now.Day).ToList();
                    break;
                case "В этом месяце":
                    filterViewModel.Cards = filterViewModel.Cards.Where(p => p.Start.Month == DateTime.Now.Month).ToList();
                    break;
                case "В этом году":
                    filterViewModel.Cards = filterViewModel.Cards.Where(p => p.Start.Year == DateTime.Now.Year).ToList();
                    break;
                default:
                    break;
            }

            return filterViewModel;
        }

        [HttpPost]
        [Route("search")]
        public FilterCardViewModel FilterBySerchString(
            [Bind("Cards", "FilterParameter")] FilterCardViewModel filterViewModel)
        {
            if (!ModelState.IsValid)
            {
                filterViewModel.Cards = new List<PerformanceViewModel>();

                return filterViewModel;
            }

            // подумать как лучше сделать если null значения: либо запретить в модели, но + ещё и тут смотреть на всякий случай
            var searchString = filterViewModel.FilterPatameter;
            filterViewModel.Cards = filterViewModel.Cards.Where(c => c.Name.IndexOf(searchString, StringComparison.OrdinalIgnoreCase) >= 0 || 
            (c.KeyWords != null && c.KeyWords.IndexOf(searchString, StringComparison.OrdinalIgnoreCase) >= 0) ||
            c.Location.IndexOf(searchString, StringComparison.OrdinalIgnoreCase) >= 0 ||
            (c.CreatorName != null &&c.CreatorName.IndexOf(searchString, StringComparison.OrdinalIgnoreCase) >= 0)).ToList();

            return filterViewModel;
        }
    }
}