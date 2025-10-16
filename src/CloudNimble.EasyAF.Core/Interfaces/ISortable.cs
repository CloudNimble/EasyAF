namespace CloudNimble.EasyAF.Core
{

    /// <summary>
    /// An interface that specifies the implementing Entity can be contains an <see cref="int"/> that tracks the order items should be displayed in a list.
    /// </summary>
    public interface ISortable
    {

        /// <summary>
        /// The order this entity should be displayed in a list.
        /// </summary>
        int SortOrder { get; set; }

    }

}