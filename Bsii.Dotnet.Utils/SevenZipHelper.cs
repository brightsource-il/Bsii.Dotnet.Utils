using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Bsii.Dotnet.Utils.Collections;

namespace Bsii.Dotnet.Utils
{
    public class SevenZipHelper
    {
        private readonly string _executablePath;

        public SevenZipHelper(string executablePath = null)
        {
            var possiblePaths = new[]
            {
                executablePath,
                EnvironmentUtils.TryFindInPath("7z", "7z.exe"),
                @"C:\Program Files\7-Zip\7z.exe",
                @"C:\Program Files (x86)\7-Zip\7z.exe"
            };
            foreach (var possiblePath in possiblePaths.Where(p => p != null))
            {
                if (File.Exists(possiblePath))
                {
                    _executablePath = possiblePath;
                    break;
                }
            }

            if (string.IsNullOrEmpty(_executablePath))
            {
                throw new FileNotFoundException(
                    $"Didn't find 7-zip in any of the specified paths: {possiblePaths.AsString()}");
            }
        }

        public async Task Extract(string archivePath, string targetPath, List<string> filters = null)
        {
            var pOutput = await new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = _executablePath,
                    Arguments = filters == null ? 
                        $"x \"{archivePath}\" -y -o\"{targetPath}\"" : 
                        $"x \"{archivePath}\" -y -o\"{targetPath}\" {string.Join(" ",filters)} -r"
                }
            }.RunAsyncWithStandardStreamCapture();
            if (pOutput.ExitCode != 0)
            {
                throw new SevenZipException("Archive extraction failed", pOutput.StandardError);
            }
        }
    }

    public class SevenZipException : Exception
    {
        public SevenZipException(string s, string stdError)
            : base(s + "\n" + stdError)
        {
        }
    }
}