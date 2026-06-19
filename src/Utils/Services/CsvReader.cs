//*********************************************************************
//xToolkit
//Copyright(C) 2026 Xarial Pty Limited
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
    /// <summary>
    /// Service for reading CSV files
    /// </summary>
    /// <remarks>CSV is read as per RFC 4180</remarks>
    public class CsvReader : IDisposable
    {
        /// <summary>
        /// Creates reader from file
        /// </summary>
        /// <param name="filePath">Path to a file</param>
        /// <param name="delimiter">Delimeter</param>
        /// <returns>CSV reader</returns>
        public static CsvReader FromFile(string filePath, char delimiter = ',')
            => FromStream(new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite), delimiter);

        /// <summary>
        /// Creates reader from stream
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="delimiter">Delimeter</param>
        /// <returns>CSV reader</returns>
        public static CsvReader FromStream(Stream stream, char delimiter = ',')
            => new CsvReader(new StreamReader(stream), delimiter, true);

        /// <summary>
        /// Creates reader from CSV text
        /// </summary>
        /// <param name="text">CSV content</param>
        /// <param name="delimiter">Delimeter</param>
        /// <returns>CSV reader</returns>
        public static CsvReader FromText(string text, char delimiter = ',')
            => new CsvReader(new StringReader(text), delimiter, true);

        private readonly TextReader m_Reader;
        private readonly char m_Delimiter;
        private readonly bool m_OwnsReader;
        private bool m_IsDisposed;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="reader">Reader</param>
        /// <param name="delimiter">Delimiter</param>
        /// <exception cref="ArgumentNullException">Reader is null</exception>
        public CsvReader(TextReader reader, char delimiter = ',')
            : this(reader, delimiter, false) 
        {
        }

        private CsvReader(TextReader reader, char delimiter, bool ownsReader)
        {
            if (reader == null)
            {
                throw new ArgumentNullException(nameof(reader));
            }

            m_Reader = reader;
            m_Delimiter = delimiter;
            m_OwnsReader = ownsReader;
        }

        /// <summary>
        /// True if there is a content at current position
        /// </summary>
        public bool HasContent
        {
            get
            {
                ThrowIfDisposed();
                return m_Reader.Peek() != -1;
            }
        }

        /// <summary>
        /// Reads the line content
        /// </summary>
        /// <returns>Line cells</returns>
        /// <exception cref="Exception">File has no content</exception>
        public IEnumerable<string> ReadLine()
        {
            if (HasContent)
            {
                var curCell = new StringBuilder();
                bool isProtectedCell = false;
                bool cellStarted = false;

                while (true)
                {
                    var symb = m_Reader.Read();

                    if (symb == -1)
                    {
                        yield return curCell.ToString();
                        break;
                    }

                    var symbChar = (char)symb;

                    if (isProtectedCell)
                    {
                        if (symbChar == '"')
                        {
                            if (m_Reader.Peek() == '"')
                            {
                                curCell.Append((char)m_Reader.Read());
                            }
                            else
                            {
                                isProtectedCell = false;
                            }
                        }
                        else if (symbChar == '\r')
                        {
                            curCell.Append(symbChar);

                            if (m_Reader.Peek() == '\n')
                            {
                                curCell.Append((char)m_Reader.Read());
                            }
                        }
                        else
                        {
                            curCell.Append(symbChar);
                        }
                    }
                    else
                    {
                        if (symbChar == '"' && !cellStarted)
                        {
                            isProtectedCell = true;
                            cellStarted = true;
                        }
                        else if (symbChar == m_Delimiter)
                        {
                            yield return curCell.ToString();
                            curCell.Clear();
                            cellStarted = false;
                        }
                        else if (symbChar == '\r')
                        {
                            if (m_Reader.Peek() == '\n')
                            {
                                m_Reader.Read();
                            }
                            yield return curCell.ToString();
                            break;
                        }
                        else if (symbChar == '\n')
                        {
                            yield return curCell.ToString();
                            break;
                        }
                        else
                        {
                            curCell.Append(symbChar);
                            cellStarted = true;
                        }
                    }
                }
            }
            else 
            {
                throw new Exception("File has no content");
            }
        }

        private void ThrowIfDisposed()
        {
            if (m_IsDisposed)
            {
                throw new ObjectDisposedException(nameof(CsvReader));
            }
        }

        /// <summary>
        /// Dispose
        /// </summary>
        public void Dispose()
        {
            if (!m_IsDisposed)
            {
                if (m_OwnsReader)
                {
                    m_Reader.Dispose();
                }

                m_IsDisposed = true;
            }
        }
    }
}
