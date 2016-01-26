using System.Collections.Generic;

namespace GameUtils.Input
{
    public abstract class RecordableInputSource<TState, TRecorder> : InputSource<TState> where TState : IInputState where TRecorder : InputRecorder<TRecorder>
    {
        readonly LinkedList<TRecorder> recorders;

        protected IEnumerable<TRecorder> Recorders
        {
            get { return recorders; }
        }

        protected RecordableInputSource()
        {
            recorders = new LinkedList<TRecorder>();
        }

        protected abstract TRecorder CreateRecorderInner();

        public TRecorder CreateRecorder()
        {
            TRecorder recorder = CreateRecorderInner();
            var node = recorders.AddLast(recorder);
            recorder.SetDependency(recorders, node);
            return recorder;
        }
    }
}
