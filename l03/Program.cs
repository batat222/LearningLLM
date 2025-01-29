public class Preprocessor
{
    public string[] Execute (string text)
    {
        string _text;
        _text = text;
        var charsToRemove = new string[] { "@", ",", ".", ";", "'", "~" };
        foreach (var c in charsToRemove)
        {
            _text = _text.Replace(c, string.Empty);
        }
        _text.ToLower();
        return _text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
    }
}

public class Program
{
    public static void Main(string[] args)
    {
        Preprocessor preprocessor = new Preprocessor();
        foreach(string str in preprocessor.Execute("Lol, never thought that i'll ever try to do smth like this 0w0"))
        {
            Console.WriteLine(str);
        }
    }
}