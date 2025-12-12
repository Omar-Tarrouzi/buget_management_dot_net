using System.Reflection;
using System.Text;

namespace App_de_gestion_de_buget_version2.Services
{
    public class CsvService
    {
        public byte[] ExportToCsv<T>(IEnumerable<T> data)
        {
            var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => IsSimpleType(p.PropertyType))
                .ToList();

            var sb = new StringBuilder();

            // Header
            sb.AppendLine(string.Join(",", properties.Select(p => p.Name)));

            // Rows
            foreach (var item in data)
            {
                var values = properties.Select(p =>
                {
                    var val = p.GetValue(item, null);
                    return FormatCsvValue(val);
                });
                sb.AppendLine(string.Join(",", values));
            }

            return Encoding.UTF8.GetBytes(sb.ToString());
        }

        public List<T> ImportFromCsv<T>(Stream fileStream) where T : new()
        {
            var list = new List<T>();
            using (var reader = new StreamReader(fileStream))
            {
                var headerLine = reader.ReadLine();
                if (string.IsNullOrWhiteSpace(headerLine)) return list;

                // Detect delimiter
                char delimiter = ',';
                if (headerLine.Contains(";") && headerLine.Split(';').Length > headerLine.Split(',').Length)
                {
                    delimiter = ';';
                }

                // Parse headers using the robust parser
                var headers = ParseCsvLine(headerLine, delimiter).Select(h => h.Trim().Trim('"')).ToArray();
                
                var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
                var propertyMap = new Dictionary<int, PropertyInfo>();

                for (int i = 0; i < headers.Length; i++)
                {
                    // Clean BOM if present in first header
                    var header = headers[i];
                    if (i == 0 && header.Length > 0 && header[0] == '\uFEFF') header = header.Substring(1);

                    var prop = properties.FirstOrDefault(p => p.Name.Equals(header, StringComparison.OrdinalIgnoreCase));
                    if (prop != null && prop.CanWrite && IsSimpleType(prop.PropertyType))
                    {
                        propertyMap[i] = prop;
                    }
                }

                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    if (string.IsNullOrWhiteSpace(line)) continue;

                    var values = ParseCsvLine(line, delimiter);
                    var item = new T();

                    for (int i = 0; i < values.Count && i < headers.Length; i++)
                    {
                        if (propertyMap.TryGetValue(i, out var prop))
                        {
                            try
                            {
                                var safeValue = ConvertValue(values[i], prop.PropertyType);
                                if (safeValue != null)
                                {
                                    prop.SetValue(item, safeValue);
                                }
                            }
                            catch
                            {
                                // Conversion failed - skip this property or set default
                            }
                        }
                    }
                    list.Add(item);
                }
            }
            return list;
        }

        private bool IsSimpleType(Type type)
        {
            var underlyingType = Nullable.GetUnderlyingType(type) ?? type;
            return underlyingType.IsPrimitive || underlyingType.IsEnum || underlyingType == typeof(string) || underlyingType == typeof(decimal) || underlyingType == typeof(DateTime) || underlyingType == typeof(Guid);
        }

        private string FormatCsvValue(object? value)
        {
            if (value == null) return "";
            var str = value.ToString() ?? "";
            
            // Format dates strictly to avoid ambiguity
            if (value is DateTime dt) str = dt.ToString("yyyy-MM-dd HH:mm:ss");
            if (value is decimal dec) str = dec.ToString(System.Globalization.CultureInfo.InvariantCulture);

            if (str.Contains(",") || str.Contains(";") || str.Contains("\"") || str.Contains("\n"))
            {
                return $"\"{str.Replace("\"", "\"\"")}\"";
            }
            return str;
        }

        private List<string> ParseCsvLine(string line, char delimiter)
        {
            var result = new List<string>();
            var current = new StringBuilder();
            var inQuotes = false;

            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];
                if (c == '\"')
                {
                    if (inQuotes && i + 1 < line.Length && line[i + 1] == '\"')
                    {
                        current.Append('\"');
                        i++;
                    }
                    else
                    {
                        inQuotes = !inQuotes;
                    }
                }
                else if (c == delimiter && !inQuotes)
                {
                    result.Add(current.ToString());
                    current.Clear();
                }
                else
                {
                    current.Append(c);
                }
            }
            result.Add(current.ToString());
            return result;
        }

        private object? ConvertValue(string value, Type targetType)
        {
            if (string.IsNullOrWhiteSpace(value)) return null;
            var t = Nullable.GetUnderlyingType(targetType) ?? targetType;

            try 
            {
                if (t.IsEnum) return Enum.Parse(t, value);
                if (t == typeof(Guid)) return Guid.Parse(value);
                
                // Smart Decimal Parsing (Handle 12.50 vs 12,50)
                if (t == typeof(decimal) || t == typeof(double) || t == typeof(float))
                {
                    value = value.Replace(",", "."); // Normalize to dot
                    return Convert.ChangeType(value, t, System.Globalization.CultureInfo.InvariantCulture);
                }

                // Smart Date Parsing
                if (t == typeof(DateTime))
                {
                   if (DateTime.TryParse(value, out DateTime res)) return res;
                   return null; // Fail safe
                }

                return Convert.ChangeType(value, t, System.Globalization.CultureInfo.InvariantCulture);
            }
            catch
            {
                return null;
            }
        }
    }
}
