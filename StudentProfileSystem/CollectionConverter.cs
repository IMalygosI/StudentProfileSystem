using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Avalonia.Data.Converters;
using StudentProfileSystem.Models;

namespace StudentProfileSystem.Converters
{
    /// <summary>
    /// Конвертер для преобразования коллекций в строку с разделителем " , "
    /// </summary>
    public class CollectionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            const string defaultSeparator = ", ";
            string separator = parameter as string ?? defaultSeparator;

            switch (value)
            {
                case IEnumerable<StudentGiaResult> giaResults:
                    return string.Join(Environment.NewLine, // Используем перенос строки вместо запятой
                        giaResults.Select(g =>
                            $"{g.IdGiaSubjectsNavigation?.GiaSubjectsNavigation?.Name} " +
                            $"({g.IdGiaSubjectsNavigation?.GiaType?.Name})")
                                  .Where(name => !string.IsNullOrEmpty(name)));

                case IEnumerable<StudentOlympiadParticipation> olympiads:
                    return string.Join(Environment.NewLine,
                        olympiads.Select(o =>
                            $"{o.IdOlympiadsNavigation?.OlympiadsNavigation?.Name} " +
                            $"({o.IdOlympiadsNavigation?.OlympiadsItemsNavigation?.Name})")
                                 .Where(name => !string.IsNullOrEmpty(name)));

                case IEnumerable collection when !(collection is string):
                    return string.Join(separator,
                        collection.Cast<object>()
                                  .Select(x => x?.ToString())
                                  .Where(s => !string.IsNullOrEmpty(s)));

                default:
                    return string.Empty;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}