using Microsoft.AspNetCore.Hosting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MathEvent.Helpers
{
    public static class UserDataPathWorker
    {
        private static IWebHostEnvironment _webHostEnvironment;
        private static string _mainDirectory = "UserData";
        private static string _defaultImageName = "default.png";

        public static void Init(IWebHostEnvironment webHostEnvironment)
        {
            _webHostEnvironment = webHostEnvironment;
            DirectoryInfo directoryInfo = new DirectoryInfo(_mainDirectory);
        }

        public static string CreateNewUserPath(string userId)
        {
            return Path.Combine(_mainDirectory, userId);
        }

        public static bool CreateDirectory(string path)
        {
            // сделать проверку, что папка создалась, иначе что-то делать
            var userTrueDirPath = Path.Combine(_webHostEnvironment.WebRootPath, path);

            DirectoryInfo directoryInfo = new DirectoryInfo(userTrueDirPath);

            if (!directoryInfo.Exists)
            {
                directoryInfo.Create();

                return true;
            }
            else
            {
                return false;
            }

        }

        public static bool CreateSubDirectory(ref string parentDir, string newDirName)
        {
            parentDir = Path.Combine(parentDir, newDirName);
            var parentTrueDir = Path.Combine(_webHostEnvironment.WebRootPath, parentDir);
            DirectoryInfo directoryInfo = new DirectoryInfo(parentTrueDir);

            if (!directoryInfo.Exists)
            {
                directoryInfo.Create();

                return true;
            }
            else
            {
                return false;
            }
        }

        public static string GetRootPath(string path)
        {
            return Path.Combine(_webHostEnvironment.WebRootPath, path);
        }

        public static string GetDefaultImagePath()
        {
            return _defaultImageName;
        }

        private static string ConcatPaths(string path1, string path2)
        {
            return $"{path1}/{path2}";
        }
    }
}
