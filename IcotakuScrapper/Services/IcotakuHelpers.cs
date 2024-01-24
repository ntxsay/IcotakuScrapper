using IcotakuScrapper.Extensions;

namespace IcotakuScrapper.Services
{
    public static partial class IcotakuHelpers
    {
        public static DiffusionStateKind GetDiffusionStateKind(string? value)
        {
            if (value == null || value.IsStringNullOrEmptyOrWhiteSpace())
                return DiffusionStateKind.Unknown;

            return value.ToLower() switch
            {
                "bientôt" or "bientot" => DiffusionStateKind.UpComing,
                "en cours" => DiffusionStateKind.InProgress,
                "en pause" => DiffusionStateKind.Paused,
                "terminée" or "terminé" or "terminee" or "termine" => DiffusionStateKind.Completed,
                "arrêtée" or "arrêté" or "arretee" or "arrete" => DiffusionStateKind.Stopped,
                _ => DiffusionStateKind.Unknown,
            };
        }

        public static string GetDiffusionStateLiteral(DiffusionStateKind stateKind)
            => stateKind switch
            {
                DiffusionStateKind.Unknown => "Inconnu",
                DiffusionStateKind.UpComing => "Bientôt",
                DiffusionStateKind.InProgress => "En cours",
                DiffusionStateKind.Paused => "En pause",
                DiffusionStateKind.Completed => "Terminé",
                DiffusionStateKind.Stopped => "Arrêté",
                _ => throw new ArgumentOutOfRangeException(nameof(stateKind), stateKind, "La valeur spécifiée est invalide")
            };
        
        public static string? GetDiffusionStateSearchPamareter(DiffusionStateKind stateKind)
            => stateKind switch
            {
                DiffusionStateKind.Unknown => null,
                DiffusionStateKind.UpComing => "bientot",
                DiffusionStateKind.InProgress => "en_cours",
                DiffusionStateKind.Paused => "en_pause",
                DiffusionStateKind.Completed => "terminee",
                DiffusionStateKind.Stopped => "arretee",
                _ => null
            };
    }
}
