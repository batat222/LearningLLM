public interface ITokenizer
{
    string[] Tokenize(string text);
}

public class WhitespaceTokenizer : ITokenizer
{
    string[] ITokenizer.Tokenize(string text)
    {
        return text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
    }
}

public class CharacterTokenizer : ITokenizer
{
    string[] ITokenizer.Tokenize(string text)
    {
        return text.ToCharArray().Select(c => c.ToString()).ToArray();
    }
}

public class TokenizerContext
{
    public ITokenizer tokenizer { get; set; }

    public TokenizerContext(ITokenizer _tokenizer)
    {
        tokenizer = _tokenizer;
    }

    public string[] Execute(string token)
    {
        return tokenizer.Tokenize(token);
    }
}