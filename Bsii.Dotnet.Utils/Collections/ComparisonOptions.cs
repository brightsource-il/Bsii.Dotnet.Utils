namespace Bsii.Dotnet.Utils.Collections
{
    public enum ComparisonOptions
    {
        /// <summary>
        /// Left or right closest neigbour according to distance from the search element
        /// </summary>
        NearestNeighbour,

        /// <summary>
        /// The lower neighbour when available
        /// </summary>
        LeftNeigbour,

        /// <summary>
        /// The higher neighbour when available
        /// </summary>
        RightNeighbour
    }
}