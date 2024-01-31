//*********************************************************************
//xToolkit
//Copyright(C) 2023 Xarial Pty Limited
//Product URL: https://xtoolkit.xarial.com
//License: https://xtoolkit.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xarial.XToolkit.Services
{
    public class CsvWriter : IDisposable
    {
        public static CsvWriter ToFile(string filePath, char delimeter = ',')
            => new CsvWriter(File.CreateText(filePath), delimeter);

        private const char PROTECT_SYMBOL = '\"';
        private const char ROW_SEPARATION_SYMBOL = '\n';

        private readonly TextWriter m_Writer;

        private readonly char m_Delimeter;

        private bool m_IsFirstLine;

        public CsvWriter(TextWriter writer, char delimeter = ',') 
        {
            if (writer == null) 
            {
                throw new ArgumentNullException(nameof(writer));
            }

            m_Delimeter = delimeter;

            m_Writer = writer;

            m_IsFirstLine = true;
        }

        public void WriteLine(IEnumerable<string> line)
        {
            if (line == null)
            {
                throw new ArgumentNullException(nameof(line));
            }

            if (!m_IsFirstLine)
            {
                m_Writer.Write(Environment.NewLine);
            }
            else 
            {
                m_IsFirstLine = false;
            }

            var isFirstCell = true;

            foreach (var cell in line) 
            {
                if (!isFirstCell) 
                {
                    m_Writer.Write(m_Delimeter);
                }

                m_Writer.Write(EscapeCellValue(cell));
                
                isFirstCell = false;
            }
        }

        private string EscapeCellValue(string val)
        {
            if (val != null)
            {
                var escapedVal = new StringBuilder();

                var needEscape = false;

                foreach (var symb in val)
                {
                    if (symb == PROTECT_SYMBOL)
                    {
                        needEscape = true;
                        escapedVal.Append(PROTECT_SYMBOL);
                    }
                    else if (!needEscape) 
                    {   
                        if (symb == m_Delimeter || symb == ROW_SEPARATION_SYMBOL) 
                        {
                            needEscape = true;
                        }
                    }

                    escapedVal.Append(symb);
                }

                if (needEscape) 
                {
                    escapedVal.Insert(0, PROTECT_SYMBOL);
                    escapedVal.Append(PROTECT_SYMBOL);
                }

                return escapedVal.ToString();
            }
            else
            {
                return "";
            }
        }

        public void Dispose()
        {
            m_Writer.Dispose();
        }
    }
}
