namespace CloudNimble.EasyAF.Core
{

    /// <summary>
    /// An interface that identifies this Entity as being the enumeration details for the SimpleStateMachine.
    /// </summary>
    public interface IDbStateEnum : IDbEnum
    {

        /// <summary>
        /// Text to display to the user regarding the current state, and what needs to happen next.
        /// </summary>
        string InstructionText { get; set; }

        /// <summary>
        /// A string that describes the next action in the SimpleStateMachine, usually displayed on a button or link.
        /// </summary>
        string PrimaryTargetDisplayText { get; set; }

        /// <summary>
        /// An integer that represents the State the Entity should be moved to once this action completes successfully.
        /// </summary>
        int PrimaryTargetSortOrder { get; set; }

        /// <summary>
        /// A string that describes an alternate action in the SimpleStateMachine. This action could skip States moving forward, or return the Entity to a previous State. This text is usually displayed on a button or link.
        /// </summary>
        string SecondaryTargetDisplayText { get; set; }

        /// <summary>
        /// An integer that represents an alternate State the Entity should be moved to once this action is finished.
        /// </summary>
        int SecondaryTargetSortOrder { get; set; }

    }

}
