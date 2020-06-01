using BlazorInputFile;
using MathEvent.Models;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.IO;

namespace MathEvent.Helpers.File
{
    public class FileUploadService : IFileUpload
    {
        private readonly ApplicationContext _db;

        public FileUploadService(ApplicationContext db)
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
                    if (performance.PosterName != null)
                    {
                        var imageToBeDeleted = UserDataPathWorker.GetRootPath(Path.Combine(performance.DataPath, performance.PosterName));

                        if (System.IO.File.Exists(imageToBeDeleted))
                        {
                            System.IO.File.Delete(imageToBeDeleted);
                        }
                    }

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

                _db.Performances.Update(performance);
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

                    if (!string.IsNullOrEmpty(performance.ProceedingsName))
                    {
                        var filePath = UserDataPathWorker.GetRootPath(UserDataPathWorker.ConcatPaths(performance.DataPath, performance.ProceedingsName));
                        
                        // перенести в UserDataPathWorker
                        if (System.IO.File.Exists(filePath))
                        {
                            try
                            {
                                System.IO.File.Delete(filePath);
                            }
                            catch
                            {
                                return;
                            }

                            performance.ProceedingsName = null;
                        }
                    }

                    performance.ProceedingsName = fileName;
                    await UserDataPathWorker.UploadFile(file, performance.DataPath, performance.ProceedingsName);
                    await _db.SaveChangesAsync();
                }
            }
        }
    }
}
