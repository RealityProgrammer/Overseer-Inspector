using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RealityProgrammer.OverseerInspector.Editors.Miscs.Aurora {
    public enum TokenType {
        LeftParenthesis, RightParenthesis, Dot, Comma, Greater, Less, Bang,
        Plus, Minus, Star, Slash, Question, Percentage, LeftSquareBracket, RightSquareBracket,

        BitwiseAnd, BitwiseOr, BitwiseXor, BitwiseLeftShift, BitwiseRightShift, BitwiseComplement,

        GreaterEqual, LessEqual, EqualEqual, BangEqual, ConditionalAnd, ConditionalOr,

        Identifier, Number, String,

        True, False, Null,

        This,

        EOF,
    }
}