using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xarial.XToolkit.Services
{
    public class CsvReader : IDisposable
    {
        public static CsvReader FromFile(string filePath, char separator = ',')
            => new CsvReader(File.OpenText(filePath), separator);

        private readonly TextReader m_Reader;

        private readonly char m_Separator;

        public CsvReader(TextReader reader, char separator = ',')
        {
            if (reader == null) 
            {
                throw new ArgumentNullException(nameof(reader));
            }

            m_Separator = separator;

            m_Reader = reader;

            HasContent = m_Reader.Peek() != 1;
        }

        public bool HasContent { get; private set; }

        public IEnumerable<string> ReadLine()
        {
            if (!HasContent) 
            {
                throw new Exception("CSV file already read");
            }

            var curCell = new StringBuilder();
            var isPrevQuote = false;
            var isProtectedCell = false;
            char? bufferSymbol = null;

            while (HasContent)
            {
                var symb = m_Reader.Read();

                HasContent = symb != -1;

                if (!HasContent)
                {
                    break;
                }

                var symbChar = (char)symb;

                if (symbChar == m_Separator && (!isProtectedCell || isPrevQuote))
                {
                    yield return curCell.ToString();
                    curCell.Clear();
                    isPrevQuote = false;
                    isProtectedCell = false;
                }
                else if (symbChar == '\n' && !isProtectedCell)
                {
                    yield return curCell.ToString();
                    yield break;
                }
                else 
                {
                    if (bufferSymbol.HasValue) 
                    {
                        curCell.Append(bufferSymbol.Value);
                    }

                    bufferSymbol = null;

                    if (symbChar == '\"') 
                    {
                        if (curCell.Length == 0)
                        {
                            isProtectedCell = true;
                            continue;
                        }
                        else if (!isPrevQuote)
                        {
                            isPrevQuote = true;
                            continue;
                        }
                        else if (isProtectedCell) 
                        {
                            bufferSymbol = symbChar;
                            isPrevQuote = false;
                            continue;
                        }
                    }

                    if (symbChar == '\r')
                    {
                        bufferSymbol = symbChar;
                    }
                    else
                    {
                        curCell.Append(symbChar);
                    }

                    isPrevQuote = false;
                }
            }

            yield return curCell.ToString();
        }

        public void Dispose()
        {
            m_Reader.Dispose();
        }
    }
}
