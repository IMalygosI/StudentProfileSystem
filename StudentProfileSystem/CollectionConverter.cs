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
                    return string.Join(Environment.NewLine, giaResults.Select(g => $"{g.IdGiaSubjectsNavigation?.GiaSubjectsNavigation?.Name} ")
                                                                      .Where(name => !string.IsNullOrEmpty(name)));

                case IEnumerable<StudentOlympiadParticipation> olympiads:
                    // Группируем по названию олимпиады и объединяем предметы через запятую
                    var groupedOlympiads = olympiads
                        .Where(o => o.IdOlympiadsNavigation?.OlympiadsNavigation != null &&
                                   o.IdOlympiadsNavigation?.OlympiadsItemsNavigation != null)
                        .GroupBy(o => o.IdOlympiadsNavigation.OlympiadsNavigation.Name)
                        .Select(g => $"{g.Key} ({string.Join(", ", g.Select(o => o.IdOlympiadsNavigation.OlympiadsItemsNavigation.Name))})");

                    return string.Join(Environment.NewLine, groupedOlympiads);

                case IEnumerable collection when !(collection is string):
                    return string.Join(separator, collection.Cast<object>().Select(x => x?.ToString())
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
    public class IsGiaClassConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string classString)
            {
                var match = System.Text.RegularExpressions.Regex.Match(classString, @"^\d{1,2}");
                if (match.Success)
                {
                    string numericPart = match.Value;
                    return numericPart == "9" || numericPart == "11";
                }
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class HasItemsConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is IEnumerable<StudentOlympiadParticipation> olympiads)
            {
                return olympiads.Any();
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}