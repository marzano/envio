using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NfImpacto.Envio.Helpers
{
    public class FileSystemHelper
    {

        public static string CombineDirectory(string rootDirectoryPath, string childDirectoryPath)
        {
            rootDirectoryPath = rootDirectoryPath.TrimEnd('\\');
            childDirectoryPath = childDirectoryPath.Trim('\\');
            return Path.Combine(rootDirectoryPath, childDirectoryPath);
        }

        public static string CombineFile(string rootDirectoryPath, string filePathOrName)
        {
            rootDirectoryPath = rootDirectoryPath.TrimEnd('\\');
            filePathOrName = filePathOrName.Trim('\\');
            return Path.Combine(rootDirectoryPath, filePathOrName);
        }

        public static void CreateDirectoryIfNotExists(string directoryPath)
        {
            if (!DirectoryExists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }
        }

        public static void DeleteFileIfExists(string filePath)
        {
            if (FileExists(filePath))
            {
                File.Delete(filePath);
            }
        }

        public static bool DirectoryExists(string directoryPath)
        {
            return Directory.Exists(directoryPath);
        }

        public static bool FileExists(string filePath)
        {
            return File.Exists(filePath);
        }


        public static void MoveFile(string fromFilePath, string toFilePath)
        {
            File.Move(fromFilePath, toFilePath);
        }

        public static void FileAppendAllText(string filePath, string contents)
        {
            File.AppendAllText(filePath, contents);
        }
    }
}
