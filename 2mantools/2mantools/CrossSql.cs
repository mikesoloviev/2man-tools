using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

// NOTE: For Oracle and SQLite the implementation is not completed.

namespace X2MANTools {

    public class CrossSql {

        Dictionary<string, string> mysqlTable = new Dictionary<string, string> {
            { "IDENTITY", "AUTO_INCREMENT" },
            { "BIT", "BOOL" },
            { "REAL", "FLOAT" },
            { "FLOAT", "DOUBLE" },
            { "VARCHAR(MAX)", "LONGTEXT" },
            { "NVARCHAR(MAX)", "LONGTEXT" },
            { "VARBINARY(MAX)", "LONGBLOB" }
        };

        Dictionary<string, string> sqliteTable = new Dictionary<string, string> {
            { "IDENTITY", "AUTOINCREMENT" },
            { "INT", "INTEGER" },
            { "BIGINT", "INTEGER" },
            { "BIT", "BOOLEAN" },
            { "REAL", "FLOAT" },
            { "FLOAT", "DOUBLE" },
            { "VARCHAR(MAX)", "CLOB" },
            { "NVARCHAR(MAX)", "CLOB" },
            { "VARBINARY(MAX)", "BLOB" }
        };

        Dictionary<string, string> oracleTable = new Dictionary<string, string> {
            { "IDENTITY", "GENERATED ALWAYS AS IDENTITY" },
            { "DATABASE", "SCHEMA" },
            { "USE", "SET SCHEMA" },
            { "DATETIME", "TIMESTAMP(3)" },
            { "BIT", "NUMBER(1)" },
            { "INT", "NUMBER(10)" },
            { "BIGINT", "NUMBER(19)" },
            { "REAL", "BINARY_FLOAT" },
            { "FLOAT", "BINARY_DOUBLE" },
            { "VARCHAR(MAX)", "CLOB" },
            { "NVARCHAR(MAX)", "NCLOB" },
            { "VARBINARY(MAX)", "BLOB" }
        };

        string[] varMaxKeywords = { "NVARCHAR", "VARCHAR", "VARBINARY"};

        public void Transform(string sourcePath, string targetPath, string targetType) {
            targetType = targetType.ToLower();
            if (targetType == "mssql") {
                File.WriteAllText(targetPath, File.ReadAllText(sourcePath));
                return;
            }
            var words = ParseToWords(File.ReadAllLines(sourcePath));
            switch (targetType) {
                case "mysql": 
                    words = ToMysql(words); 
                    break;
                case "sqlite": 
                    words = ToSqlite(words); 
                    break;
                case "oracle": 
                    words = ToOracle(words); 
                    break;
                default: 
                    break;
            }
            File.WriteAllText(targetPath, FormatToText(words));
        }

        string[] ToMysql(string[] inWords) {
            var outWords = new List<string>();
            foreach (var word in inWords) {
                if (word.StartsWith("\""))
                    outWords.Add(word.Replace('"', '`'));
                else if (StartsWithLetter(word) && mysqlTable.ContainsKey(word))
                    outWords.Add(mysqlTable[word]);
                else
                    outWords.Add(word);
            }
            return outWords.ToArray();
        }

        string[] ToSqlite(string[] inWords) {
            var outWords = new List<string>();
            for (var i = 0; i < inWords.Length; i++) {
                var word = inWords[i];
                if (word == "CREATE") {
                    if (i < inWords.Length - 1 && inWords[i + 1] == "DATABASE")
                        outWords.Add("-- " + word);
                    else
                        outWords.Add(word);
                }
                else if (word == "USE") {
                    outWords.Add("-- " + word);
                }
                else if (StartsWithLetter(word) && sqliteTable.ContainsKey(word)) {
                    outWords.Add(sqliteTable[word]);
                }
                else {
                    outWords.Add(word);
                }
            }
            return outWords.ToArray();
        }

        string[] ToOracle(string[] inWords) {
            var outWords = new List<string>();
            foreach (var word in inWords) {
                if (StartsWithLetter(word) && oracleTable.ContainsKey(word))
                    outWords.Add(oracleTable[word]);
                else
                    outWords.Add(word);
            }
            return outWords.ToArray();
        }

        string[] ParseToWords(string[] lines) {
            var words = new List<string>();
            var quoted = false;
            var qoute = new List<string>();
            foreach (var line in lines) {
                foreach (var token in line.Replace("(", " ( ").Replace(")", " ) ").Replace(",", " , ").Replace(";", " ; ").Split(' ')) {
                    if (token == "") {
                        /* skip */
                    }
                    else if (token.StartsWith("\"")) {
                        words.Add(token);
                    }
                    else if (token.StartsWith("'")) {
                        if (token.EndsWith("'")) {
                            words.Add(token);
                        }
                        else {
                            quoted = true;
                            qoute = new List<string>();
                            qoute.Add(token);
                        }
                    }
                    else if (token.EndsWith("'")) {
                        quoted = false;
                        qoute.Add(token);
                        words.Add(string.Join(" ", qoute));
                    }
                    else if (quoted) {
                        qoute.Add(token);
                    }
                    else {
                        words.Add(token.ToUpper());
                    }
                }
            }
            return CoalesceVarMax(words);
        }

        string[] CoalesceVarMax(List<string> inWords) {
            try {
                var outWords = new List<string>();
                var i = 0;
                while (i < inWords.Count()) {
                    if (varMaxKeywords.Contains(inWords[i]) && inWords[i + 2] == "MAX" && inWords[i + 1] == "(" && inWords[i + 3] == ")") {
                        outWords.Add($"{inWords[i]}(MAX)");
                        i += 4;                        
                    }
                    else {
                        outWords.Add(inWords[i]);
                        i++;
                    }
                }
                return outWords.ToArray();
            }
            catch {
                return inWords.ToArray();
            }
        }

        string FormatToText(string[] words) {
            var text = new StringBuilder();
            var creating = false;
            var head = false;
            for (var i = 0; i < words.Length; i++) {
                switch (words[i]) {
                    case "(":
                        if (creating && head) {
                            text.Append(" ");
                            text.AppendLine(words[i]);
                            text.Append(" ");
                            head = false;
                        }
                        else {
                            text.Append(words[i]);
                        }
                        break;
                    case ")":
                        text.Append(words[i]);
                        break;
                    case ",": 
                        if (creating) {
                            text.AppendLine(words[i]);
                        }
                        else {
                            text.Append(words[i]);
                        }
                        break;
                    case ";":
                        text.AppendLine(words[i]);
                        creating = false;
                        break;
                    case "CREATE":
                        creating = true;
                        head = true;
                        text.Append(" ");
                        text.Append(words[i]);
                        break;
                    default:
                        if (i == 0 || words[i - 1] != "(") text.Append(" ");
                        text.Append(words[i]);
                        break;
                }
            }
            return text.ToString();
        }

        bool StartsWithLetter(string s) {
            return char.IsLetter(s[0]);
        }

    }
}
