﻿using System;
using System.Collections.Generic;
using System.Text;

namespace NaoBlocks.Core.Generators
{
    static class ExcelHelpers
    {
        public static string ToExcelColumn(this int i)
        {
            string column = string.Empty;

            if (i / 26m > 1)
            {
                int letter = (int)i / 26;
                column = ((char)(65 + letter - 1)).ToString();
                i -= letter * 26;
            }

            column += ((char)(65 + i - 1)).ToString();

            return column;
        }
    }
}
