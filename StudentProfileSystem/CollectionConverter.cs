using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Avalonia.Data.Converters;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using StudentProfileSystem.Models;
using Avalonia;

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

    /// <summary>
    /// Конвертер для проверки, относится ли класс к ГИА (9 или 11 класс)
    /// </summary>
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
    /// <summary>
    /// Конвертер для проверки наличия элементов в коллекции
    /// </summary>
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

    /// <summary>
    /// Конвертер для преобразования между DateOnly и DateTime
    /// </summary>
    public class DateOnlyConverter : ValueConverter<DateOnly, DateTime>
    {
        public DateOnlyConverter() : base(
            dateOnly => dateOnly.ToDateTime(TimeOnly.MinValue),
            dateTime => DateOnly.FromDateTime(dateTime))
        { }
    }

    public class HasGiaConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is ICollection<StudentGiaResult> giaResults)
            {
                return giaResults.Any();
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class AndMultiConverter : IMultiValueConverter
    {
        public object Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
        {
            return values.OfType<bool>().All(b => b);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Конвертер для проверки, что значение не равно 0
    /// </summary>
    public class NotZeroConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is int id && id != 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }


}