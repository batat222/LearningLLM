using System.Data;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
public class SimpleNGramModel
{
    Dictionary<string, Dictionary<string, int>> Trigrams;
    Dictionary<string, Dictionary<string, int>> Bigrams;
    Dictionary<string, int> Unigrams;

    public SimpleNGramModel()
    {
        Trigrams = new Dictionary<string, Dictionary<string, int>>();
        if (File.Exists("TriData.json"))
        {
            string jsonString = File.ReadAllText("TriData.json", Encoding.UTF8);
            Trigrams = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, int>>>(jsonString);
        }
        Bigrams = new Dictionary<string, Dictionary<string, int>>();
        if (File.Exists("BiData.json"))
        {
            string jsonString = File.ReadAllText("BiData.json", Encoding.UTF8);
            Bigrams = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, int>>>(jsonString);
        }
        Unigrams = new Dictionary<string, int>();
        if (File.Exists("UniData.json"))
        {
            string jsonString = File.ReadAllText("UniData.json", Encoding.UTF8);
            Unigrams = JsonSerializer.Deserialize<Dictionary<string, int>>(jsonString);
        }
    }

    public void Analize(string input)
    {
        Preprocessor preprocessor = new Preprocessor();
        string[] tokens = preprocessor.Execute(input);

        for (int i = 0; i < tokens.Length - 2; i++)
        {
            string context = $"{tokens[i]} {tokens[i + 1]}";
            string nextWord = tokens[i + 2];

            if (!Trigrams.ContainsKey(context))
            {
                Trigrams[context] = new Dictionary<string, int>();
            }

            if (!Trigrams[context].ContainsKey(nextWord))
            {
                Trigrams[context][nextWord] = 0;
            }

            Trigrams[context][nextWord]++;

        }

        for (int i = 0; i < tokens.Length - 1; i++)
        {
            string word1 = tokens[i];
            string word2 = tokens[i + 1];

            if (!Bigrams.ContainsKey(word1))
            {
                Bigrams[word1] = new Dictionary<string, int>();
            }

            if (!Bigrams[word1].ContainsKey(word2))
            {
                Bigrams[word1][word2] = 0;
            }

            Bigrams[word1][word2]++;
        }

        for (int i = 0; i < tokens.Length; i++)
        {
            string word = tokens[i];
            if (!Unigrams.ContainsKey(word))
            {
                Unigrams[word] = 0;
            }
            Unigrams[word]++;
        }
    }

    public void AnalizeWithLogs(string input)
    {
        Preprocessor preprocessor = new Preprocessor();
        string[] tokens = preprocessor.Execute(input);

        for (int i = 0; i < tokens.Length; i++)
        {
            // Unigrams
            string word = tokens[i];
            if (!Unigrams.ContainsKey(word))
            {
                Unigrams[word] = 0;
            }
            Unigrams[word]++;
            Console.WriteLine($"{word} = {Unigrams[word]}");

            // Bigrams
            if (i < tokens.Length - 1)
            {
                string word1 = tokens[i];
                string word2 = tokens[i + 1];
                if (!Bigrams.ContainsKey(word1))
                {
                    Bigrams[word1] = new Dictionary<string, int>();
                }
                if (!Bigrams[word1].ContainsKey(word2))
                {
                    Bigrams[word1][word2] = 0;
                }
                Bigrams[word1][word2]++;
                Console.WriteLine($"{word1} {word2} = {Bigrams[word1][word2]}");
            }

            // Trigrams
            if (i < tokens.Length - 2)
            {
                string context = $"{tokens[i]} {tokens[i + 1]}";
                string nextWord = tokens[i + 2];
                if (!Trigrams.ContainsKey(context))
                {
                    Trigrams[context] = new Dictionary<string, int>();
                }
                if (!Trigrams[context].ContainsKey(nextWord))
                {
                    Trigrams[context][nextWord] = 0;
                }
                Trigrams[context][nextWord]++;
                Console.WriteLine($"{context} {nextWord} = {Trigrams[context][nextWord]}");
            }
        }
    }

    public void Learn()
    {
        string sampleData = File.ReadAllText("sampledata.txt");
        AnalizeWithLogs(sampleData);
        string TriJson = JsonSerializer.Serialize(Trigrams, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText("TrigramData.json", TriJson);
        string BiJson = JsonSerializer.Serialize(Bigrams, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText("BigramData.json", BiJson);
        string UniJson = JsonSerializer.Serialize(Unigrams, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText("UnigramData.json", UniJson);
        Console.WriteLine("Learned successfully, i hope...");
    }


    public string PredictNextWord(string phrase)
    {
        Preprocessor preprocessor = new Preprocessor();
        string[] contextTokens = preprocessor.Execute(phrase);
        string nextWord = null;
        int maxFrequency = 0;
        if (Trigrams.TryGetValue(contextTokens[contextTokens.Length - 1], out var candidates))
        {
            foreach (var pair in candidates)
            {
                if (pair.Value > maxFrequency)
                {
                    maxFrequency = pair.Value;
                    nextWord = pair.Key;
                }
            }
            return nextWord;
        }

        if (contextTokens.Length == 1)
        {
            string word = contextTokens[contextTokens.Length - 1];
            var innerDict = Bigrams[word];
            foreach (var pair in innerDict)
            {
                if (pair.Value > maxFrequency)
                {
                    maxFrequency = pair.Value;
                    nextWord = pair.Key;
                }
            }
            return nextWord;
        }

        try
        {
            foreach (var wrd in Unigrams.Keys)
            {
                if (maxFrequency > Unigrams[wrd])
                {
                    maxFrequency = Unigrams[wrd];
                    nextWord = wrd;
                }
            }
            return nextWord;
        }
        catch (System.Collections.Generic.KeyNotFoundException)
        {
            return ".";
        }
    }
}

public class Preprocessor
{
    public string[] Execute(string text)
    {
        string _text;
        _text = text;
        _text = Regex.Replace(_text, @"[^\w\s]", "");
        _text.ToLower();
        return _text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
    }
}

public class Program
{
    public static string predict(SimpleNGramModel model, string prevText)
    {
        string phrase;
        Console.Write("Вы: ");
        phrase = Console.ReadLine();
        if (phrase != "")
        {
            prevText = prevText + ' ' + phrase + ' ' + model.PredictNextWord(phrase);
            predict(model, prevText);
        }
        return $"Модель: {prevText}";
    }
    public static void start(SimpleNGramModel model)
    {
        string text = "";
        Console.WriteLine("1.Режим прогнозирования слова\n2.Обучить модель");
        switch (Console.ReadLine())
        {
            case "1":
                {
                    predict(model, text);
                    break;
                }
            case "2":
                {
                    model.Learn();
                    start(model);
                    break;
                }
            default:
                {
                    start(model);
                    break;
                }
        }
    }
    public static void Main(string[] args)
    {
        Console.OutputEncoding = Encoding.UTF8;
        SimpleNGramModel model = new SimpleNGramModel();
        start(model);
    }
}