using Microsoft.AspNetCore.Hosting;
using System;
using System.Collections.Generic;
using System.IO;

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

        public static string GetEmailTemplateDirectory()
        {
            return _emailTemplatesDirectory;
        }
    }
}
