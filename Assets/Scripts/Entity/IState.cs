namespace Spelunky {

    /// <summary>
    /// Interface for state machine states. Implemented by both player and enemy states.
    /// </summary>
    public interface IState {

        /// <summary>
        /// Whether this state component is enabled.
        /// </summary>
        bool enabled { get; set; }

        /// <summary>
        /// Check if we can enter this state.
        /// </summary>
        bool CanEnterState();

        /// <summary>
        /// Called when entering this state.
        /// </summary>
        void EnterState();

        /// <summary>
        /// Called when exiting this state.
        /// </summary>
        void ExitState();

    }

}