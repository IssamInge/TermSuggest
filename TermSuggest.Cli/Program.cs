using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TermSuggest.Core;
using TermSuggest.Services;

namespace TermSuggest.Cli
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            try
            {
                using IHost host = CreateHostBuilder(args).Build();
                await RunApplication(host);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur: {ex.Message}");
            }
        }

        private static async Task RunApplication(IHost host)
        {
            using var scope = host.Services.CreateScope();
            var termMatcher = scope.ServiceProvider.GetRequiredService<ITermMatcher>();
            Console.WriteLine("=== TERM SUGGEST - Service de suggestion de termes similaires ===");
            Console.WriteLine();

            await RunDemoExamples(termMatcher);
        }

        private static  async Task RunDemoExamples(ITermMatcher termMatcher)
        {
            Console.WriteLine("EXEMPLES DE DEMONSTRATION");
            Console.WriteLine("=========================");

            // Exemple 1: Cas de base
            var example1 = new
            {
                SearchTerm = "gros",
                Candidates = new[] { "gros", "gras", "graisse", "agressif", "go", "ros", "gro" },
                MaxSuggestions = 2
            };

            await ExecuteExample(termMatcher, "Exemple 1 - Cas de base", example1.SearchTerm,
            example1.Candidates, example1.MaxSuggestions);

            // Exemple 2: Test avec différences
            var example2 = new
            {
                SearchTerm = "test",
                Candidates = new[] { "test", "tent", "rest", "best", "taste", "testing" },
                MaxSuggestions = 3
            };

            await ExecuteExample(termMatcher, "Exemple 2 - Termes similaires", example2.SearchTerm,
                example2.Candidates, example2.MaxSuggestions);

            // Exemple 3: Normalisation
            var example3 = new
            {
                SearchTerm = "MAJUSCULE",
                Candidates = new[] { "majuscule", "Majuscule", "MAJUSCULE", "minuscule" },
                MaxSuggestions = 2
            };

            await ExecuteExample(termMatcher, "Exemple 3 - Normalisation", example3.SearchTerm,
                example3.Candidates, example3.MaxSuggestions);
        }
        private static async Task ExecuteExample(ITermMatcher termMatcher, string title,
             string searchTerm, string[] candidates, int maxSuggestions)
        {
            Console.WriteLine();
            Console.WriteLine($"{title}:");
            Console.WriteLine($"  Terme recherché: {searchTerm}");
            Console.WriteLine($"  Candidats: {string.Join(", ", candidates)}");
            Console.WriteLine($"  Suggestions demandées: {maxSuggestions}");

            var suggestions = await Task.Run(() =>
                termMatcher.FindSimilarTerms(searchTerm, candidates, maxSuggestions).ToList());

            if (suggestions.Any())
            {
                Console.WriteLine("  Suggestions:");
                foreach (var suggestion in suggestions)
                {
                    Console.WriteLine($"    - {suggestion}");
                }
            }
            else
            {
                Console.WriteLine("  Aucune suggestion trouvée.");
            }
        }
        private static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
            .ConfigureServices((_, services) =>
            {
                services.AddTermSuggestServices();
            });
        
    }
}
