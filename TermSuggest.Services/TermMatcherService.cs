using TermSuggest.Core;

namespace TermSuggest.Services
{
    /// <summary>
    /// Service principal de matching de termes avec calcul de similarité intégré
    /// </summary>
    public class TermMatcherService : ITermMatcher
    {
        public IEnumerable<string> FindSimilarTerms(string searchTerm, IEnumerable<string> candidates, int maxSuggestions)
        {
            ValidateInput(searchTerm, candidates, maxSuggestions);
            var normalizedSearchTerm = searchTerm.ToLowerInvariant().Trim();
            var normalizedCandidates = NormalizeCandidates(candidates);
            var matches = CalculateMatches(normalizedSearchTerm, normalizedCandidates);
            var validMatches = FilterValidMatches(matches, normalizedSearchTerm);
            var sortedMatches = SortMatches(validMatches);

            return sortedMatches.Select(m => m.Term).Take(maxSuggestions);
        }

        private void ValidateInput(string searchTerm, IEnumerable<string> candidates, int maxSuggestions)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                throw new ArgumentException("Le terme de recherche ne peut pas être vide", nameof(searchTerm));
            if(candidates is null)
                throw new ArgumentNullException(nameof(candidates), "La liste des candidats ne peut pas être nulle");
            if(maxSuggestions <=0)
                throw new ArgumentException("Le nombre de suggestions doit être positif", nameof(maxSuggestions));
        }

        private static IEnumerable<string> NormalizeCandidates(IEnumerable<string> candidates)
        {
            return candidates
           .Where(c => !string.IsNullOrWhiteSpace(c))
           .Select(c => c.ToLowerInvariant().Trim())
           .Distinct();
        }

        private IEnumerable<TermMatchResult> CalculateMatches(string searchTerm, IEnumerable<string> candidates)
        {
            return candidates.Select(candidate =>
            {
                var differences = CalculateDifferences(searchTerm, candidate);
                var lengthDiff = Math.Abs(searchTerm.Length - candidate.Length);
                return new TermMatchResult(candidate, differences, lengthDiff);
            });
        }

        /// <summary>
        /// Calcule le nombre de différences entre deux termes
        /// </summary>
        private int CalculateDifferences(string source, string target)
        {
            if (string.IsNullOrEmpty(source) || string.IsNullOrEmpty(target))
                return int.MaxValue;

            // Le terme cible doit être au moins aussi long que la source
            if (target.Length < source.Length)
                return int.MaxValue;

            int differences = 0;

            // Compare caractère par caractère
            for (int i = 0; i < source.Length; i++)
            {
                if (i >= target.Length)
                {
                    differences += source.Length - i;
                    break;
                }

                if (source[i] != target[i])
                    differences++;
            }

            return differences;
        }

        private static IEnumerable<TermMatchResult> FilterValidMatches(IEnumerable<TermMatchResult> matches, string searchTerm)
        {
            return matches.Where(match => IsValidMatch(match, searchTerm));
        }

        private static bool IsValidMatch(TermMatchResult match, string searchTerm)
        {
            // Élimine les matches avec trop de différences
            if (match.DifferenceCount > searchTerm.Length / 2)
                return false;

            // Élimine les termes trop courts
            if (match.Term.Length < searchTerm.Length / 2)
                return false;

            return true;
        }
        private static IEnumerable<TermMatchResult> SortMatches(IEnumerable<TermMatchResult> matches)
        {
            return matches
                .OrderBy(m => m.DifferenceCount)    // D'abord par nombre de différences
                .ThenBy(m => m.LengthDifference)    // Puis par différence de longueur
                .ThenBy(m => m.Term);               // Enfin par ordre alphabétique
        }

    }
}
