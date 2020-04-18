using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MathEvent.Models;
using MathEvent.Models.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MathEvent.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FilterController : ControllerBase
    {
        private readonly ApplicationContext _db;

        public FilterController(ApplicationContext db)
        {
            _db = db;
        }

        [HttpPost]
        [Route("type")]
        public async Task<FilterViewModel> FilterByType(FilterViewModel filterViewModel)
        {

            filterViewModel.Cards = filterViewModel.Cards.Where(c => c.Type == filterViewModel.FilterPatameter).ToList();
            //else
            //{
            //    var performances = await _db.Performances
            //        .Include(p => p.Section)
            //        .Include(p => p.Creator).ToListAsync();
            //    filterViewModel.Cards = new List<CardViewModel>();
            //    foreach (var performance in performances)
            //    {
            //        filterViewModel.Cards.Add(new CardViewModel
            //        {
            //            Id = performance.Id,
            //            Name = performance.Name,
            //            Annotation = performance.Annotation,
            //            KeyWords = performance.KeyWords,
            //            Start = performance.Start,
            //            CreatorId = performance.CreatorId,
            //            CreatorName = performance.Creator.Name,
            //            DataPath = performance.DataPath,
            //            PosterName = performance.PosterName,
            //            Traffic = performance.Traffic,
            //            Type = performance.Type
            //        });
            //    }
            //}

            return filterViewModel;
        }

        [HttpPost]
        [Route("period")]
        public async Task<FilterViewModel> FilterByPeriod(FilterViewModel filterViewModel)
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
                    //var performances = await _db.Performances
                    //    .Include(p => p.Section)
                    //    .Include(p => p.Creator).ToListAsync();
                    //filterViewModel.Cards = new List<CardViewModel>();
                    //foreach(var performance in performances)
                    //{
                    //    filterViewModel.Cards.Append(new CardViewModel
                    //    {
                    //        Id = performance.Id,
                    //        Name = performance.Name,
                    //        Annotation = performance.Annotation,
                    //        KeyWords = performance.KeyWords,
                    //        Start = performance.Start,
                    //        CreatorId = performance.CreatorId,
                    //        CreatorName = performance.Creator.Name,
                    //        DataPath = performance.DataPath,
                    //        PosterName = performance.PosterName,
                    //        Traffic = performance.Traffic,
                    //        Type = performance.Type
                    //    });
                    //}
                    break;
            }

            return filterViewModel;
        }
    }
}