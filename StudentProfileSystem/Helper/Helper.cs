using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StudentProfileSystem.Context;

namespace StudentProfileSystem
{
    internal class Helper
    {
        public static readonly ImcContext DateBase = new ImcContext();

        /// <summary>
        /// Для исключения дублирования школ с одинаковыми номерами
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string ExtractDigits(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;

            // Удаляем символы "Включая №, пробелы, буквы и т.д." и оставляем только цифры 
            return new string(input.Where(c => char.IsDigit(c)).ToArray());
        }
    }

}
