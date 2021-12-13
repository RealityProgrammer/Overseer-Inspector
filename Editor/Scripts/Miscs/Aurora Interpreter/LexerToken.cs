using System.Text;

namespace RealityProgrammer.OverseerInspector.Editors.Miscs.Aurora {
    public class LexerToken {
        public TokenType Type { get; set; }
        public object Literal { get; set; }
        public string Lexeme { get; set; }

        public NumericType NumericType { get; set; }

        public LexerToken(TokenType type, object literal, string lexeme) {
            Type = type;
            Literal = literal;
            Lexeme = lexeme;
        }

        public LexerToken(TokenType type, object literal, string lexeme, NumericType numericType) {
            Type = type;
            Literal = literal;
            Lexeme = lexeme;
            NumericType = numericType;
        }

        public override string ToString() {
            StringBuilder sb = new StringBuilder(GetType().Name).Append("(").Append(Type);

            if (Literal != null) {
                sb.Append(", Literal: ").Append(Literal);
            }

            if (!string.IsNullOrEmpty(Lexeme)) {
                sb.Append(", Lexeme: ").Append(Lexeme);
            }

            if (NumericType != NumericType.NAN) {
                sb.Append(", NumericType: [");

                if ((int)(NumericType & NumericType.Unsigned) == 0) {
                    sb.Append(NumericType).Append("]");
                } else {
                    sb.Append(NumericType ^ NumericType.Unsigned).Append(", Unsigned]");
                }
            }

            sb.Append(")");

            return sb.ToString();
        }
    }
}