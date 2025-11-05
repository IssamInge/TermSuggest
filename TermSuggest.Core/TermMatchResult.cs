namespace TermSuggest.Core
{
    /// <summary>
    /// Résultat d'un matching de terme avec métriques de similarité
    /// </summary>
    /// <param name="Term">Terme candidat</param>
    /// <param name="DifferenceCount">Nombre de différences</param>
    /// <param name="LengthDifference">Différence de longueur</param>
    public record TermMatchResult(string Term, int DifferenceCount, int LengthDifference);
}