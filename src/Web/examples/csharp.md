prompt > "Create a person POCO"
response > "class Person
{
    public string Name { get; set; }
}"

prompt > "Create a person with a greeting method"
response > "class Person
{
    public string Name { get; set; }
    public string Greeting(string name)
        => $"Hello, {name}. I'm {Name}! Nice to meet you!";
}"

prompt > "Create a person with a greeting method that writes to the console."
response > "class Person
{
    public string Name { get; set; }
    public void Greeting(string name)
    {
        System.Console.WriteLine("Hello, {0}. I'm {1}! Nice to meet you!", name, Name);
    }
}"

prompt > "Create a person object with a greeting method and use it to show a conversation between two people."
response > "
namespace ExampleCode;

class Person(string name)
{
    public string Name { get; set; } = name;
    public void Greeting(string name)
    {
        System.Console.WriteLine("Hello, {0}. I'm {1}! Nice to meet you!", name, Name);
    }
    public void Greet(Person other)
    {
        Greeting(other.Name);
    }
}

public static class Program
{
    static void Main()
    {
        Person
            dan = new("Dan"),
            pandora = new("Pandora");

        dan.Greet(pandora);
        pandora.Greet(dan);
    }
}
"