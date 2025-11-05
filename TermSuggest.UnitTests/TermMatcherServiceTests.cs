using TermSuggest.Services;

namespace TermSuggest.UnitTests
{
    public class TermMatcherServiceTests
    {
        private readonly TermMatcherService _termMatcher;

        public TermMatcherServiceTests()
        {
            _termMatcher = new TermMatcherService();
        }

        #region Tests de Validation

        [Fact]
        public void FindSimilarTerms_WithNullSearchTerm_ThrowsArgumentException()
        {
            // Arrange
            var candidates = new[] { "test" };
            string nullSearchTerm = null!;

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() =>
                _termMatcher.FindSimilarTerms(nullSearchTerm, candidates, 5));

            Assert.Equal("Le terme de recherche ne peut pas être vide (Parameter 'searchTerm')", exception.Message);
        }

        [Fact]
        public void FindSimilarTerms_WithEmptySearchTerm_ThrowsArgumentException()
        {
            // Arrange
            var candidates = new[] { "test" };

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() =>
                _termMatcher.FindSimilarTerms("", candidates, 5));

            Assert.Equal("Le terme de recherche ne peut pas être vide (Parameter 'searchTerm')", exception.Message);
        }

        [Fact]
        public void FindSimilarTerms_WithWhitespaceSearchTerm_ThrowsArgumentException()
        {
            // Arrange
            var candidates = new[] { "test" };

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() =>
                _termMatcher.FindSimilarTerms("   ", candidates, 5));

            Assert.Equal("Le terme de recherche ne peut pas être vide (Parameter 'searchTerm')", exception.Message);
        }

        [Fact]
        public void FindSimilarTerms_WithNullCandidates_ThrowsArgumentNullException()
        {
            // Arrange
            var searchTerm = "test";
            IEnumerable<string> nullCandidates = null!;
            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() =>
                _termMatcher.FindSimilarTerms(searchTerm, nullCandidates, 5));

            Assert.Equal("candidates", exception.ParamName);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(-5)]
        public void FindSimilarTerms_WithInvalidMaxSuggestions_ThrowsArgumentException(int maxSuggestions)
        {
            // Arrange
            var candidates = new[] { "test" };

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() =>
                _termMatcher.FindSimilarTerms("test", candidates, maxSuggestions));

            Assert.Equal("Le nombre de suggestions doit être positif (Parameter 'maxSuggestions')", exception.Message);
        }

        #endregion

        #region Tests Fonctionnels - Scénarios Métier

        [Fact]
        public void FindSimilarTerms_WithExactExampleFromRequirements_ReturnsExpectedResults()
        {
            // Arrange - Scénario exact de l'énoncé
            var searchTerm = "gros";
            var candidates = new[] { "gros", "gras", "graisse", "agressif", "go", "ros", "gro" };
            var expected = new[] { "gros", "gras" };

            // Act
            var result = _termMatcher.FindSimilarTerms(searchTerm, candidates, 2).ToList();

            // Assert
            Assert.Equal(expected, result);
        }

        [Fact]
        public void FindSimilarTerms_WithPerfectMatch_ReturnsExactTermFirst()
        {
            // Arrange
            var searchTerm = "test";
            var candidates = new[] { "test", "tent", "rest" };

            // Act
            var result = _termMatcher.FindSimilarTerms(searchTerm, candidates, 3).ToList();

            // Assert
            Assert.Equal("test", result[0]);
        }

        [Fact]
        public void FindSimilarTerms_WithMultipleSimilarTerms_ReturnsBestMatches()
        {
            // Arrange - Tous les termes ont au moins 5 caractères
            var searchTerm = "hello";
            var candidates = new[] { "hello", "hallo", "hells", "helli", "helpo" };

            // Act
            var result = _termMatcher.FindSimilarTerms(searchTerm, candidates, 5).ToList();

            // Assert - Tous les 5 termes sont valides
            Assert.Equal(5, result.Count);
            Assert.Equal("hello", result[0]);  // 0 différence
            Assert.Contains("hallo", result);  // 1 différence
            Assert.Contains("hells", result);  // 1 différence
            Assert.Contains("helli", result);  // 1 différence  
            Assert.Contains("helpo", result);  // 1 différence
        }

        #endregion

        #region Tests de Comportement - Tri et Filtrage

        [Fact]
        public void FindSimilarTerms_SortsByDifferenceCountFirst()
        {
            // Arrange - Différences claires
            var searchTerm = "test";
            var candidates = new[] { "test", "tent", "rest", "best", "texts" };

            // Act
            var result = _termMatcher.FindSimilarTerms(searchTerm, candidates, 5).ToList();

            // Assert - Ordre par nombre de différences
            Assert.Equal("test", result[0]);  // 0 différence
            Assert.Equal("best", result[1]);  // 1 différence (b)
            Assert.Equal("rest", result[2]);  // 1 différence (r)  
            Assert.Equal("tent", result[3]);  // 1 différence (t)
        }

        [Fact]
        public void FindSimilarTerms_WithTieInDifferences_SortsByLengthDifference()
        {
            // Arrange - Différences de longueur claires
            var searchTerm = "test";
            var candidates = new[] { "test", "tests", "testing", "tent" };

            // Act
            var result = _termMatcher.FindSimilarTerms(searchTerm, candidates, 4).ToList();

            // Assert
            Assert.Equal("test", result[0]);    // 0 diff, longueur 4
            Assert.Equal("tests", result[1]);   // 0 diff, longueur 5
            Assert.Equal("testing", result[2]); // 0 diff, longueur 7  
            Assert.Equal("tent", result[3]);    // 1 diff, longueur 4
        }

        [Fact]
        public void FindSimilarTerms_WithTieInDifferencesAndLength_SortsAlphabetically()
        {
            // Arrange
            var searchTerm = "test";
            var candidates = new[] { "best", "rest", "nest", "west" };

            // Act
            var result = _termMatcher.FindSimilarTerms(searchTerm, candidates, 4).ToList();

            // Assert - Tous ont 1 différence et même longueur, tri alphabétique
            Assert.Equal("best", result[0]);
            Assert.Equal("nest", result[1]);
            Assert.Equal("rest", result[2]);
            Assert.Equal("west", result[3]);
        }

        [Fact]
        public void FindSimilarTerms_FiltersTermsWithTooManyDifferences()
        {
            // Arrange
            var searchTerm = "test"; // longueur 4
            var candidates = new[] { "good", "test", "tent", "completelydifferent" };

            // Act
            var result = _termMatcher.FindSimilarTerms(searchTerm, candidates, 5).ToList();

            // Assert - "completelydifferent" a trop de différences (> 4/2 = 2)
            Assert.DoesNotContain("completelydifferent", result);
            Assert.DoesNotContain("good", result);
        }

        [Fact]
        public void FindSimilarTerms_FiltersTermsThatAreTooShort()
        {
            // Arrange
            var searchTerm = "test"; // longueur 4
            var candidates = new[] { "te", "tes", "test", "testing" };

            // Act
            var result = _termMatcher.FindSimilarTerms(searchTerm, candidates, 5).ToList();

            // Assert - "te" et "tes" sont trop courts (< 4/2 = 2)
            Assert.DoesNotContain("te", result);
            Assert.DoesNotContain("tes", result);
            Assert.Contains("test", result);
            Assert.Contains("testing", result);
        }

        #endregion

        #region Tests de Normalisation

        [Fact]
        public void FindSimilarTerms_NormalizesInputToLowerCase()
        {
            // Arrange
            var searchTerm = "TEST";
            var candidates = new[] { "TEST", "test", "Test", "test" }; // "test" en double

            // Act
            var result = _termMatcher.FindSimilarTerms(searchTerm, candidates, 5).ToList();

            // Assert - Tous en minuscules et dédupliqués
            Assert.All(result, term => Assert.Equal(term.ToLowerInvariant(), term));
            Assert.Single(result); // Un seul "test" après déduplication
            Assert.Equal("test", result[0]);
        }

        [Fact]
        public void FindSimilarTerms_RemovesDuplicateCandidates()
        {
            // Arrange
            var searchTerm = "test";
            var candidates = new[] { "test", "test", "TEST", "tent" }; // "tent" a 1 différence

            // Act
            var result = _termMatcher.FindSimilarTerms(searchTerm, candidates, 5).ToList();

            // Assert - Les doublons sont éliminés, "tent" est valide
            Assert.Equal(2, result.Count); // "test" (dédupliqué) et "tent"
            Assert.Contains("test", result);
            Assert.Contains("tent", result);
        }

        [Fact]
        public void FindSimilarTerms_IgnoresNullOrEmptyCandidates()
        {
            // Arrange
            var searchTerm = "test";
            var candidates = new[] { "test", "", "  ", null,"valid", "tent" };

            // Act
            var result = _termMatcher.FindSimilarTerms(searchTerm, candidates, 6).ToList();

            // Assert - Les candidats null ou vides sont ignorés
            Assert.Equal(2, result.Count);
            Assert.Contains("test", result);
            Assert.Contains("tent", result);
        }

        #endregion
    }
}