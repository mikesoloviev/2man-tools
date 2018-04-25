using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

// NOTE: In progress, most of the code -- just stubs.

namespace X2MANTools {

    public class CrossSql {

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
            return outWords.ToArray();
        }

        string[] ToSqlite(string[] inWords) {
            var outWords = new List<string>();
            return outWords.ToArray();
        }

        string[] ToOracle(string[] inWords) {
            var outWords = new List<string>();
            return outWords.ToArray();
        }

        string[] ParseToWords(string[] lines) {
            var words = new List<string>();
            foreach (var line in lines) {
                foreach (var token in line.Replace("(", " ( ").Replace(")", " ) ").Replace(",", " , ").Replace(";", " ; ").Split(' ')) {
                    if (token == "") { /* skip */ };
                    else if (token.StartsWith("\"")) { words.Add(token); }
                    else { words.Add(token.ToUpper()); }
                }
            }
            return words.ToArray();
        }

        string FormatToText(string[] words) {
            var text = new StringBuilder();

            
            retrun text.ToString();
        }

    }
}
