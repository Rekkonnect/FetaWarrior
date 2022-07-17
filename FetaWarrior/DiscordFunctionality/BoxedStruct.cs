namespace FetaWarrior.DiscordFunctionality;

public sealed class BoxedStruct<T>
    where T : struct
{
    public T Value { get; set; }
    
    public BoxedStruct() { }
    public BoxedStruct(T value)
    {
        Value = value;
    }
}
