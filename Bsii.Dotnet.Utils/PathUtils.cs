using System;
using System.IO;

namespace Bsii.Dotnet.Utils
{
    public static class PathUtils
    {
        /// <summary>
        /// Try to find one of the given file names in one of the paths defined in the PATH environment variable
        /// (e.g. pass "git" & "git.exe" as fileNames to do cross-platform search of the GIT executable)
        /// </summary>
        /// <param name="fileNames">Names of files to look for</param>
        /// <returns>The first match of a file name within one of the paths defined within the PATH env. variable</returns>
        public static string TryFindInPath(params string[] fileNames)
        {
            var pathEnvVar = Environment.GetEnvironmentVariable("PATH");
            if (pathEnvVar == null)
            {
                return null;
            }

            var values = pathEnvVar.Split(Path.PathSeparator);
            foreach (var fileName in fileNames)
            {
                if (File.Exists(fileName))
                {
                    return Path.GetFullPath(fileName);
                }

                foreach (var path in values)
                {
                    var fullPath = Path.Combine(path, fileName);
                    if (File.Exists(fullPath))
                    {
                        return fullPath;
                    }
                }
            }

            return null;
        }
    }
}