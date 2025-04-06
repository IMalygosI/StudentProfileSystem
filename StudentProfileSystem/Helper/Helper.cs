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
            if (string.IsNullOrEmpty(input)) return "";
            return new string(input.Where(char.IsDigit).ToArray());
        }
    }

}
