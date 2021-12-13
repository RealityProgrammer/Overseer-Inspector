using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace RealityProgrammer.OverseerInspector.Editors.Miscs.Aurora {
    public sealed class AuroraScanner {
        private static readonly Dictionary<string, TokenType> _keywords;

        static AuroraScanner() {
            _keywords = new Dictionary<string, TokenType> {
                { "this", TokenType.This },
                { "null", TokenType.Null },
                { "true", TokenType.True },
                { "false", TokenType.False },
            };
        }

        private int start = 0;
        private int position = 0;

        private string sourceProgram;
        private List<LexerToken> _tokens;

        public AuroraScanner() {
            _tokens = new List<LexerToken>();
        }

        public List<LexerToken> Scan(string source) {
            sourceProgram = source;
            _tokens.Clear();

            start = 0;
            position = 0;

            while (!IsProgramEnd()) {
                start = position;
                ScanToken();
            }

            _tokens.Add(new LexerToken(TokenType.EOF, null, string.Empty));

            return _tokens;
        }

        private void ScanToken() {
            char c = Advance();

            switch (c) {
                case '(': AddToken(TokenType.LeftParenthesis); break;
                case ')': AddToken(TokenType.RightParenthesis); break;
                case '[': AddToken(TokenType.LeftSquareBracket); break;
                case ']': AddToken(TokenType.RightSquareBracket); break;
                case '.': AddToken(TokenType.Dot); break;
                case ',': AddToken(TokenType.Comma); break;

                case '+': AddToken(TokenType.Plus); break;
                case '-': AddToken(TokenType.Minus); break;
                case '*': AddToken(TokenType.Star); break;
                case '/': AddToken(TokenType.Slash); break;
                case '%': AddToken(TokenType.Percentage); break;

                case '!': AddToken(Match('=') ? TokenType.BangEqual : TokenType.Bang); break;
                case '=':
                    // Equals is a bit special, Aurora are restricted to be Expression Interpreter, so we will ignore Statements, including assigning
                    if (Match('=')) {
                        AddToken(TokenType.EqualEqual);
                    } else {
                        AddToken(TokenType.EqualEqual);

                        Debug.LogWarning("Illegal character '=' at position " + (position - 1) + ", assignments are not allowed. Fallback to equality comparision.");
                    }
                    break;
                case '<': AddToken(Match('=') ? TokenType.LessEqual : (Match('<') ? TokenType.BitwiseLeftShift : TokenType.Less)); break;
                case '>': AddToken(Match('=') ? TokenType.GreaterEqual : (Match('>') ? TokenType.BitwiseRightShift : TokenType.Greater)); break;

                case '&': AddToken(Match('&') ? TokenType.ConditionalAnd : TokenType.BitwiseAnd); break;
                case '|': AddToken(Match('|') ? TokenType.ConditionalOr : TokenType.BitwiseOr); break;
                case '~': AddToken(TokenType.BitwiseComplement); break;
                case '^': AddToken(TokenType.BitwiseOr); break;

                case ' ':
                case '\n':
                case '\r':
                case '\t':
                    break;

                case '"':
                    while (Peek() != '"' && !IsProgramEnd()) {
                        Advance();
                    }

                    if (IsProgramEnd()) {
                        Debug.LogWarning("Unterminated string discovered at position " + start + ". Handling...");

                        AddToken(TokenType.String, sourceProgram.Substring(start + 1, sourceProgram.Length - start));
                        break;
                    }

                    Advance();
                    AddToken(TokenType.String, sourceProgram.Substring(start + 1, position - start - 2));
                    break;

                default:
                    if (char.IsDigit(c)) {
                        HandleNumbers();
                    } else if (IsAlpha(c)) {
                        while (IsAlphaNumeric(Peek())) Advance();

                        if (!_keywords.TryGetValue(sourceProgram.Substring(start, position - start), out var type)) {
                            type = TokenType.Identifier;
                        }

                        AddToken(type);
                    } else {
                        throw new UnexpectedCharacterException(position - 1, c);
                    }

                    break;
            }
        }

        void HandleNumbers() {
            bool decimalSeperator = false;

            while (char.IsDigit(Peek())) Advance();

            if (Match('.')) {
                decimalSeperator = true;

                while (char.IsDigit(Peek())) Advance();
            }

            string digits = sourceProgram.Substring(start, position - start);

            switch (Peek()) {
                case 'f':
                case 'F':
                    AddNumberToken(float.Parse(digits), NumericType.Float);
                    Advance();
                    break;

                case 'd':
                case 'D':
                    AddNumberToken(double.Parse(digits), NumericType.Double);
                    Advance();
                    break;

                case 'm':
                case 'M':
                    AddNumberToken(decimal.Parse(digits), NumericType.Decimal);
                    Advance();
                    break;

                default:
                    if (decimalSeperator) {
                        AddNumberToken(double.Parse(digits), NumericType.Double);
                        Advance();
                    } else {
                        NumericType integerNumericType = NumericType.Int;

                        bool isChecking = true;
                        while (isChecking) {
                            char peek = Peek();
                            switch (peek) {
                                case 'u':
                                case 'U':
                                    if (!integerNumericType.IsUnsigned()) {
                                        integerNumericType |= NumericType.Unsigned;
                                    } else {
                                        Debug.LogWarning("Multiple unsigned integer literals discovered.");
                                    }

                                    Advance();
                                    break;

                                case 'l':
                                case 'L':
                                    if ((int)(integerNumericType & NumericType.Long) == 0) {
                                        integerNumericType = integerNumericType.IsUnsigned() ? (NumericType.Long | NumericType.Unsigned) : NumericType.Long;
                                    } else {
                                        Debug.LogWarning("Multiple long literals discovered.");
                                    }

                                    Advance();
                                    break;

                                default:
                                    isChecking = false;
                                    break;
                            }
                        }

                        NumericType numericType = (NumericType)((int)integerNumericType & 0x00FFFFFF);
                        bool unsigned = integerNumericType.IsUnsigned();

                        switch (numericType) {
                            case NumericType.Byte:
                                AddNumberToken(unsigned ? (object)byte.Parse(digits) : (object)sbyte.Parse(digits), integerNumericType);
                                break;

                            case NumericType.Short:
                                AddNumberToken(unsigned ? (object)ushort.Parse(digits) : (object)short.Parse(digits), integerNumericType);
                                break;

                            case NumericType.Int:
                                AddNumberToken(unsigned ? (object)uint.Parse(digits) : (object)int.Parse(digits), integerNumericType);
                                break;

                            case NumericType.Long:
                                AddNumberToken(unsigned ? (object)ulong.Parse(digits) : (object)long.Parse(digits), integerNumericType);
                                break;
                        }
                    }
                    break;
            }
        }

        LexerToken AddNumberToken(object literal, NumericType type) {
            var token = AddToken(TokenType.Number, literal);
            token.NumericType = type;

            return token;
        }

        private bool IsProgramEnd() {
            return position >= sourceProgram.Length;
        }

        private char Advance() {
            if (IsProgramEnd()) return '\0';

            return sourceProgram[position++];
        }

        private char Peek() {
            if (IsProgramEnd()) return '\0';

            return sourceProgram[position];
        }

        private char PeekNext() {
            if (position + 1 >= sourceProgram.Length) return '\0';

            return sourceProgram[position + 1];
        }

        private bool IsAlpha(char c) {
            return char.IsLetter(c) || c == '_';
        }

        private bool IsAlphaNumeric(char c) {
            return IsAlpha(c) || char.IsNumber(c);
        }

        private bool Match(char c) {
            if (IsProgramEnd()) return false;
            if (sourceProgram[position] != c) return false;

            position++;
            return true;
        }

        private LexerToken AddToken(TokenType type) {
            return AddToken(type, null);
        }

        private LexerToken AddToken(TokenType type, object literal) {
            LexerToken token = new LexerToken(type, literal, sourceProgram.Substring(start, position - start));
            _tokens.Add(token);

            return token;
        }

        public string DebugTokenOutput() {
            StringBuilder sb = new StringBuilder();
            
            for (int i = 0; i < _tokens.Count; i++) {
                var token = _tokens[i];

                sb.Append(i).Append(". ");

                if (token == null) {
                    sb.Append("<NULL TOKEN>");
                } else {
                    sb.Append(token);
                }

                sb.AppendLine();
            }

            return sb.ToString();
        }
    }
}