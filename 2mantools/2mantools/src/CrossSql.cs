using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

// NOTE: For Oracle the implementation is not completed.

namespace X2MANTools {

    public class CrossSql {

        string appBaseDirectory;

        Dictionary<string, Dictionary<string, string>> table = new Dictionary<string, Dictionary<string, string>>();

        string[] varMaxKeywords = { "NVARCHAR", "VARCHAR", "VARBINARY"};

        public CrossSql(string appBaseDirectory) {
            this.appBaseDirectory = appBaseDirectory;
            Load();
        }

        public void Transform(string sourcePath, string targetPath, string targetType) {
            targetType = targetType.ToLower();
            if (table.ContainsKey(targetType)) {
                File.WriteAllText(targetPath, File.ReadAllText(sourcePath));
                return;
            }
            var words = ParseToWords(File.ReadAllLines(sourcePath));
            switch (targetType) {
                case Term.mysql: 
                    words = ToMysql(words); 
                    break;
                case Term.sqlite: 
                    words = ToSqlite(words); 
                    break;
                case Term.oracle: 
                    words = ToOracle(words); 
                    break;
                default:
                    words = ToCustom(targetType, words);
                    break;
            }
            File.WriteAllText(targetPath, FormatToText(words));
        }

        string[] ToMysql(string[] inWords) {
            var outWords = new List<string>();
            foreach (var word in inWords) {
                if (word.StartsWith("\""))
                    outWords.Add(word.Replace('"', '`'));
                else if (StartsWithLetter(word) && table[Term.mysql].ContainsKey(word))
                    outWords.Add(table[Term.mysql][word]);
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
                else if (StartsWithLetter(word) && table[Term.sqlite].ContainsKey(word)) {
                    outWords.Add(table[Term.sqlite][word]);
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
                if (StartsWithLetter(word) && table[Term.oracle].ContainsKey(word))
                    outWords.Add(table[Term.oracle][word]);
                else
                    outWords.Add(word);
            }
            return outWords.ToArray();
        }

        string[] ToCustom(string type, string[] inWords) {
            var outWords = new List<string>();
            foreach (var word in inWords) {
                if (StartsWithLetter(word) && table[type].ContainsKey(word))
                    outWords.Add(table[type][word]);
                else
                    outWords.Add(word);
            }
            return outWords.ToArray();
        }

        void Load() {
            try {
                var group = "";
                foreach (var rawLine in File.ReadAllLines(Path.Combine(appBaseDirectory, "cross-sql.ini"))) {
                    var line = rawLine.Trim();
                    if (line.StartsWith(";") || line.StartsWith("#")) {
                        // comment
                    }
                    else if (line.StartsWith("[")) {
                        group = line.TrimStart('[').TrimEnd(']').Trim().ToLower();
                        table[group] = new Dictionary<string, string>();
                    }
                    else if (line.Contains("=")) {
                        var fields = line.Split('=');
                        table[group][fields[0].Trim()] = fields[1].Trim();
                    }
                }
            }
            catch {
            }
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
            var create = false;
            var level = 0;
            for (var i = 0; i < words.Length; i++) {
                switch (words[i]) {
                    case "(":
                        level++;
                        if (create && level == 1) {
                            text.Append(" ");
                            text.AppendLine(words[i]);
                            text.Append(" ");
                        }
                        else {
                            text.Append(words[i]);
                        }
                        break;
                    case ")":
                        level--;
                        text.Append(words[i]);
                        break;
                    case ",": 
                        if (create && level == 1) {
                            text.AppendLine(words[i]);
                        }
                        else {
                            text.Append(words[i]);
                        }
                        break;
                    case ";":
                        text.AppendLine(words[i]);
                        create = false;
                        break;
                    case "CREATE":
                        create = true;
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
