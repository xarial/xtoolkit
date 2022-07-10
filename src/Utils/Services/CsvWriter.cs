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
        public static CsvWriter ToFile(string filePath, char separator = ',')
            => new CsvWriter(File.CreateText(filePath), separator);

        private const char PROTECT_SYMBOL = '\"';
        private const char ROW_SEPARATION_SYMBOL = '\n';

        private readonly TextWriter m_Writer;

        private readonly char[] m_SpecSymbols;
        private readonly char m_Separator;

        private bool m_IsFirstLine;

        public CsvWriter(TextWriter writer, char separator = ',') 
        {
            if (writer == null) 
            {
                throw new ArgumentNullException(nameof(writer));
            }

            m_Separator = separator;

            m_SpecSymbols = new char[]
            {
                separator,
                PROTECT_SYMBOL,
                ROW_SEPARATION_SYMBOL
            };

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
                    m_Writer.Write(m_Separator);
                }

                m_Writer.Write(EscapeCellValue(cell));
                
                isFirstCell = false;
            }
        }

        private string EscapeCellValue(string val)
        {
            if (val != null)
            {
                if (m_SpecSymbols.Any(s => val.Contains(s)))
                {
                    return PROTECT_SYMBOL + val + PROTECT_SYMBOL;
                }
                else
                {
                    return val;
                }
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
