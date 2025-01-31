using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

public class SimpleNGramModel
{
    Dictionary<string, Dictionary<string, Dictionary<string, int>>> Trigrams;  // Key = "language", Value = { "word1 word2": { "word3": frequency } }
    Dictionary<string, Dictionary<string, Dictionary<string, int>>> Bigrams;   // Key = "language", Value = { "word1": { "word2": frequency } }
    Dictionary<string, Dictionary<string, int>> Unigrams;                      // Key = "language", Value = { "word": frequency }

    public SimpleNGramModel()
    {
        Trigrams = new Dictionary<string, Dictionary<string, Dictionary<string, int>>>();
        if (File.Exists("TrigramData.json"))
        {
            string jsonString = File.ReadAllText("TrigramData.json", Encoding.UTF8);
            Trigrams = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, Dictionary<string, int>>>>(jsonString);
        }

        Bigrams = new Dictionary<string, Dictionary<string, Dictionary<string, int>>>();
        if (File.Exists("BigramData.json"))
        {
            string jsonString = File.ReadAllText("BigramData.json", Encoding.UTF8);
            Bigrams = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, Dictionary<string, int>>>>(jsonString);
        }

        Unigrams = new Dictionary<string, Dictionary<string, int>>();
        if (File.Exists("UnigramData.json"))
        {
            string jsonString = File.ReadAllText("UnigramData.json", Encoding.UTF8);
            Unigrams = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, int>>>(jsonString);
        }
    }

    public void AnalizeWithLogs(string input)
{
    Preprocessor preprocessor = new Preprocessor();
    LanguageDetector detector = new LanguageDetector();
    string[] tokens = preprocessor.Execute(input);

    Console.WriteLine("Starting analysis with logs...");
    Console.WriteLine("--------------------------------");

    int totalTokens = tokens.Length;
    int processedTokens = 0;
    var stopwatch = System.Diagnostics.Stopwatch.StartNew();

    // Initialize progress bar
    int progressBarWidth = 50;
    Console.WriteLine("Progress: [");
    Console.SetCursorPosition(0, Console.CursorTop - 1); // Move cursor back to the progress bar line

    for (int i = 0; i < tokens.Length; i++)
    {
        string word = tokens[i];
        processedTokens++;

        // Detect language for the current word
        string language = detector.DetectLanguage(word);

        // Initialize language-specific dictionaries if they don't exist
        if (!Trigrams.ContainsKey(language))
        {
            Trigrams[language] = new Dictionary<string, Dictionary<string, int>>();
        }
        if (!Bigrams.ContainsKey(language))
        {
            Bigrams[language] = new Dictionary<string, Dictionary<string, int>>();
        }
        if (!Unigrams.ContainsKey(language))
        {
            Unigrams[language] = new Dictionary<string, int>();
        }

        // Unigrams
        if (!Unigrams[language].ContainsKey(word))
        {
            Unigrams[language][word] = 0;
        }
        Unigrams[language][word]++;

        // Bigrams
        if (i < tokens.Length - 1)
        {
            string word1 = tokens[i];
            string word2 = tokens[i + 1];
            if (!Bigrams[language].ContainsKey(word1))
            {
                Bigrams[language][word1] = new Dictionary<string, int>();
            }
            if (!Bigrams[language][word1].ContainsKey(word2))
            {
                Bigrams[language][word1][word2] = 0;
            }
            Bigrams[language][word1][word2]++;
        }

        // Trigrams
        if (i < tokens.Length - 2)
        {
            string context = $"{tokens[i]} {tokens[i + 1]}";
            string nextWord = tokens[i + 2];
            if (!Trigrams[language].ContainsKey(context))
            {
                Trigrams[language][context] = new Dictionary<string, int>();
            }
            if (!Trigrams[language][context].ContainsKey(nextWord))
            {
                Trigrams[language][context][nextWord] = 0;
            }
            Trigrams[language][context][nextWord]++;
        }

        // Update progress every 1% or at the end
        if (processedTokens % (totalTokens / 100) == 0 || processedTokens == totalTokens)
        {
            // Calculate progress percentage
            double progress = (double)processedTokens / totalTokens;

            // Update progress bar
            int progressBarFill = (int)(progress * progressBarWidth);
            Console.Clear();
            Console.Write(new string('=', progressBarFill).PadRight(progressBarWidth));
            Console.Write($"] {progress * 100:F1}%");

            // Update stats
            Console.SetCursorPosition(0, Console.CursorTop + 1);
            Console.WriteLine($"Time elapsed: {stopwatch.Elapsed.TotalSeconds:F2} seconds");
            Console.WriteLine($"Unigrams: {Unigrams.Sum(lang => lang.Value.Count)}");
            Console.WriteLine($"Bigrams: {Bigrams.Sum(lang => lang.Value.Count)}");
            Console.WriteLine($"Trigrams: {Trigrams.Sum(lang => lang.Value.Count)}");
            Console.WriteLine("--------------------------------");

            // Move cursor back to the progress bar line
            Console.SetCursorPosition(0, Console.CursorTop - 4);
        }
    }

    // Move cursor to the end of the logs
    Console.SetCursorPosition(0, Console.CursorTop + 4);
    Console.WriteLine("Analysis with logs completed.");
    Console.WriteLine($"Total time: {stopwatch.Elapsed.TotalSeconds:F2} seconds");
}

    public void Learn()
    {
        if (!File.Exists("sampledata.txt"))
        {
            Console.WriteLine("Error: sampledata.txt not found.");
            return;
        }

        string sampleData = File.ReadAllText("sampledata.txt");
        AnalizeWithLogs(sampleData);

        string TriJson = JsonSerializer.Serialize(Trigrams, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText("TrigramData.json", TriJson);

        string BiJson = JsonSerializer.Serialize(Bigrams, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText("BigramData.json", BiJson);

        string UniJson = JsonSerializer.Serialize(Unigrams, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText("UnigramData.json", UniJson);

        Console.WriteLine("Learned successfully, I hope...");
    }

    public string PredictNextWord(string phrase)
    {
        Preprocessor preprocessor = new Preprocessor();
        LanguageDetector detector = new LanguageDetector();
        string language = detector.DetectLanguage(phrase);
        string[] contextTokens = preprocessor.Execute(phrase);
        string nextWord = null;
        int maxFrequency = 0;

        Console.WriteLine($"Detected language: {language}");
        Console.WriteLine($"Context tokens: {string.Join(", ", contextTokens)}");

        if (!Trigrams.ContainsKey(language) || !Bigrams.ContainsKey(language) || !Unigrams.ContainsKey(language))
        {
            Console.WriteLine("Language not found in dictionaries.");
            return ".";
        }

        // Try trigrams
        if (contextTokens.Length >= 2)
        {
            string trigramKey = $"{contextTokens[^2]} {contextTokens[^1]}";
            Console.WriteLine($"Trying trigram key: {trigramKey}");
            if (Trigrams[language].TryGetValue(trigramKey, out var candidates))
            {
                foreach (var pair in candidates)
                {
                    if (pair.Value > maxFrequency)
                    {
                        maxFrequency = pair.Value;
                        nextWord = pair.Key;
                    }
                }
                Console.WriteLine($"Trigram prediction: {nextWord}");
                return nextWord;
            }
        }

        // Fallback to bigrams
        if (contextTokens.Length >= 1)
        {
            string word = contextTokens[^1];
            Console.WriteLine($"Trying bigram key: {word}");
            if (Bigrams[language].TryGetValue(word, out var candidates))
            {
                foreach (var pair in candidates)
                {
                    if (pair.Value > maxFrequency)
                    {
                        maxFrequency = pair.Value;
                        nextWord = pair.Key;
                    }
                }
                Console.WriteLine($"Bigram prediction: {nextWord}");
                return nextWord;
            }
        }

        // Fallback to unigrams
        Console.WriteLine("Falling back to unigrams.");
        foreach (var wrd in Unigrams[language].Keys)
        {
            if (Unigrams[language][wrd] > maxFrequency)
            {
                maxFrequency = Unigrams[language][wrd];
                nextWord = wrd;
            }
        }
        Console.WriteLine($"Unigram prediction: {nextWord}");
        return nextWord ?? ".";
    }

    public class Preprocessor
    {
        public string[] Execute(string text)
        {
            // Normalize text to lowercase
            text = text.ToLower();

            // Preserve Cyrillic, Udmurt, and Latin characters
            string pattern = @"[^\p{IsCyrillic}\p{IsBasicLatin}\s]";
            text = Regex.Replace(text, pattern, "");

            // Split into tokens
            return text.Split(new[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
        }
    }

    public class LanguageDetector
    {
        private static readonly Dictionary<string, string[]> LanguageStopwords = new()
        {
            { "Russian", new[] { "и", "в", "на", "с", "по", "что", "это", "как", "все", "но" } },
            { "Udmurt", new[] { "но", "азь", "уке", "але", "ог", "со", "та", "ӧй", "кыӵе", "мынам" } },
            { "English", new[] { "the", "and", "you", "that", "for", "are", "with", "this", "have", "was" } }
        };

        private static readonly Dictionary<string, string[]> LanguagePatterns = new()
        {
            { "Russian", new[] { "ый", "ов", "ев", "ин", "ия", "ть", "ся" } },
            { "Udmurt", new[] { "ӝ", "ӟ", "ӥ", "ӧ", "ӵ", "ӧй", "ысь" } },
            { "English", new[] { "th", "ing", "sh", "ch", "the", "and", "you" } }
        };

        public string DetectLanguage(string text)
        {
            var scores = new Dictionary<string, int>();

            // Initialize scores for each language
            foreach (var lang in LanguageStopwords.Keys)
            {
                scores[lang] = 0;
            }

            // Split text into words
            string[] words = text.ToLower().Split(new[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

            // Score each word based on stopwords
            foreach (var word in words)
            {
                foreach (var lang in LanguageStopwords.Keys)
                {
                    if (LanguageStopwords[lang].Contains(word))
                    {
                        scores[lang]++;
                    }
                }
            }

            // Score based on language-specific patterns
            foreach (var lang in LanguagePatterns.Keys)
            {
                foreach (var pattern in LanguagePatterns[lang])
                {
                    scores[lang] += Regex.Matches(text, pattern).Count;
                }
            }

            // Return the language with the highest score
            return scores.OrderByDescending(kvp => kvp.Value).First().Key;
        }
    }

    public class Program
    {
        public static void predict(SimpleNGramModel model)
        {
            string prevText = "";
            while (true)
            {
                Console.Write("Вы: ");
                string phrase = Console.ReadLine();
                if (string.IsNullOrEmpty(phrase))
                {
                    break;
                }

                // Get the model's prediction
                string prediction = model.PredictNextWord(phrase);
                prevText = prevText + " " + phrase + " " + prediction;

                // Display the model's response
                Console.WriteLine($"Модель: {prevText}");
            }
        }

        public static void start(SimpleNGramModel model)
        {
            while (true)
            {
                Console.WriteLine("1. Режим прогнозирования слова\n2. Обучить модель\n3. Выход");
                string choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        predict(model);
                        break;
                    case "2":
                        model.Learn();
                        break;
                    case "3":
                        return;  // Exit the program
                    default:
                        Console.WriteLine("Неверный выбор. Попробуйте снова.");
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
}