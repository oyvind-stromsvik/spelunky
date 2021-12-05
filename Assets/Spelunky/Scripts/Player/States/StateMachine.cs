namespace Spelunky {

    public class StateMachine {
        public State CurrentState { get; private set; }
        public State PreviousState { get; private set; }

        public void AttemptToChangeState(State newState) {
            if (CurrentState != null) {
                if (newState == CurrentState) {
                    return;
                }

                if (!newState.CanEnter()) {
                    return;
                }

                CurrentState.Exit();
                CurrentState.enabled = false;
                PreviousState = CurrentState;
            }

            CurrentState = newState;
            CurrentState.enabled = true;
            CurrentState.Enter();
        }
    }

}