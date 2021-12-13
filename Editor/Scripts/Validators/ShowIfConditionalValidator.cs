using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using RealityProgrammer.OverseerInspector.Editors.Attributes;
using RealityProgrammer.OverseerInspector.Editors.Miscs.Aurora;
using RealityProgrammer.OverseerInspector.Runtime.Validation;

namespace RealityProgrammer.OverseerInspector.Editors.Validators {
    [ConditionalConnect(typeof(ShowIfAttribute))]
    public class ShowIfConditionalValidator : BaseConditionalValidator {
        private class AuroraCacheUnit {
            public AuroraScanner Scanner { get; set; }
            public AuroraLexer Lexer { get; set; }
            public AuroraInterpreter Interpreter { get; set; }

            public BaseExpression Expression { get; set; }

            public string ErrorMessage { get; set; }

            public object Interpret() {
                return Interpreter.InterpretExpression(Expression);
            }
        }

        private static readonly Dictionary<ShowIfAttribute, AuroraCacheUnit> _showIfAuroraCache = new Dictionary<ShowIfAttribute, AuroraCacheUnit>();

        public override bool Validate(ValidateContext context) {
            var sia = (ShowIfAttribute)context.Attribute;

            if (_showIfAuroraCache.TryGetValue(sia, out var cache)) {
                if (!string.IsNullOrEmpty(cache.ErrorMessage)) {
                    EditorGUILayout.HelpBox(cache.ErrorMessage, MessageType.Error, true);

                    return true;
                }

                bool interpret = (bool)cache.Interpret();

                return interpret;
            } else {
                cache = new AuroraCacheUnit();
                _showIfAuroraCache.Add(sia, cache);

                try {
                    cache.Scanner = new AuroraScanner();
                    var tokens = cache.Scanner.Scan(sia.Program);

                    cache.Lexer = new AuroraLexer(tokens);
                    cache.Lexer.BindTarget(context.ValidateTarget);
                    cache.Expression = cache.Lexer.BeginLexing();

                    cache.Interpreter = new AuroraInterpreter();
                    cache.Interpreter.BindInterpretingTarget(context.ValidateTarget);
                    var result = cache.Interpret();

                    if (result is bool boolean) {
                        return boolean;
                    } else {
                        cache.ErrorMessage = "Output result is not a boolean";

                        return true;
                    }
                } catch (Exception e) {
                    cache.ErrorMessage = e.GetType().Name + " were thrown: " + e.Message;
                }

                return true;
            }
        }
    }
}