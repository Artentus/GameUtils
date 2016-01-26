namespace GameUtils.Input
{
    public abstract class InputConverter<TSource, TResult> : InputSource<TResult> where TSource : IInputState where TResult : IInputState
    {
        protected InputConverter(InputSource<TSource> source)
        {
            
        }
    }
}
