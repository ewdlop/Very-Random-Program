namespace 亂七八糟.Reflection
{
    public class AnAttribute(string name) : Attribute
    {
        public string Name { get; set; } = name;
    }
}