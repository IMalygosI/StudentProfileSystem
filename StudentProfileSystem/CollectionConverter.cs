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
        /// <summary>
        /// Преобразует коллекцию в строку
        /// </summary>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Разделитель по умолчанию - запятая
            const string defaultSeparator = ", ";
            string separator = parameter as string ?? defaultSeparator;

            switch (value)
            {
                // Обработка предметов ГИА
                case IEnumerable<StudentGiaResult> giaResults:
                    return string.Join(separator,
                        giaResults.Select(g => g.IdGiaSubjectsNavigation?.GiaSubjectsNavigation?.Name)
                                  .Where(name => !string.IsNullOrEmpty(name)));

                // Обработка олимпиад
                case IEnumerable<StudentOlympiadParticipation> olympiads:
                    return string.Join(separator,
                        olympiads.Select(o => o.IdOlympiadsNavigation?.OlympiadsItemsNavigation?.Name)
                                 .Where(name => !string.IsNullOrEmpty(name)));

                // Общий случай для любых коллекций
                case IEnumerable collection when !(collection is string):
                    return string.Join(separator,
                        collection.Cast<object>()
                                  .Select(x => x?.ToString())
                                  .Where(s => !string.IsNullOrEmpty(s)));

                default:
                    return string.Empty;
            }
        }

        /// <summary>
        /// Обратное преобразование не поддерживается
        /// </summary>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}