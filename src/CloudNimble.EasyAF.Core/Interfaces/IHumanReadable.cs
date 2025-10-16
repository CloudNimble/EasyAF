namespace CloudNimble.EasyAF.Core
{

    /// <summary>
    /// An interface that specifies the implementing Entity displays text to the user.
    /// </summary>
    public interface IHumanReadable
    {

        /// <summary>
        /// The text to be displayed to the user.
        /// </summary>
        string DisplayName { get; set; }

    }

}