using System.Data;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
public class SimpleNGramModel
{
    Dictionary<string, Dictionary<string, int>> dictionary;

    public SimpleNGramModel()
    {
        dictionary = new Dictionary<string, Dictionary<string, int>>();
        if(File.Exists("data.json"))
        {
            string jsonString = File.ReadAllText("data.json", Encoding.UTF8);
            dictionary = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, int>>>(jsonString);
        }
    }

    public void Analize(string input)
    {
        Preprocessor preprocessor = new Preprocessor();
        string[] tokens = preprocessor.Execute(input);
        for (int i = 0; i < tokens.Length - 1; i++)
        {
            string word1 = tokens[i];
            string word2 = tokens[i + 1];

            if (!dictionary.ContainsKey(word1))
            {
                dictionary[word1] = new Dictionary<string, int>();
            }

            if (!dictionary[word1].ContainsKey(word2))
            {
                dictionary[word1][word2] = 0;
            }

            dictionary[word1][word2]++;
        }
    }

    public void AnalizeWithLogs(string input)
    {
        Preprocessor preprocessor = new Preprocessor();
        string[] tokens = preprocessor.Execute(input);
        for (int i = 0; i < tokens.Length - 1; i++)
        {
            string word1 = tokens[i];
            string word2 = tokens[i + 1];

            if (!dictionary.ContainsKey(word1))
            {
                dictionary[word1] = new Dictionary<string, int>();
            }

            if (!dictionary[word1].ContainsKey(word2))
            {
                dictionary[word1][word2] = 0;
            }

            dictionary[word1][word2]++;
            Console.WriteLine($"{word1} {word2} = {dictionary[word1][word2]}");
        }
    }

    public void Learn()
    {
        string sampleData = File.ReadAllText("sampledata.txt");
        AnalizeWithLogs(sampleData);
        string json = JsonSerializer.Serialize(dictionary, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText("data.json", json);
        Console.WriteLine("Learned successfully, i hope...");
    }


    public string PredictNextWord(string phrase)
    {
        Preprocessor preprocessor = new Preprocessor();
        string[] words = preprocessor.Execute(phrase);
        string word = words[words.Length - 1];
        if (!dictionary.ContainsKey(word))
        {
            return ".";
        }

        var innerDict = dictionary[word];
        string nextWord = null;
        int maxFrequency = 0;

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
        Console.Write(prevText + ' ');
        phrase = Console.ReadLine();
        if (phrase != "")
        {
            prevText = prevText + ' ' + phrase + ' ' + model.PredictNextWord(phrase);
            predict(model, prevText);
        }
        return prevText;
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