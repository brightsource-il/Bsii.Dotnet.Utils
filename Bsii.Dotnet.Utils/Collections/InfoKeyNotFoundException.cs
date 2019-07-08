using System.Collections.Generic;

namespace Bsii.Dotnet.Utils.Collections
{
    /// <summary>
    /// Informative key not found exception
    /// </summary>
    public class InfoKeyNotFoundException : KeyNotFoundException
    {
        /// <summary>
        /// The key value that was not found
        /// </summary>
        public object MissingKey { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="missingKey"></param>
        public InfoKeyNotFoundException(object missingKey) : base(
            $"The key '{missingKey}' wasn't found in the dictionary")
        {
            MissingKey = missingKey;
        }
    }
}