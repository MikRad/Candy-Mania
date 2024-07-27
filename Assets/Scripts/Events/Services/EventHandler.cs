public class EventHandler
{
    public object CallbackWithArg { get; }
    public object CallbackNoArgs { get; }
    public int Priority { get; }
    
    public EventHandler(object callbackWithArg, object callbackNoArgs, int priority)
    {
        CallbackWithArg = callbackWithArg;
        CallbackNoArgs = callbackNoArgs;
        Priority = priority;
    }
}