-- region Table TsheetIndex
/*
 Création de la table TsheetIndex qui permet 
 d'indexer les adresses url des fiches des animés ou autres
 */
DROP TABLE IF EXISTS TsheetIndex;
CREATE TABLE IF NOT EXISTS TsheetIndex
(
    "Id"          INTEGER NOT NULL UNIQUE PRIMARY KEY AUTOINCREMENT,
    "SheetId"     INTEGER NOT NULL DEFAULT 0, -- Id de la fiche (anime, manga, etc)
    "Url"         TEXT    NOT NULL UNIQUE,    -- Url de la fiche (anime, manga, etc)
    "Section"     INTEGER NOT NULL,           -- Section de la fiche (ANime, Manga, etc)
    "SheetName"   TEXT    NOT NULL,           -- Nom de la fiche (anime, manga, etc)
    "SheetType"   INTEGER NOT NULL,           -- Type de la fiche (anime, manga, character, studios, individual, etc)
    "FoundedPage" INTEGER NOT NULL DEFAULT 0  -- Page de recherche sur laquelle la fiche a été trouvée
);
-- endregion

-- region Table TsheetStatistic
/*
 Création de la table TsheetIndex qui permet 
 d'indexer les adresses url des fiches des animés ou autres
 */
DROP TABLE IF EXISTS TsheetStatistic;
CREATE TABLE IF NOT EXISTS TsheetStatistic
(
    "Id"                          INTEGER NOT NULL UNIQUE PRIMARY KEY AUTOINCREMENT,
    "SheetId"                     INTEGER NOT NULL DEFAULT 0, -- Id de la fiche (anime, manga, etc)
    "Section"                     INTEGER NOT NULL,           -- Section de la fiche (ANime, Manga, etc)
    "CreatingDate"                TEXT    NULL,
    "LastUpdateDate"              TEXT    NULL,
    "CreatedBy"                   TEXT    NULL,
    "LastUpdatedBy"               TEXT    NULL,
    "InReadOrWatchListAverageAge" REAL    NULL,
    "VisitCount"                  INTEGER NOT NULL DEFAULT 0,
    "LastVisitedBy"               TEXT    NULL

);
-- endregion

-- region Table TsheetIndex
/*
 Création de la table TsheetIndex qui permet 
 d'indexer les adresses url des fiches des animés ou autres
 */
DROP TABLE IF EXISTS TsheetMostAwaitedPopular;
CREATE TABLE IF NOT EXISTS TsheetMostAwaitedPopular
(
    "Id"        INTEGER NOT NULL UNIQUE PRIMARY KEY AUTOINCREMENT,
    "SheetId"   INTEGER NOT NULL DEFAULT 0, -- Id de la fiche (anime, manga, etc)
    "Url"       TEXT    NOT NULL,           -- Url de la fiche (anime, manga, etc)
    "Section"   INTEGER NOT NULL,           -- Section de la fiche (ANime, Manga, etc)
    "SheetName" TEXT    NOT NULL,           -- Nom de la fiche (anime, manga, etc)
    "SheetType" INTEGER NOT NULL,           -- Type de la fiche (anime, manga, character, studios, individual, etc)
    "ListType"  INTEGER NOT NULL,           -- Type de liste (most awaited, most popular)
    "Rank"      INTEGER NOT NULL DEFAULT 0, -- Classement
    "VoteCount" INTEGER NOT NULL DEFAULT 0, -- Nombre de vote
    "Note"      REAL    NOT NULL DEFAULT 0  -- Note
);
-- endregion

-- region Table Tformat
/*
 Création de la table Tformat qui permet 
 d'enregistrer les formats (sous quelle forme sera présenté le produit) des fiches des animés ou autres
 */
DROP TABLE IF EXISTS Tformat;
CREATE TABLE IF NOT EXISTS Tformat
(
    Id          INTEGER NOT NULL UNIQUE PRIMARY KEY AUTOINCREMENT,
    Section     INTEGER NOT NULL, -- Section de la catégorie (ANime, Manga, etc)
    Name        TEXT    NOT NULL, -- Nom du format (tv, oav, etc)
    Description TEXT    NULL      -- description du format
);
-- endregion

-- region Table Ttarget
/*
 Création de la table Ttarget qui permet 
 d'enregistrer les cibles ou public visé dans les fiches des animés ou autres
 */
DROP TABLE IF EXISTS Ttarget;
CREATE TABLE IF NOT EXISTS Ttarget
(
    Id          INTEGER NOT NULL UNIQUE PRIMARY KEY AUTOINCREMENT,
    Section     INTEGER NOT NULL,        -- Section de la catégorie (ANime, Manga, etc)
    Name        TEXT    NOT NULL UNIQUE, -- Nom de la cible ou public visé (shonen, seinen, etc)
    Description TEXT    NULL
);
-- endregion

-- region Table TorigineAdaptation
/*
 Création de la table TorigineAdaptation qui permet 
 d'enregistrer les origines d'adaptation des fiches des animés ou autres
 */
DROP TABLE IF EXISTS TorigineAdaptation;
CREATE TABLE IF NOT EXISTS TorigineAdaptation
(
    Id          INTEGER NOT NULL UNIQUE PRIMARY KEY AUTOINCREMENT,
    Section     INTEGER NOT NULL,        -- Section de la catégorie (ANime, Manga, etc)
    Name        TEXT    NOT NULL UNIQUE, -- Nom de l'origine
    Description TEXT    NULL             -- description de l'origine 
);
-- endregion

-- region Table TlicenseType
/*
 Création de la table TlicenseType qui permet 
 d'enregistrer le type de licence des fiches des animés ou autres
 */
DROP TABLE IF EXISTS TlicenseType;
CREATE TABLE IF NOT EXISTS TlicenseType
(
    Id          INTEGER NOT NULL UNIQUE PRIMARY KEY AUTOINCREMENT,
    Section     INTEGER NOT NULL,        -- Section de la catégorie (ANime, Manga, etc)
    Name        TEXT    NOT NULL UNIQUE, -- Nom du type de licence (VOD, physique, simulcast, streaming, etc)
    Description TEXT    NULL
);
-- endregion

-- region Table Tcountry
/*
 Création de la table Tcountry qui permet 
 d'enregistrer les pays de production des fiches des animés ou autres
 */
DROP TABLE IF EXISTS Tcountry;
CREATE TABLE IF NOT EXISTS Tcountry
(
    Id       INTEGER NOT NULL UNIQUE PRIMARY KEY AUTOINCREMENT,
    Name     TEXT    NOT NULL UNIQUE, -- Nom du pays
    FileName TEXT    NULL             -- Nom du fichier du drapeau 
);
-- endregion

-- region Table Tcategory
/*
 Création de la table Tcategory qui permet 
 d'enregistrer les catégories (genres et thèmes) présent dans les fiches des animés ou autres
 */
DROP TABLE IF EXISTS Tcategory;
CREATE TABLE IF NOT EXISTS Tcategory
(
    "Id"             INTEGER NOT NULL UNIQUE PRIMARY KEY AUTOINCREMENT,
    "SheetId"        INTEGER NOT NULL DEFAULT 0, -- Id de la fiche
    "Url"            TEXT    NULL UNIQUE,        -- Url de la fiche
    "Name"           TEXT    NOT NULL,           -- Nom de la catégorie
    "Section"        INTEGER NOT NULL,           -- Section de la catégorie (ANime, Manga, etc)
    "Type"           INTEGER NOT NULL,           -- Type de la catégorie (genre ou thème)
    "Description"    TEXT    NULL,               -- description de la catégorie
    "IsFullyScraped" INTEGER NOT NULL DEFAULT 0  -- Est-ce que la catégorie a été entièrement scrapée ?
);
-- endregion

-- region Table ToeuvreRole
/*
 Création de la table ToeuvreRole qui permet 
 d'enregistrer les rôles des oeuvres (réalisateur, scénariste, etc) ou role de personnage (protagoniste, antagoniste, etc)
 */
DROP TABLE IF EXISTS ToeuvreRole;
CREATE TABLE IF NOT EXISTS ToeuvreRole
(
    Id          INTEGER NOT NULL UNIQUE PRIMARY KEY AUTOINCREMENT,
    Name        TEXT    NOT NULL, -- Nom du rôle
    Type        INTEGER NOT NULL, -- Type de rôle (staff ou personnage)
    Description TEXT    NULL      -- description du rôle
);
-- endregion

-- region Table Tseason
/*
 Création de la table Tseason qui permet 
 d'enregistrer les saisons pour une année donnée
 */
DROP TABLE IF EXISTS Tseason;
CREATE TABLE IF NOT EXISTS Tseason
(
    Id           INTEGER NOT NULL UNIQUE PRIMARY KEY AUTOINCREMENT,
    SeasonNumber INTEGER NOT NULL UNIQUE, -- 202301 (année + numéro de saison)
    DisplayName  TEXT    NOT NULL         -- Nom de la saison (printemps 2008, etc)
);
-- endregion

-- region Table TcontactGenre
/*
 Création de la table TcontactGenre qui permet 
 d'enregistrer les genres des contacts (fémimin, masculin, etc)
 */
DROP TABLE IF EXISTS TcontactGenre;
CREATE TABLE IF NOT EXISTS TcontactGenre
(
    Id          INTEGER NOT NULL UNIQUE PRIMARY KEY AUTOINCREMENT,
    Name        TEXT    NOT NULL UNIQUE, -- Nom du genre
    Description TEXT    NULL
);
-- endregion

-- region Table Tcontact
/*
 Création de la table Tcontact qui permet 
 d'enregistrer les informations des contacts (individu, personnage, etc) des fiches des animés ou autres
 */
DROP TABLE IF EXISTS Tcontact;
CREATE TABLE IF NOT EXISTS Tcontact
(
    "Id"           INTEGER NOT NULL UNIQUE PRIMARY KEY AUTOINCREMENT,
    "Guid"         TEXT    NOT NULL UNIQUE DEFAULT (lower(hex(randomblob(4))) || '-' || lower(hex(randomblob(2))) ||
                                                    '-4' || substr(lower(hex(randomblob(2))), 2) || '-' ||
                                                    substr('89ab', 1 + (abs(random()) % 4), 1) ||
                                                    substr(lower(hex(randomblob(2))), 2) || '-' ||
                                                    lower(hex(randomblob(6)))),
    "IdGenre"      INTEGER NULL REFERENCES TcontactGenre (Id) ON DELETE CASCADE,
    "SheetId"      INTEGER NOT NULL        DEFAULT 0, -- Id de la fiche (anime, manga, etc)
    "Url"          TEXT    NOT NULL UNIQUE,           -- Url de la fiche (anime, manga, etc)
    "Type"         INTEGER NOT NULL        DEFAULT 0, -- Type de la fiche (Individu, personnage, etc)
    "DisplayName"  TEXT    NOT NULL,
    "BirthName"    TEXT    NULL,
    "FirstName"    TEXT    NULL,
    "OriginalName" TEXT    NULL,
    "Age"          INTEGER NULL,
    "BirthDate"    TEXT    NULL,
    "Presentation" TEXT    NULL,
    "ThumbnailUrl" TEXT    NULL                       -- Url de l'image du personnage
);
-- endregion

-- region Table TcontactWebSite
/*
 Création de la table TcontactWebSite qui permet 
 d'enregistrer les sites web d'une fiche fiche contact
 */
DROP TABLE IF EXISTS TcontactWebSite;
CREATE TABLE IF NOT EXISTS TcontactWebSite
(
    Id          INTEGER NOT NULL UNIQUE PRIMARY KEY AUTOINCREMENT,
    IdContact   INTEGER NOT NULL REFERENCES Tcontact (Id) ON DELETE CASCADE,
    Url         TEXT    NOT NULL,
    Description TEXT    NULL
);
-- endregion

-- region Table Tanime
/*
 Création de la table Tanime qui permet 
 d'enregistrer les informations des animés
 */
DROP TABLE IF EXISTS Tanime;
CREATE TABLE IF NOT EXISTS Tanime
(
    "Id"                INTEGER NOT NULL UNIQUE PRIMARY KEY AUTOINCREMENT,
    "Guid"              TEXT    NOT NULL UNIQUE DEFAULT (lower(hex(randomblob(4))) || '-' ||
                                                         lower(hex(randomblob(2))) || '-4' ||
                                                         substr(lower(hex(randomblob(2))), 2) || '-' ||
                                                         substr('89ab', 1 + (abs(random()) % 4), 1) ||
                                                         substr(lower(hex(randomblob(2))), 2) || '-' ||
                                                         lower(hex(randomblob(6)))),
    "IdStatistic"       INTEGER NULL REFERENCES TsheetStatistic (Id) ON DELETE CASCADE,
    "IdTarget"          INTEGER NULL REFERENCES Ttarget (Id) ON DELETE CASCADE,
    "IdFormat"          INTEGER NULL REFERENCES Tformat (Id) ON DELETE CASCADE,
    "IdOrigine"         INTEGER NULL REFERENCES TorigineAdaptation (Id) ON DELETE CASCADE,
    "IdSeason"          INTEGER NULL REFERENCES Tseason (Id) ON DELETE CASCADE,
    "SheetId"           INTEGER NOT NULL        DEFAULT 0, -- Id de la fiche (anime, manga, etc)
    "Url"               TEXT    NOT NULL UNIQUE,           -- Url de la fiche (anime, manga, etc)
    "Name"              TEXT    NOT NULL,                  -- Nom de la fiche (anime, manga, etc)
    "Note"              REAL    NULL,                      -- Note de l'animé
    "VoteCount"         INTEGER NOT NULL        DEFAULT 0, -- Nombre de vote
    "IsAdultContent"    INTEGER NOT NULL        DEFAULT 0, -- Est-ce un anime pour adulte ?
    "IsExplicitContent" INTEGER NOT NULL        DEFAULT 0, -- Est-ce un anime qui contient des scènes particulièrement violentes ou sexuellement explicites sans pour autant être pornographique ?
    "EpisodeCount"      INTEGER NOT NULL        DEFAULT 0, -- Nombre d'épisode
    "EpisodeDuration"   REAL    NOT NULL        DEFAULT 0, -- Durée d'un épisode (en minute)
    "Season"            INTEGER NOT NULL        DEFAULT 0, -- Saison de l'animé
    "ReleaseMonth"      INTEGER NOT NULL        DEFAULT 0, -- annnée et mois de sortie (yyyymm)
    "ReleaseDate"       TEXT    NULL,                      -- Date de sortie de l'animé (yyyy-mm-dd)
    "EndDate"           TEXT    NULL,                      -- Date de fin de l'animé (yyyy-mm-dd)
    "DiffusionState"    INTEGER NOT NULL        DEFAULT 0, -- Etat de diffusion de l'animé
    "Description"       TEXT    NULL,                      -- Description de l'animé
    "ThumbnailUrl"      TEXT    NULL,                      -- Url de l'image de l'animé
    "Remark"            TEXT    NULL                       -- Remarque sur l'animé
);
-- endregion

-- region Table TanimeAlternativeTitle
/*
 Création de la table TanimeAlternativeTitle qui permet 
 d'enregistrer les titres alternatifs des fiches des animés
 */
DROP TABLE IF EXISTS TanimeAlternativeTitle;
CREATE TABLE IF NOT EXISTS TanimeAlternativeTitle
(
    Id          INTEGER NOT NULL UNIQUE PRIMARY KEY AUTOINCREMENT,
    IdAnime     INTEGER NOT NULL REFERENCES Tanime (Id) ON DELETE CASCADE,
    Title       TEXT    NOT NULL,
    Description TEXT    NULL -- exemple : titre anglais, titre original, etc
);
-- endregion

-- region Table tanimeWebSite
/*
 Création de la table tanimeWebSite qui permet 
 d'enregistrer les sites web d'une fiche fiche anime
 */
DROP TABLE IF EXISTS TanimeWebSite;
CREATE TABLE IF NOT EXISTS TanimeWebSite
(
    Id          INTEGER NOT NULL UNIQUE PRIMARY KEY AUTOINCREMENT,
    IdAnime     INTEGER NOT NULL REFERENCES Tanime (Id) ON DELETE CASCADE,
    Url         TEXT    NOT NULL,
    Description TEXT    NULL
);
-- endregion

-- region Table TanimeCategory
/*
 Création de la table TanimeCategory qui permet 
 de lier les catégories aux fiches anime
 */
DROP TABLE IF EXISTS TanimeCategory;
CREATE TABLE IF NOT EXISTS TanimeCategory
(
    Id         INTEGER NOT NULL UNIQUE PRIMARY KEY AUTOINCREMENT,
    IdAnime    INTEGER NOT NULL REFERENCES Tanime (Id) ON DELETE CASCADE,
    IdCategory INTEGER NOT NULL REFERENCES Tcategory (Id) ON DELETE CASCADE
);
-- endregion

-- region Table tanimeStudio
/*
 Création de la table tanimeStudio qui permet 
 de lier les studios aux fiches anime
 */
DROP TABLE IF EXISTS TanimeStudio;
CREATE TABLE IF NOT EXISTS TanimeStudio
(
    Id       INTEGER NOT NULL UNIQUE PRIMARY KEY AUTOINCREMENT,
    IdAnime  INTEGER NOT NULL REFERENCES Tanime (Id) ON DELETE CASCADE,
    IdStudio INTEGER NOT NULL REFERENCES Tcontact (Id) ON DELETE CASCADE
);
-- endregion

-- region Table tanimeLicence
/*
 Création de la table tanimeStudio qui permet 
 de lier les distributeurs aux fiches anime
 */
DROP TABLE IF EXISTS TanimeLicense;
CREATE TABLE IF NOT EXISTS TanimeLicense
(
    Id            INTEGER NOT NULL UNIQUE PRIMARY KEY AUTOINCREMENT,
    IdAnime       INTEGER NOT NULL REFERENCES Tanime (Id) ON DELETE CASCADE,
    IdDistributor INTEGER NOT NULL REFERENCES Tcontact (Id) ON DELETE CASCADE,
    IdLicenseType INTEGER NOT NULL REFERENCES TlicenseType (Id) ON DELETE CASCADE
);
-- endregion

-- region Table tanimeStaff
/*
 Création de la table tanimeStaff qui permet 
 de lier le staff aux fiches anime
 */
DROP TABLE IF EXISTS TanimeStaff;
CREATE TABLE IF NOT EXISTS TanimeStaff
(
    Id         INTEGER NOT NULL UNIQUE PRIMARY KEY AUTOINCREMENT,
    IdAnime    INTEGER NOT NULL REFERENCES Tanime (Id) ON DELETE CASCADE,
    IdIndividu INTEGER NOT NULL REFERENCES Tcontact (Id) ON DELETE CASCADE,
    IdRole     INTEGER NULL REFERENCES ToeuvreRole (Id) ON DELETE CASCADE
);
-- endregion

-- region Table tanimeCharacter
/*
 Création de la table tanimeCharacter qui permet 
 de lier les personnages aux fiches anime
 */
DROP TABLE IF EXISTS TanimeCharacter;
CREATE TABLE IF NOT EXISTS TanimeCharacter
(
    Id          INTEGER NOT NULL UNIQUE PRIMARY KEY AUTOINCREMENT,
    IdAnime     INTEGER NOT NULL REFERENCES Tanime (Id) ON DELETE CASCADE,
    IdCharacter INTEGER NOT NULL REFERENCES Tcontact (Id) ON DELETE CASCADE,
    IdRole      INTEGER NULL REFERENCES ToeuvreRole (Id) ON DELETE CASCADE
);

-- endregion

-- region Table TanimeEpisode
/*
 Création de la table TanimeEpisode qui permet 
 d'enregistrer les dates de diffusion des épisodes des animés
 */
DROP TABLE IF EXISTS TanimeEpisode;
CREATE TABLE IF NOT EXISTS TanimeEpisode
(
    Id          INTEGER NOT NULL UNIQUE PRIMARY KEY AUTOINCREMENT,
    IdAnime     INTEGER NOT NULL REFERENCES Tanime (Id) ON DELETE CASCADE,
    ReleaseDate TEXT    NOT NULL, -- Date de sortie de l'animé (yyyy-mm-dd)
    NoEpisode   INTEGER NOT NULL, -- Numéro de l'épisode
    EpisodeName TEXT    NULL,
    NoDay       INTEGER NOT NULL, -- Numéro du jour de diffusion
    Description TEXT    NULL
);
-- endregion

-- region Table TanimeDailyPlanning
/*
 Création de la table TanimePlanning qui permet 
 d'enregistrer les dates de diffusion des épisodes des animés
 */
DROP TABLE IF EXISTS TanimeDailyPlanning;
CREATE TABLE IF NOT EXISTS TanimeDailyPlanning
(
    Id                INTEGER NOT NULL UNIQUE PRIMARY KEY AUTOINCREMENT,
    SheetId           INTEGER NOT NULL,           -- Id de la fiche (anime, manga, etc)
    Url               TEXT    NOT NULL,           -- Url de la fiche (anime, manga, etc)
    AnimeName         TEXT    NOT NULL,           -- Nom de la fiche (anime, manga, etc)
    IsAdultContent    INTEGER NOT NULL DEFAULT 0, -- Est-ce un anime pour adulte ?
    IsExplicitContent INTEGER NOT NULL DEFAULT 0, -- Est-ce un anime qui contient des scènes particulièrement violentes ou sexuellement explicites sans pour autant être pornographique ?
    ReleaseDate       TEXT    NOT NULL,           -- Date de sortie de l'animé (yyyy-mm-dd)
    NoEpisode         INTEGER NOT NULL,           -- Numéro de l'épisode
    EpisodeName       TEXT    NULL,
    NoDay             INTEGER NOT NULL,           -- Numéro du jour de diffusion
    Description       TEXT    NULL,
    ThumbnailUrl      TEXT    NULL                -- Url de l'image de l'animé
);
-- endregion

-- region Table TanimeSeasonalPlanning
/*
 Création de la table TanimeSeasonalPlanning qui permet 
 d'enregistrer les dates de diffusion des épisodes des animés
 */
DROP TABLE IF EXISTS TanimeSeasonalPlanning;
CREATE TABLE IF NOT EXISTS TanimeSeasonalPlanning
(
    Id                INTEGER NOT NULL UNIQUE PRIMARY KEY AUTOINCREMENT,
    IdOrigine         INTEGER NULL REFERENCES TorigineAdaptation (Id) ON DELETE CASCADE,
    IdSeason          INTEGER NOT NULL REFERENCES Tseason (Id) ON DELETE CASCADE,
    SheetId           INTEGER NOT NULL,           -- Id de la fiche (anime, manga, etc)
    Url               TEXT    NOT NULL,           -- Url de la fiche (anime, manga, etc)
    GroupName         TEXT    NOT NULL,           --  Ex (Séries, OAV / OAD / Format court/Bonus, Films, ONA, Spéciaux)
    AnimeName         TEXT    NOT NULL,           -- Nom de la fiche (anime, manga, etc)
    IsAdultContent    INTEGER NOT NULL DEFAULT 0, -- Est-ce un anime pour adulte ?
    IsExplicitContent INTEGER NOT NULL DEFAULT 0, -- Est-ce un anime qui contient des scènes particulièrement violentes ou sexuellement explicites sans pour autant être pornographique ?
    Description       TEXT    NULL,
    Studios           TEXT    NULL,               -- Nom des studios (séparé par des virgules)
    Distributors      TEXT    NULL,               -- Nom des distributeurs (séparé par des virgules)
    ReleaseMonth      INTEGER NOT NULL DEFAULT 0, -- annnéé et mois de sortie (yymm)
    ThumbnailUrl      TEXT    NULL                -- Url de l'image de l'animé
);
-- endregion

-- region Table TanimeAlternativeTitle
/*
 Création de la table TanimeAlternativeTitle qui permet 
 d'enregistrer les titres alternatifs des fiches des animés
 */
DROP TABLE IF EXISTS TuserSheetNotation;
CREATE TABLE IF NOT EXISTS TuserSheetNotation
(
    "Id"             INTEGER NOT NULL UNIQUE PRIMARY KEY AUTOINCREMENT,
    "IdAnime"        INTEGER NULL REFERENCES Tanime (Id) ON DELETE CASCADE,
    "Section"        INTEGER NOT NULL, -- Section de la catégorie (ANime, Manga, etc)
    "SheetId"        INTEGER NOT NULL, -- Id de la fiche (anime, manga, etc)
    "WatchingStatus" INTEGER NOT NULL,
    "Note"           REAL    NULL,
    "PublicComment"  TEXT    NULL,
    "PrivateComment" TEXT    NULL
);
-- endregion
