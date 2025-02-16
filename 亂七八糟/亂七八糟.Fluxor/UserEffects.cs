using Fluxor;
using 亂七八糟.Model;
using 亂七八糟.Shared.Interfaces;

namespace 亂七八糟.Fluxor
{
    // Effects
    public class UserEffects
    {
        private readonly IProtectedStorageService _storage;

        public UserEffects(IProtectedStorageService storage)
        {
            _storage = storage;
        }

        [EffectMethod]
        public async Task HandleLoadUserData(LoadUserDataAction action, IDispatcher dispatcher)
        {
            try
            {
                var userData = await _storage.GetAsync<UserState>("userData");

                dispatcher.Dispatch(new LoadUserDataSuccessAction(
                    userData ?? new UserState(),
                    action.OnSuccess
                ));
            }
            catch (Exception ex)
            {
                dispatcher.Dispatch(new LoadUserDataErrorAction(
                    "Failed to load user data",
                    ex,
                    action.OnError
                ));
            }
        }

        [EffectMethod]
        public async Task HandleSaveUserData<T2,T3,T4>(SaveUserDataAction<UserState,T2,T3> action, IDispatcher dispatcher)
        {
            try
            {
                var userData = new UserState
                {
                    Username = action.Username,
                    Settings = action.Settings,
                    Cause = action
                };

                await _storage.SetAsync("userData", userData);

                dispatcher.Dispatch(new SaveUserDataSuccessAction<UserEffects>(
                    userData,
                    action.OnSuccess,
                    this
                ));
            }
            catch (Exception ex)
            {
                dispatcher.Dispatch(new SaveUserDataErrorAction(
                    "Failed to save user data",
                    ex,
                    action.OnError
                ));
            }
        }
    }
}