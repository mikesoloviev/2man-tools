using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

// NOTE: Implementation not completed.

namespace X2MANTools {

    public class CrossSql {

        Dictionary<string, string> mysqlTable = new Dictionary<string, string> {
            { "IDENTITY", "AUTO_INCREMENT" }
        };

        Dictionary<string, string> sqliteTable = new Dictionary<string, string> {
            { "IDENTITY", "AUTOINCREMENT" },
            { "USE", "-- USE" }
        };

        Dictionary<string, string> oracleTable = new Dictionary<string, string> {
            { "IDENTITY", "GENERATED ALWAYS AS IDENTITY" },
            { "DATABASE", "SCHEMA" },
            { "USE", "SET SCHEMA" }
        };

        public void Transform(string sourcePath, string targetPath, string targetType) {
            var words = ParseToWords(File.ReadAllLines(sourcePath));
            switch (targetType.ToLower()) {
                case "mssql": words = ToMssql(words); break;
                case "mysql": words = ToMysql(words); break;
                case "sqlite": words = ToSqlite(words); break;
                case "oracle": words = ToOracle(words); break;
                default: break;
            }
            File.WriteAllText(targetPath, FormatToText(words));
        }

        string[] ToMssql(string[] inWords) {
            // no transform required as of now
            return inWords;
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
            foreach (var line in lines) {
                foreach (var token in line.Replace("(", " ( ").Replace(")", " ) ").Replace(",", " , ").Replace(";", " ; ").Split(' ')) {
                    if (token == "") { /* skip */ }
                    else if (token.StartsWith("\"")) { words.Add(token); }
                    else { words.Add(token.ToUpper()); }
                }
            }
            return words.ToArray();
        }

        string FormatToText(string[] words) {
            var text = new StringBuilder();
            for (var i = 0; i < words.Length; i++) {
                switch (words[i]) {
                    case "(":
                        if (i > 0 && (words[i - 1].EndsWith("\"") || words[i - 1].EndsWith("`"))) {
                            text.Append(" ");
                            text.AppendLine(words[i]);
                            text.Append(" ");
                        }
                        else {
                            text.Append(words[i]);
                        }
                        break;
                    case ")":
                        text.Append(words[i]);
                        break;
                    case ",":
                    case ";":
                        text.AppendLine(words[i]);
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
