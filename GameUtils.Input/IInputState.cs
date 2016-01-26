using System.Collections;

namespace GameUtils.Input
{
    public interface IInputState
    {
        IEnumerable EnumerateElements();

        ElementType GetElementType(object element);

        float GetElementState(object element);
    }
}
