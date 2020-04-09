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

        public static void Init(IWebHostEnvironment webHostEnvironment)
        {
            _webHostEnvironment = webHostEnvironment;
            DirectoryInfo directoryInfo = new DirectoryInfo(_mainDirectory);

            if (!directoryInfo.Exists)
            {
                directoryInfo.Create();
            }
        }

        public static string CreateNewUserPath(string userId)
        {
            return Path.Combine(_mainDirectory, userId);
        }

        public static bool CreateUserDirectory(string path)
        {
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
            parentDir = ConcatPaths(parentDir, newDirName);
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

        private static string ConcatPaths(string path1, string path2)
        {
            return $"{path1}/{path2}";
        }
    }
}
