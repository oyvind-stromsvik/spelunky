public class StateMachine {

    public State CurrentState { get; private set; }
    public State LastState { get; private set; }

    public void AttemptToChangeState(State newState) {
        if (CurrentState != null) {
            if (newState == CurrentState) {
                return;
            }

            if (!newState.CanEnter()) {
                return;
            }

            CurrentState.Exit();
            LastState = CurrentState;
        }

        CurrentState = newState;
        CurrentState.Enter();
    }
}
