namespace TermSuggest.Core
{
    /// <summary>
    /// Service de matching de termes avec similarité
    /// </summary>
    public interface ITermMatcher
    {
        /// <summary>
        /// Trouve les termes les plus similaires au terme recherché
        /// </summary>
        /// <param name="searchTerm">Terme à rechercher</param>
        /// <param name="candidates">Liste des termes candidats</param>
        /// <param name="maxSuggestions">Nombre maximum de suggestions</param>
        /// <returns>Termes suggérés triés par pertinence</returns>
        IEnumerable<string> FindSimilarTerms(string searchTerm, IEnumerable<string> candidates, int maxSuggestions);
    }
}
