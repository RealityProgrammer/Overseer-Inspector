using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RealityProgrammer.OverseerInspector.Editors.Miscs.Aurora {
    public sealed class AuroraLexer {
        private List<LexerToken> _input;

        private int current;

        public object BindingTarget { get; private set; }

        public void FeedTokens(List<LexerToken> inputs) {
            _input = inputs;
        }

        public void BindTarget(object obj) {
            BindingTarget = obj;
        }

        public BaseExpression BeginLexing() {
            current = 0;

            return Expression();
        }

        private BaseExpression Expression() {
            return ConditionalOr();
        }

        private BaseExpression ConditionalOr() {
            var expr = ConditionalAnd();

            while (Match(TokenType.ConditionalOr)) {
                expr = new BinaryExpression(expr, Previous(), ConditionalAnd());
            }

            return expr;
        }

        private BaseExpression ConditionalAnd() {
            var expr = BitwiseOr();

            while (Match(TokenType.ConditionalAnd)) {
                expr = new BinaryExpression(expr, Previous(), BitwiseOr());
            }

            return expr;
        }

        private BaseExpression BitwiseOr() {
            var expr = BitwiseXor();

            while (Match(TokenType.BitwiseOr)) {
                expr = new BinaryExpression(expr, Previous(), BitwiseXor());
            }

            return expr;
        }

        private BaseExpression BitwiseXor() {
            var expr = BitwiseAnd();

            while (Match(TokenType.BitwiseXor)) {
                expr = new BinaryExpression(expr, Previous(), BitwiseAnd());
            }

            return expr;
        }

        private BaseExpression BitwiseAnd() {
            var expr = Equality();

            while (Match(TokenType.BitwiseAnd)) {
                expr = new BinaryExpression(expr, Previous(), Equality());
            }

            return expr;
        }

        private BaseExpression Equality() {
            var expression = Comparision();

            while (Match(TokenType.EqualEqual, TokenType.BangEqual)) {
                expression = new BinaryExpression(expression, Previous(), Comparision());
            }

            return expression;
        }

        private BaseExpression Comparision() {
            var expression = Shift();

            while (Match(TokenType.Greater, TokenType.GreaterEqual, TokenType.Less, TokenType.LessEqual)) {
                expression = new BinaryExpression(expression, Previous(), Shift());
            }

            return expression;
        }

        private BaseExpression Shift() {
            var expression = Term();

            while (Match(TokenType.BitwiseLeftShift, TokenType.BitwiseRightShift)) {
                expression = new BinaryExpression(expression, Previous(), Term());
            }

            return expression;
        }

        private BaseExpression Term() {
            var expression = Factor();

            while (Match(TokenType.Minus, TokenType.Plus)) {
                expression = new BinaryExpression(expression, Previous(), Factor());
            }

            return expression;
        }

        private BaseExpression Factor() {
            var expression = Unary();

            while (Match(TokenType.Star, TokenType.Slash, TokenType.Percentage)) {
                expression = new BinaryExpression(expression, Previous(), Unary());
            }

            return expression;
        }

        private BaseExpression Unary() {
            if (Match(TokenType.Bang, TokenType.Minus, TokenType.BitwiseComplement)) {
                return new UnaryExpression(Previous(), Unary());
            }

            return Call();
        }

        private BaseExpression Call() {
            var expr = Primary();

            while (true) {
                var previous = Previous();

                if (Match(TokenType.LeftParenthesis)) {
                    expr = HandleCallExpression(expr, previous);
                } else if (Match(TokenType.Dot)) {
                    if (!Check(TokenType.Identifier)) {
                        Debug.LogError("Expected identifier after member access '.'");
                        return expr;
                    }

                    expr = new MemberAccessExpression(expr, Advance());
                } else if (Match(TokenType.LeftSquareBracket)) {
                    expr = HandleIndexerExpression(expr);

                    break;
                } else {
                    break;
                }
            }

            return expr;
        }

        private BaseExpression Primary() {
            if (Match(TokenType.True)) return new LiteralExpression(true, Previous());
            if (Match(TokenType.False)) return new LiteralExpression(false, Previous());
            if (Match(TokenType.Number)) return new LiteralExpression(Previous().Literal, Previous());
            if (Match(TokenType.String)) return new LiteralExpression(Previous().Literal, Previous());
            if (Match(TokenType.Character)) return new LiteralExpression(Previous().Literal, Previous());
            if (Match(TokenType.Null)) return new LiteralExpression(null, Previous());
            if (Match(TokenType.This)) return new LiteralExpression(BindingTarget, Previous());

            if (Match(TokenType.Identifier)) {
                // Somehow this thing works, but I'm not complaining kek
                return new MemberAccessExpression(new LiteralExpression(BindingTarget, Previous()), Previous());
            }

            if (Match(TokenType.LeftParenthesis)) {
                var expression = Expression();

                if (!Check(TokenType.RightParenthesis)) {
                    Debug.LogWarning("Expected a close parenthesis ')'.");
                    return new GroupingExpression(expression);
                }

                Advance();
                return new GroupingExpression(expression);
            }

            throw new ExpectAnExpressionException("Expect an expression at the end of tokens.");
        }

        private bool Match(params TokenType[] types) {
            foreach (TokenType type in types) {
                if (Check(type)) {
                    Advance();
                    return true;
                }
            }

            return false;
        }

        private bool Check(TokenType type) {
            if (IsAtEnd()) return false;
            return Peek().Type == type;
        }

        private LexerToken Advance() {
            if (!IsAtEnd()) current++;
            return Previous();
        }

        private bool IsAtEnd() {
            return Peek().Type == TokenType.EOF;
        }

        private LexerToken Peek() {
            return _input[current];
        }

        private LexerToken Previous() {
            return current == 0 ? null : _input[current - 1];
        }

        private BaseExpression HandleCallExpression(BaseExpression expr, LexerToken name) {
            List<BaseExpression> parameters = new List<BaseExpression>();

            if (!Check(TokenType.RightParenthesis)) {
                do {
                    parameters.Add(Expression());
                } while (Match(TokenType.Comma));
            }

            if (!Check(TokenType.RightParenthesis)) {
                Debug.LogWarning("Expected a close parenthesis ')'");
                return new MethodCallExpression(expr, parameters);
            }

            Advance();
            return new MethodCallExpression(expr, parameters);
        }

        private BaseExpression HandleIndexerExpression(BaseExpression expr) {
            List<BaseExpression> parameters = new List<BaseExpression>();

            if (!Check(TokenType.RightSquareBracket)) {
                do {
                    parameters.Add(Expression());
                } while (Match(TokenType.Comma));
            }

            if (!Check(TokenType.RightSquareBracket)) {
                Debug.LogWarning("Expected a close square bracket ']'");
                return new IndexerExpression(expr, parameters);
            }

            Advance();
            return new IndexerExpression(expr, parameters);
        }
    }
}