using System;
using System.IO;

namespace Bsii.Dotnet.Utils
{
    public static class PathUtils
    {
        public static string MakeRelativePath(string baseDir, string filePath)
        {
            if (string.IsNullOrEmpty(baseDir))
            {
                throw new ArgumentNullException(nameof(baseDir));
            }

            if (string.IsNullOrEmpty(filePath))
            {
                throw new ArgumentNullException(nameof(filePath));
            }

            if (!baseDir.EndsWith(Path.DirectorySeparatorChar.ToString()))
            {
                baseDir = baseDir + Path.DirectorySeparatorChar;
            }

            var fromUri = new Uri(baseDir);
            var toUri = new Uri(filePath);

            if (fromUri.Scheme != toUri.Scheme)
            {
                return filePath;
            } // path can't be made relative.

            var relativeUri = fromUri.MakeRelativeUri(toUri);
            var relativePath = Uri.UnescapeDataString(relativeUri.ToString());

            if (toUri.Scheme.Equals("file", StringComparison.InvariantCultureIgnoreCase))
            {
                relativePath = relativePath.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
            }

            return relativePath;
        }
    }
}
