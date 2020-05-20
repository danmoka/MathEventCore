using BlazorInputFile;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MathEvent.Helpers.File
{
    public interface IFileUpload
    {
        Task UploadImageForPerformanceAsync(IFileListEntry file, int performanceId);
        Task UploadFileAsync(IFileListEntry file, int performanceId);
    }
}
