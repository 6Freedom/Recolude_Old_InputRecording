using System.Text;

namespace RecordAndPlay.IO
{
    /// <summary>
    /// Represents a single CSV.
    /// </summary>
    public class CSVPage
    {

        string name;

        string[,] entries;

        public CSVPage(string name, string[,] entries)
        {
            this.name = name;
            this.entries = entries;
        }

        public string GetName()
        {
            return this.name;
        }

        private string Encode(string entry)
        {
            if (entry == null)
            {
                return "\"\"";
            }
            var final = entry;
            if (final.Contains("\""))
            {
                final = final.Replace("\"", "\"\"");
            }
            return string.Format("\"{0}\"", final);
        }

        /// <summary>
        /// Combine like CSV tables into larger ones. Assumes each table has the same headers
        /// </summary>
        /// <param name="name">Name of the resulting CSV.</param>
        /// <param name="orderName">The column name that dictates which row comes from which CSV.</param>
        /// <param name="pages">The like pages to combine.</param>
        /// <returns>A resulting CSV table that holds all the contents of those passed in.</returns>
        public static CSVPage Combine(string name, string orderName, params CSVPage[] pages)
        {
            int rows = 1 - pages.Length; // remove all duplicate entries of header rows (1 per page but keep our own)
            int columns = pages[0].entries.GetLength(1) + 1;
            foreach (var p in pages)
            {
                rows += p.entries.GetLength(0);
            }

            string[,] newEntries = new string[rows, columns];
            newEntries[0, 0] = orderName;
            for (int j = 0; j < pages[0].entries.GetLength(1); j++)
            {
                newEntries[0, j + 1] = pages[0].entries[0, j];
            }

            int rowsAdded = 0;
            for (int pageIndex = 0; pageIndex < pages.Length; pageIndex++)
            {
                for (int localRowIndex = 1; localRowIndex < pages[pageIndex].entries.GetLength(0); localRowIndex++)
                {
                    int globalRowIndex = localRowIndex + rowsAdded;
                    newEntries[globalRowIndex, 0] = pageIndex.ToString();
                    for (int columnIndex = 0; columnIndex < pages[pageIndex].entries.GetLength(1); columnIndex++)
                    {
                        newEntries[globalRowIndex, columnIndex + 1] = pages[pageIndex].entries[localRowIndex, columnIndex];
                    }
                }
                rowsAdded += pages[pageIndex].entries.GetLength(0) - 1;
            }

            return new CSVPage(name, newEntries);
        }

        public override string ToString()
        {
            if (entries == null)
            {
                return "";
            }

            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < entries.GetLength(0); i++)
            {
                for (int j = 0; j < entries.GetLength(1); j++)
                {
                    builder.Append(Encode(entries[i, j]));
                    if (j < entries.GetLength(1) - 1)
                    {
                        builder.Append(",");
                    }
                }
                if (i < entries.GetLength(0) - 1)
                {
                    builder.Append("\n");
                }
            }
            return builder.ToString();
        }

    }

}