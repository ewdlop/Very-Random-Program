using System.Threading;
using System.Threading.Tasks.Sources;

namespace 亂七八糟.确认
{
    public class ValueTaskSource : IValueTaskSource
    {
        public ValueTaskSourceStatus GetStatus(short token)
        {
            throw new System.NotImplementedException();
        }
        public void OnCompleted(Action<object> continuation, object state, short token, ValueTaskSourceOnCompletedFlags flags)
        {
            throw new System.NotImplementedException();
        }
        public void GetResult(short token)
        {
            throw new System.NotImplementedException();
        }
    }
}
