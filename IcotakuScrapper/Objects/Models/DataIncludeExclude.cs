using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace IcotakuScrapper.Objects.Models
{
    /// <summary>
    /// Enumération permettant de gérer les données à inclure ou exclure dans une recherche
    /// </summary>
    public enum IncludeDataMode
    {
        /// <summary>
        /// Que l'élément apparaisse ou non dans les résultats importe peu.
        /// </summary>
        Ignore,
        /// <summary>
        /// La données est incluse dans la recherche de résultats
        /// </summary>
        Include,
        /// <summary>
        /// La données est exclue de la recherche de résultats
        /// </summary>
        Exclude,
    }

    /// <summary>
    /// Classe permettant de gérer les données à inclure ou exclure dans une recherche
    /// </summary>
    public class DataIncludeExclude : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged = delegate { };

        public DataIncludeExclude()
        {

        }

        public DataIncludeExclude(IncludeDataMode mode, string header, object? data, string? description)
        {
            Mode = mode;
            Header = header;
            Data = data;
            Description = description;
        }

        public DataIncludeExclude(object? data, string header, string? description)
        {
            Data = data;
            Header = header;
            Description = description;
        }

        protected IncludeDataMode _Mode;
        public virtual IncludeDataMode Mode
        {
            get => _Mode;
            set
            {
                if (_Mode != value)
                {
                    _Mode = value;
                    _IsIncluded = value switch
                    {
                        IncludeDataMode.Ignore => null,
                        IncludeDataMode.Include => true,
                        IncludeDataMode.Exclude => false,
                        _ => null,
                    };
                    OnPropertyChanged();
                }
            }
        }

        protected bool? _IsIncluded;
        public virtual bool? IsIncluded
        {
            get => _IsIncluded;
            set
            {
                if (_IsIncluded != value)
                {
                    _IsIncluded = value;
                    _Mode = value switch
                    {
                        null => IncludeDataMode.Ignore,
                        true => IncludeDataMode.Include,
                        false => IncludeDataMode.Exclude,
                    };
                    OnPropertyChanged();
                }
            }
        }

        protected string _Header = string.Empty;
        public virtual string Header
        {
            get => _Header;
            set
            {
                if (_Header != value)
                {
                    _Header = value;
                    OnPropertyChanged();
                }
            }
        }

        protected object? _Data;
        public virtual object? Data
        {
            get => _Data;
            set
            {
                if (_Data != value)
                {
                    _Data = value;
                    OnPropertyChanged();
                }
            }
        }

        protected string? _Description;
        public virtual string? Description
        {
            get => _Description;
            set
            {
                if (_Description != value)
                {
                    _Description = value;
                    OnPropertyChanged();
                }
            }
        }

        public void OnPropertyChanged([CallerMemberName] string? propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    /// <summary>
    /// Structure permettant de gérer les données à inclure ou exclure dans une recherche
    /// </summary>
    public readonly struct DataIncludeExcludeStruct
    {
        public IncludeDataMode Mode { get; init; }
        public string Header { get; init; } = "";
        public object? Data { get; init; }
        public string? Description { get; init; }

        public DataIncludeExcludeStruct()
        {

        }

        public DataIncludeExcludeStruct(IncludeDataMode mode, string header, object? data, string? description)
        {
            Mode = mode;
            Header = header;
            Data = data;
            Description = description;
        }
    }
}
