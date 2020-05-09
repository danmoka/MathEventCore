﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MathEvent.Helpers;
using MathEvent.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Wkhtmltopdf.NetCore;

namespace MathEvent.Controllers
{
    public class PdfController : Controller
    {
        private readonly IGeneratePdf _generatePdf;
        private readonly ApplicationContext _db;
        public PdfController(ApplicationContext db, IGeneratePdf generatePdf)
        {
            _generatePdf = generatePdf;
            _db = db;
        }

        [HttpGet]
        public async Task<IActionResult> GetTimeTable(int conferenceId)
        {
            var conference = await _db.Conferences.Where(c => c.Id == conferenceId).SingleOrDefaultAsync();
            return await _generatePdf.GetPdf(UserDataPathWorker.ConcatPaths(
                UserDataPathWorker.GetPdfTemplatesDirectory(), "ConferenceTimeTable.cshtml"), conference);
        }
    }
}