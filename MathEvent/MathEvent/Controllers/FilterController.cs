using System;
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
            filterViewModel.Cards = filterViewModel.Cards.Where(c => c.Type == filterViewModel.FilterPatameter).ToList();

            return filterViewModel;
        }

        [HttpPost]
        [Route("period")]
        public  FilterCardViewModel FilterByPeriod(
            [Bind("Cards", "FilterParameter")] FilterCardViewModel filterViewModel)
        {
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
            var searchString = filterViewModel.FilterPatameter;
            filterViewModel.Cards = filterViewModel.Cards.Where(c =>
            c.Name.IndexOf(searchString, StringComparison.OrdinalIgnoreCase) >= 0 ||
            c.KeyWords.IndexOf(searchString, StringComparison.OrdinalIgnoreCase) >= 0 ||
            c.Location.IndexOf(searchString, StringComparison.OrdinalIgnoreCase) >= 0 ||
            c.CreatorName.IndexOf(searchString, StringComparison.OrdinalIgnoreCase) >= 0).ToList();

            return filterViewModel;
        }
    }
}