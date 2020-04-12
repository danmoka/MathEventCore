using Microsoft.AspNetCore.Hosting;
using System.IO;

namespace MathEvent.Helpers
{
    public static class UserDataPathWorker
    {
        private static IWebHostEnvironment _webHostEnvironment;
        private static string _mainUserDirectory = "UserData";
        private static string _defaultImageName = "default.png";

        public static void Init(IWebHostEnvironment webHostEnvironment)
        {
            _webHostEnvironment = webHostEnvironment;
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

            return Path.Combine(_mainUserDirectory, userId);
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

            var userTrueDirPath = Path.Combine(_webHostEnvironment.WebRootPath, path);
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

            parentDir = Path.Combine(parentDir, newDirName);
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

            return Path.Combine(_webHostEnvironment.WebRootPath, path);
        }

        public static string GetDefaultImagePath()
        {
            return _defaultImageName;
        }
    }
}
