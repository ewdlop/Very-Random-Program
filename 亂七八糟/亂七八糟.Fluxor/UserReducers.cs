using Fluxor;
using 亂七八糟.Model;

namespace 亂七八糟.Fluxor
{
    // Reducers
    public static class UserReducers
    {
        [ReducerMethod]
        public static UserState OnLoadUserData(UserState state, LoadUserDataAction action) =>
            state with { IsLoading = true, Error = null };

        [ReducerMethod]
        public static UserState OnLoadUserDataSuccess(UserState state, LoadUserDataSuccessAction action)
        {
            var newState = state with
            {
                Username = action.LoadedState.Username,
                Settings = action.LoadedState.Settings,
                IsLoading = false,
                Error = null
            };

            // Execute callback if provided
            action.OnSuccess?.Invoke(newState);

            return newState;
        }

        [ReducerMethod]
        public static UserState OnLoadUserDataError(UserState state, LoadUserDataErrorAction action)
        {
            var newState = state with
            {
                IsLoading = false,
                Error = action.ErrorMessage
            };

            // Execute error callback if provided
            action.OnError?.Invoke(action.Exception);

            return newState;
        }
    }
}