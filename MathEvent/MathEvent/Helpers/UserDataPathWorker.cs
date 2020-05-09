using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Formats;

namespace MathEvent.Helpers
{
    public static class UserDataPathWorker
    {
        private static string _webRootPath;
        private static string _mainUserDirectory = "UserData";
        private static string _defaultImageName = "default.png";
        private static string _slash = "/";
        private static string _imageDirectory = "Images";
        private static string _instructionsDirectory = "Images/Instructions";
        private static string _emailTemplatesDirectory = "EmailTemplate";
        private static string _pdfTemplatesDirectory = "wwwroot/PdfTemplate";
        private static int _imageWidth = 900;
        private static int _imageHeight = 600;

        public static void Init(IWebHostEnvironment webHostEnvironment)
        {
            _webRootPath = webHostEnvironment.WebRootPath.Replace("\\", "/");
        }

        /// <summary>
        /// Создает путь, по которому будут храниться файлы пользователя
        /// </summary>
        /// <param name="userId">Id пользователя</param>
        /// <returns>Путь, по которому будут храниться файлы пользователя</returns>
        /// <exception cref="System.ArgumentException">Если входной параметр равен null</exception>
        public static string CreateNewUserPath(string userId)
        {
            if (userId == null)
            {
                throw new System.ArgumentException("argument is null", "userId");
            }

            return ConcatPaths(_mainUserDirectory, userId);
        }

        /// <summary>
        /// Создает папку по указанному пути
        /// </summary>
        /// <param name="path">Путь, по которому создается папка</param>
        /// <returns>true, если папка создалась, false иначе</returns>
        /// <exception cref="System.ArgumentException">Если входной параметр равен null</exception>
        public static bool CreateDirectory(string path)
        {
            if (path == null)
            {
                throw new System.ArgumentException("argument is null", "path");
            }

            var userTrueDirPath = ConcatPaths(_webRootPath, path);             
            DirectoryInfo directoryInfo = new DirectoryInfo(userTrueDirPath);

            if (!directoryInfo.Exists)
            {
                try
                {
                    directoryInfo.Create();
                }
                catch (IOException)
                {
                    return false;
                }
                

                return true;
            }
            else
            {
                return false;
            }

        }

        /// <summary>
        /// Создает папку в указанной директории
        /// </summary>
        /// <param name="parentDir">Директория</param>
        /// <param name="newDirName">Имя новой папки</param>
        /// <returns>true, если папка создалась, false иначе</returns>
        /// <exception cref="System.ArgumentException">Если какой-либо из входных параметров равен null</exception>
        public static bool CreateSubDirectory(ref string parentDir, string newDirName)
        {
            if (parentDir == null)
            {
                throw new System.ArgumentException("argument is null", "parentDir");
            }

            if (newDirName == null)
            {
                throw new System.ArgumentException("argument is null", "newDirName");
            }

            parentDir = ConcatPaths(parentDir, newDirName);
            return CreateDirectory(parentDir);
        }

        public static async Task UploadFile(IFormFile file, string path, string fileName)
        {
            if (file != null)
            {
                using var fileStream = new FileStream(GetRootPath(Path.Combine(path, fileName)), FileMode.Create);
                await file.CopyToAsync(fileStream);
            }
        }

        public static async Task UploadImage(IFormFile file, string path, string fileName)
        {
            if (file != null)
            {
                using var outStream = new FileStream(GetRootPath(Path.Combine(path, fileName)), FileMode.Create);
                var inStream = file.OpenReadStream();
                var image = Image.Load(inStream, out IImageFormat format);

                if (image.Size().Width > _imageWidth || image.Size().Height > _imageHeight)
                {
                    image.Mutate(x => x.Resize(_imageWidth, _imageHeight));
                }

                image.Save(outStream, format);
            }
            else
            {
                using var deafaultImg = new FileStream(GetRootPath(GetDefaultImagePath()), FileMode.Open);
                using var fileStream = new FileStream(GetRootPath(Path.Combine(path, GetDefaultImagePath())), FileMode.Create);
                await deafaultImg.CopyToAsync(fileStream);
            }
        }

        /// <summary>
        /// Возращает полный путь до папки
        /// </summary>
        /// <param name="path">Относительный путь до папки</param>
        /// <returns>Полный до папки</returns>
        /// <exception cref="System.ArgumentException">Если входной параметр равен null</exception>
        public static string GetRootPath(string path)
        {
            if (path == null)
            {
                throw new System.ArgumentException("argument is null", "path");
            }

            return ConcatPaths(_webRootPath, path);
        }

        public static string GetDefaultImagePath()
        {
            return _defaultImageName;
        }

        public static string ConcatPaths(string path1, string path2)
        {
            return $"{path1}{_slash}{path2}";
        }

        public static string GetImagesDirectory()
        {
            return _imageDirectory;
        }

        public static string GetInstructionsDirectory()
        {
            return _instructionsDirectory;
        }

        public static string GetEmailTemplatesDirectory()
        {
            return _emailTemplatesDirectory;
        }

        public static string GetPdfTemplatesDirectory()
        {
            return _pdfTemplatesDirectory;
        }
    }
}
