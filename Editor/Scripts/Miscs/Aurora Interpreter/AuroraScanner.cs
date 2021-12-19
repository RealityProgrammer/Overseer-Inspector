using System.Text.RegularExpressions;
using System.Collections.Generic;
using System;
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

            if (string.IsNullOrEmpty(source)) {
                _tokens.Add(new LexerToken(TokenType.EOF, null, string.Empty));

                return _tokens;
            }

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
                    SkipUntilMatch('"');

                    if (IsProgramEnd()) {
                        Debug.LogWarning("Unterminated string discovered at position " + start + ". Handling...");

                        AddToken(TokenType.String, sourceProgram.Substring(start + 1, sourceProgram.Length - start));
                        break;
                    }

                    Advance();
                    AddToken(TokenType.String, Regex.Unescape(sourceProgram.Substring(start + 1, position - start - 2)));
                    break;

                case '\'':
                    var advance = Advance();

                    if (advance == '\\') {
                        switch (Peek()) {
                            case '0':
                                AddCharacterToken('\0');
                                Advance();
                                break;

                            case 'a':
                                AddCharacterToken('\a');
                                Advance();
                                break;

                            case 'b':
                                AddCharacterToken('\b');
                                Advance();
                                break;

                            case 'f':
                                AddCharacterToken('\f');
                                Advance();
                                break;

                            case 'n':
                                AddCharacterToken('\n');
                                Advance();
                                break;

                            case 'r':
                                AddCharacterToken('\r');
                                Advance();
                                break;

                            case 't':
                                AddCharacterToken('\t');
                                Advance();
                                break;

                            case 'v':
                                AddCharacterToken('\v');
                                Advance();
                                break;

                            case '\'':
                                AddCharacterToken('\'');
                                Advance();
                                break;

                            case '"':
                                AddCharacterToken('"');
                                Advance();
                                break;

                            case '\\':
                                AddCharacterToken('\\');
                                Advance();
                                break;

                            case 'u': {
                                Advance();

                                int pos = position;
                                SkipUntilMatch('\'');
                                Advance();

                                if (position - pos - 1 != 4) {
                                    Debug.LogError("Escape character 'u' need to be provided with exactly 4 hexadecimal digits.");
                                    AddCharacterToken('\0');
                                    return;
                                }

                                AddCharacterToken(Regex.Unescape("\\u" + sourceProgram.Substring(pos, position - pos - 1))[0]);
                                break;
                            }

                            // Regex.Unescape doesn't recognise '\U'
                            //case 'U': {
                            //    Advance();

                            //    int pos = position;
                            //    SkipUntilMatch('\'');
                            //    Advance();

                            //    if (position - pos - 1 != 8) {
                            //        Debug.LogError("Escape character 'U' need to be provided with exactly 8 hexadecimal digits.");
                            //        AddCharacterToken('\0');
                            //        return;
                            //    }

                            //    Debug.Log("\\U" + sourceProgram.Substring(pos, position - pos - 1));
                            //    AddCharacterToken(Regex.Unescape("\\U" + sourceProgram.Substring(pos, position - pos - 1))[0]);
                            //    break;
                            //}

                            default:
                                Debug.LogError("Unknown escape character at position " + position + ": " + Peek());
                                SkipUntilMatch('\'');

                                Advance();
                                break;
                        }

                        if (!Match('\'')) {
                            Debug.LogError("Expected an single quote to end character literal.");
                        }
                    } else {
                        if (advance == '\'') {
                            Debug.LogError("Empty character literal at position " + position);
                        } else {
                            //AddCharacterToken(advance);
                            if (!Match('\'')) {
                                Debug.LogError("Too many characters in character literal at position: " + start);
                                SkipUntilMatch('\'');
                                Advance();
                            }

                            AddToken(TokenType.Character, advance);
                        }
                    }
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
                        bool explicitLiteral = false;

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
                                    if (!explicitLiteral) {
                                        explicitLiteral = true;

                                        if (integerNumericType.MaskNumericType() != NumericType.Long) {
                                            integerNumericType = integerNumericType.IsUnsigned() ? (NumericType.Long | NumericType.Unsigned) : NumericType.Long;
                                        } else {
                                            Debug.LogWarning("Multiple long literals discovered.");
                                        }
                                    } else {
                                        Debug.LogWarning("Multiple long literals discovered");
                                    }

                                    Advance();
                                    break;

                                case 's':
                                case 'S':
                                    if (!explicitLiteral) {
                                        explicitLiteral = true;

                                        if (integerNumericType.MaskNumericType() != NumericType.Short) {
                                            integerNumericType = integerNumericType.IsUnsigned() ? (NumericType.Short | NumericType.Unsigned) : NumericType.Short;
                                        } else {
                                            Debug.LogWarning("Multiple short literals discovered.");
                                        }
                                    } else {
                                        Debug.LogWarning("Multiple short literals discovered");
                                    }

                                    Advance();
                                    break;

                                case 'b':
                                case 'B':
                                    if (!explicitLiteral) {
                                        explicitLiteral = true;

                                        if (integerNumericType.MaskNumericType() != NumericType.Byte) {
                                            integerNumericType = integerNumericType.IsUnsigned() ? (NumericType.Byte | NumericType.Unsigned) : NumericType.Byte;
                                        } else {
                                            Debug.LogWarning("Multiple byte literals discovered.");
                                        }
                                    } else {
                                        Debug.LogWarning("Multiple byte literals discovered");
                                    }

                                    Advance();
                                    break;

                                default:
                                    isChecking = false;
                                    break;
                            }
                        }

                        NumericType numericType = integerNumericType.MaskNumericType();
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

        void SkipUntilMatch(char c) {
            while (Peek() != c && !IsProgramEnd()) {
                Advance();
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

        private LexerToken AddCharacterToken(char character) {
            return AddToken(TokenType.Character, character);
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