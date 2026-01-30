namespace Spelunky {

    /// <summary>
    /// Generic state machine that works with any IState implementation.
    /// Used by both Player and Enemy systems.
    /// </summary>
    public class StateMachine {

        public IState CurrentState { get; private set; }
        public IState PreviousState { get; private set; }

        public bool AttemptToChangeState(IState newState) {
            if (CurrentState != null) {
                if (newState == CurrentState) {
                    return false;
                }

                if (!newState.CanEnterState()) {
                    return false;
                }

                CurrentState.ExitState();
                CurrentState.enabled = false;
                PreviousState = CurrentState;
            }

            CurrentState = newState;
            CurrentState.enabled = true;
            CurrentState.EnterState();

            return true;
        }

    }

}