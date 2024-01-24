using System.Runtime.CompilerServices;

namespace IcotakuScrapper.Objects.Exceptions;

/// <summary>
/// Représente une exception levée lorsqu'une valeur de l'énumération <see cref="IntColumnSelect"/> n'est pas supportée.
/// </summary>
/// <param name="message"></param>
public class IntColumnSelectException(string message) : Exception(message)
{
    /// <summary>
    /// Lève une exception si la valeur de l'argument n'est pas supportée.
    /// </summary>
    /// <param name="argument"></param>
    /// <param name="paramName"></param>
    /// <param name="supportedValues"></param>
    /// <exception cref="IntColumnSelectException"></exception>
    internal static void ThrowNotSupportedException(IntColumnSelect argument, [CallerArgumentExpression(nameof(argument))] string? paramName = null, params IntColumnSelect[] supportedValues)
    {
        if (supportedValues.Length == 0 || supportedValues.Contains(argument)) 
            return;
        throw new IntColumnSelectException($"La valeur {argument} contenue dans \"{paramName}\" n'est pas supportée. Valeurs supportées : \"{string.Join(", ", supportedValues)}\"");
    }
}