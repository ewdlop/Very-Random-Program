using Fluxor;
using 亂七八糟.Model;

namespace 亂七八糟.Fluxor
{
    // Feature
    public class UserFeature : Feature<UserState>
    {
        public override string GetName() => "User";

        protected override UserState GetInitialState() => new()
        {
            IsLoading = false,
            Error = null
        };
    }
}
