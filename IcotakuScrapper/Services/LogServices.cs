using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace IcotakuScrapper.Services;

internal static class LogServices
{
    public static void LogDebug(string message, [CallerFilePath] string callerFilePath = "", [CallerMemberName] string caller = "")
    {
        Debug.WriteLine("--------------------Debut (Debug)--------------------");
        Debug.WriteLine($"CallerMember : {caller}");
        Debug.WriteLine($"Message : {message}");
        Debug.WriteLine($"Fichier : {callerFilePath}");
        Debug.WriteLine("--------------------Fin (Debug)--------------------");
        
        Console.WriteLine("--------------------Debut (Console)--------------------");
        Console.WriteLine($"CallerMember : {caller}");
        Console.WriteLine($"Message : {message}");
        Console.WriteLine($"Fichier : {callerFilePath}");
        Console.WriteLine("--------------------Fin (Console)--------------------");
    }

    public static void LogDebug(Exception exception, [CallerFilePath] string callerFilePath = "", [CallerMemberName] string caller = "")
    {
        Debug.WriteLine("--------------------Debut (Debug)--------------------");
        Debug.WriteLine($"CallerMember : {caller}");
        Debug.WriteLine($"Message : {exception?.Message}");
        Debug.WriteLine($"Message (Inner) : {exception?.InnerException?.Message}");
        Debug.WriteLine($"Stack : {exception?.StackTrace}");
        Debug.WriteLine($"Fichier : {callerFilePath}");
        Debug.WriteLine("--------------------Fin (Debug)--------------------");
        
        Console.WriteLine("--------------------Debut (Console)--------------------");
        Console.WriteLine($"CallerMember : {caller}");
        Console.WriteLine($"Message : {exception?.Message}");
        Console.WriteLine($"Message (Inner) : {exception?.InnerException?.Message}");
        Console.WriteLine($"Stack : {exception?.StackTrace}");
        Console.WriteLine($"Fichier : {callerFilePath}");
        Console.WriteLine("--------------------Fin (Console)--------------------");
    }
}