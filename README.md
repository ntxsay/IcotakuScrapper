# Bienvenue dans le projet Icotaku Scraper

![Logo d'icotaku](https://communaute.icotaku.com/images/communaute/logo.png)

## Avant de commencer

Cet outils a été développé car le site Icotaku.com permet l’obtention de ses informations librement à condition de citer la source, je cite :

> Puis-je utiliser le contenu d'Icotaku sur mon site ?
>
> A condition de citer la source, vous pouvez utiliser/citer les
> synopsis/informations de Icotaku. Les textes d'Icotaku sont
> disponibles sous les termes de la licence de documentation libre GNU
> (GFDL). Attention ! Toutes les œuvres présentées sur Icotaku
> appartiennent à leurs auteurs respectifs.
> [Voir la FAQ Icotaku](https://communaute.icotaku.com/faq.html#6)

Je tiens à préciser que ce projet est une initiative personnelle et n'a pas été mandaté ou approuvé par Icotaku.com. L'utilisation du nom et du logo d'Icotaku.com dans le projet actuel est strictement limitée à une citation de source conforme à leurs directives.

## Présentation du projet
Ce projet permet l'obtention et le stockage local de synopsis d'animés (pour le moment) depuis le site web de l'association Icotaku vers une base de données SQLite.

### Récupérer les informations d'un animé

```csharp
//Récupère les informations de l'anime via l'url de la fiche
OperationState<int> animeCreationResult = await Tanime.ScrapFromUrlAsync(new Uri("https://anime.icotaku.com/anime/5633/Dr-STONE.html"));

//Vérifie que l'opération s'est bien déroulée
Console.WriteLine(animeCreationResult.IsSuccess);

//Obtient des informations supplémentaires sur l'opération
Console.WriteLine(animeCreationResult.Message);

//Obtient l'id (SQLite) de l'anime
Console.WriteLine(animeCreationResult.Data);
```

### Afficher les informations d'un animé

```csharp
//Récupère les informations de l'anime précédement "scrapé" via l'url de la fiche
Tanime? anime = await Tanime.SingleAsync(new Uri("https://anime.icotaku.com/anime/5633/Dr-STONE.html"));

if (anime is null)
{
    Console.WriteLine("L'anime n'a pas été trouvé");
    return;
}

//Obtient le nom de l'anime
Console.WriteLine(anime.Name);

//Obtient le nombre d'épisodes
Console.WriteLine(anime.EpisodesCount);

//obtient le synopsis
Console.WriteLine(anime.Description);
```


## Contribution
Je ne suis pas (spécialement) un ingénieur en la matière mais si vous trouvez que ce projet mérite une contribution, alors on y va ensemble avec ntxsay from GP.

## Soutenez Icotaku.com
Sauf erreur de ma part mais je n'ai trouvé aucun autre site ou plateforme assez bien structuré(e) et permettant l'utilisation de leur ressource gratuitement et sans contrepartie contraignante. Vraiment un grand merci Icotaku !

## Utilisation
Ce projet est libre, vous pouvez le modifier, simplement n'oubliez pas que la documentation d'Icotaku.com est sous licence de documentation libre GNU (GFDL).
