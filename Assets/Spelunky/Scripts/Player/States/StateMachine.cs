namespace Spelunky {

    public class StateMachine {

        public State CurrentState { get; private set; }
        public State PreviousState { get; private set; }

        public bool AttemptToChangeState(State newState) {
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
