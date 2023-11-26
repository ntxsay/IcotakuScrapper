-- region Table TsheetIndex
/*
 Création de la table TsheetIndex qui permet 
 d'indexer les adresses url des fiches des animés ou autres
 */
DROP TABLE IF EXISTS TsheetIndex;
CREATE TABLE IF NOT EXISTS TsheetIndex
(
    Id          INTEGER NOT NULL UNIQUE PRIMARY KEY AUTOINCREMENT,
    SheetId     INTEGER NOT NULL DEFAULT 0, -- Id de la fiche (anime, manga, etc)
    Url         TEXT    NOT NULL UNIQUE,    -- Url de la fiche (anime, manga, etc)
    Name        TEXT    NULL,               -- Nom de la fiche (anime, manga, etc)
    Type        INTEGER NOT NULL,           -- Type de la fiche (anime, manga, etc)
    FoundedPage INTEGER NOT NULL DEFAULT 0  -- Page de recherche sur laquelle la fiche a été trouvée
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
    Name        TEXT    NOT NULL UNIQUE, -- Nom du format (tv, oav, etc)
    Description TEXT    NULL             -- description du format
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
    Name        TEXT    NOT NULL UNIQUE, -- Nom de la cible ou public visé (shonen, seinen, etc)
    Description TEXT    NULL
);
-- endregion

-- region Table TlicenceType
/*
 Création de la table TlicenceType qui permet 
 d'enregistrer le type de licence des fiches des animés ou autres
 */
DROP TABLE IF EXISTS TlicenceType;
CREATE TABLE IF NOT EXISTS TlicenceType
(
    Id          INTEGER NOT NULL UNIQUE PRIMARY KEY AUTOINCREMENT,
    Name        TEXT    NOT NULL UNIQUE, -- Nom du type de licence (VOD, physique, simulcast, streaming, etc)
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
    Name        TEXT    NOT NULL UNIQUE, -- Nom de l'origine
    Description TEXT    NULL             -- description de l'origine 
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
    Name     TEXT    NOT NULL UNIQUE, -- Nom de l'origine
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
    Id          INTEGER NOT NULL UNIQUE PRIMARY KEY AUTOINCREMENT,
    Name        TEXT    NOT NULL, -- Nom de la catégorie
    Type        INTEGER NOT NULL, -- Type de la catégorie (genre ou thème)
    Description TEXT    NULL      -- description de la catégorie
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
    Id           INTEGER NOT NULL UNIQUE PRIMARY KEY AUTOINCREMENT,
    IdGenre      INTEGER NULL REFERENCES TcontactGenre (Id) ON DELETE CASCADE,
    SheetId      INTEGER NOT NULL DEFAULT 0, -- Id de la fiche (anime, manga, etc)
    Url          TEXT    NOT NULL UNIQUE,    -- Url de la fiche (anime, manga, etc)
    Type         INTEGER NOT NULL DEFAULT 0, -- Type de la fiche (Individu, personnage, etc)
    DisplayName  TEXT    NOT NULL,
    BirthName    TEXT    NULL,
    FirstName    TEXT    NULL,
    OriginalName TEXT    NULL,
    Age          INTEGER NULL,
    BirthDate    TEXT    NULL,
    Presentation TEXT    NULL
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
    Id             INTEGER NOT NULL UNIQUE PRIMARY KEY AUTOINCREMENT,
    IdTarget       INTEGER NULL REFERENCES Ttarget (Id) ON DELETE CASCADE,
    IdFormat       INTEGER NULL REFERENCES Tformat (Id) ON DELETE CASCADE,
    IdOrigine      INTEGER NULL REFERENCES TorigineAdaptation (Id) ON DELETE CASCADE,
    SheetId        INTEGER NOT NULL DEFAULT 0, -- Id de la fiche (anime, manga, etc)
    Url            TEXT    NOT NULL UNIQUE,    -- Url de la fiche (anime, manga, etc)
    Name           TEXT    NULL,               -- Nom de la fiche (anime, manga, etc)
    EpisodeCount   INTEGER NULL,               -- Nombre d'épisode
    EpisodeTime    REAL    NULL,               -- Durée d'un épisode (en minute)
    Season         INTEGER NOT NULL DEFAULT 0, -- Saison de l'animé
    Year           INTEGER NOT NULL DEFAULT 0, -- Année de sortie de l'animé
    Month          INTEGER NOT NULL DEFAULT 0, -- Mois de sortie de l'animé
    DiffusionState INTEGER NOT NULL DEFAULT 0, -- Etat de diffusion de l'animé
    Description    TEXT    NULL,               -- Description de l'animé
    Remark         TEXT    NULL                -- Remarque sur l'animé
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
    Name        TEXT    NOT NULL,
    Description TEXT    NULL -- exemple : titre anglais, titre original, etc
);
-- endregion

-- region Table tanimeWebSite
/*
 Création de la table tanimeWebSite qui permet 
 d'enregistrer les sites web d'une fiche fiche anime
 */
DROP TABLE IF EXISTS tanimeWebSite;
CREATE TABLE IF NOT EXISTS tanimeWebSite
(
    Id          INTEGER NOT NULL UNIQUE PRIMARY KEY AUTOINCREMENT,
    IdAnime     INTEGER NOT NULL REFERENCES Tanime (Id) ON DELETE CASCADE,
    Url         TEXT    NOT NULL,
    Description TEXT    NULL
);
-- endregion

-- region Table tanimeStudio
/*
 Création de la table tanimeStudio qui permet 
 de lier les studios aux fiches anime
 */
DROP TABLE IF EXISTS tanimeStudio;
CREATE TABLE IF NOT EXISTS tanimeStudio
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
DROP TABLE IF EXISTS tanimeLicence;
CREATE TABLE IF NOT EXISTS tanimeLicence
(
    Id            INTEGER NOT NULL UNIQUE PRIMARY KEY AUTOINCREMENT,
    IdAnime       INTEGER NOT NULL REFERENCES Tanime (Id) ON DELETE CASCADE,
    IdDistributor INTEGER NOT NULL REFERENCES Tcontact (Id) ON DELETE CASCADE,
    IdLicenceType INTEGER NULL REFERENCES TlicenceType (Id) ON DELETE CASCADE
);
-- endregion

-- region Table tanimeStaff
/*
 Création de la table tanimeStaff qui permet 
 de lier le staff aux fiches anime
 */
DROP TABLE IF EXISTS tanimeStaff;
CREATE TABLE IF NOT EXISTS tanimeStaff
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
DROP TABLE IF EXISTS tanimeCharacter;
CREATE TABLE IF NOT EXISTS tanimeCharacter
(
    Id          INTEGER NOT NULL UNIQUE PRIMARY KEY AUTOINCREMENT,
    IdAnime     INTEGER NOT NULL REFERENCES Tanime (Id) ON DELETE CASCADE,
    IdCharacter INTEGER NOT NULL REFERENCES Tcontact (Id) ON DELETE CASCADE,
    IdRole      INTEGER NULL REFERENCES ToeuvreRole (Id) ON DELETE CASCADE
);