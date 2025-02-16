using Fluxor;
using 亂七八糟.Model;

namespace 亂七八糟.Fluxor.Razor
{
    public partial class UserStateComponent
    {
        public required IState<UserState> UserState { get; init; }  
        public required IDispatcher Dispatcher { get; init;}

        protected override void OnInitialized()
        {
            // Load data with callbacks
            Dispatcher.Dispatch(new LoadUserDataAction(
                OnSuccess: state =>
                {
                    Console.WriteLine($"Data loaded for user: {state.Username}");
                    StateHasChanged();
                },
                OnError: ex =>
                {
                    Console.WriteLine($"Error loading data: {ex.Message}");
                    StateHasChanged();
                }
            ));
        }

        private void SaveData<T1, T2, T3>()
        {
            Dispatcher.Dispatch(new SaveUserDataAction<T1,T2,T3>(
                "testUser",
                new Dictionary<string, string> { ["theme"] = "dark" },
                OnSuccess: state =>
                {
                    Console.WriteLine("Data saved successfully");
                    StateHasChanged();
                },
                OnError: ex =>
                {
                    Console.WriteLine($"Error saving data: {ex.Message}");
                    StateHasChanged();
                }
            ));
        }
    }
}