using BlazorInputFile;
using MathEvent.Models;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MathEvent.Helpers;
using Microsoft.EntityFrameworkCore;
using System.IO;

namespace MathEvent.Helpers.File
{
    public class FileUpload : IFileUpload
    {
        private readonly ApplicationContext _db;

        public FileUpload(ApplicationContext db)
        {
            _db = db;
        }
        public async Task UploadImageForPerformanceAsync(IFileListEntry file, int performanceId)
        {
            var performance = await _db.Performances.Where(p => p.Id == performanceId).SingleOrDefaultAsync();

            if (performance != null)
            {
                if (file != null)
                {
                    var fileName = file.Name;

                    if (fileName.Length > 30)
                    {
                        var name = Path.GetFileNameWithoutExtension(fileName);
                        name = name.Substring(0, 30);
                        var ext = Path.GetExtension(fileName);
                        fileName = name + ext;
                    }

                    performance.PosterName = fileName;                }
                else
                {
                    performance.PosterName = Path.GetFileName(UserDataPathWorker.GetDefaultImagePath());
                }

                await UserDataPathWorker.UploadImage(file, performance.DataPath, performance.PosterName);
                await _db.SaveChangesAsync();
            }
        }

        public async Task UploadFileAsync(IFileListEntry file, int performanceId)
        {
            var performance = await _db.Performances.Where(p => p.Id == performanceId).SingleOrDefaultAsync();

            if (performance != null)
            {
                if (file != null)
                {
                    var fileName = file.Name;

                    if (fileName.Length > 30)
                    {
                        var name = Path.GetFileNameWithoutExtension(fileName);
                        name = name.Substring(0, 30);
                        var ext = Path.GetExtension(fileName);
                        fileName = name + ext;
                    }

                    performance.ProceedingsName = fileName;
                    await UserDataPathWorker.UploadFile(file, performance.DataPath, performance.ProceedingsName);
                }
            }
        }
    }
}
