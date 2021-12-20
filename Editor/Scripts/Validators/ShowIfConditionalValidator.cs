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
        internal class Cache {
            public string ErrorMessage { get; set; }
            public BaseExpression InterpretExpression { get; set; }
        }

        private static readonly Dictionary<ShowIfAttribute, Cache> _showIfAuroraCache = new Dictionary<ShowIfAttribute, Cache>();

        private static readonly AuroraScanner s_Scanner;
        private static readonly AuroraLexer s_Lexer;
        private static readonly AuroraInterpreter s_Interpreter;

        static ShowIfConditionalValidator() {
            s_Scanner = new AuroraScanner();
            s_Lexer = new AuroraLexer();
            s_Interpreter = new AuroraInterpreter();
        }

        public override bool Validate(ValidateContext context) {
            var sia = (ShowIfAttribute)context.Attribute;

            if (_showIfAuroraCache.TryGetValue(sia, out var cache)) {
                if (!string.IsNullOrEmpty(cache.ErrorMessage)) {
                    EditorGUILayout.HelpBox(cache.ErrorMessage, MessageType.Error, true);

                    return true;
                }

                s_Interpreter.BindInterpretingTarget(context.ValidateTarget);

                bool interpret = (bool)s_Interpreter.InterpretExpression(cache.InterpretExpression);

                return interpret;
            } else {
                cache = new Cache();
                _showIfAuroraCache.Add(sia, cache);

                try {
                    var tokens = s_Scanner.Scan(sia.Program);

                    s_Lexer.BindTarget(context.ValidateTarget);
                    s_Lexer.FeedTokens(tokens);

                    cache.InterpretExpression = s_Lexer.BeginLexing();

                    s_Interpreter.BindInterpretingTarget(context.ValidateTarget);
                    var result = s_Interpreter.InterpretExpression(cache.InterpretExpression);

                    if (result is bool boolean) {
                        return boolean;
                    } else {
                        cache.ErrorMessage = "Output result is not a boolean";

                        return true;
                    }
                } catch (Exception e) {
                    cache.ErrorMessage = e.GetType().Name + " were thrown: " + e.Message;
                    Debug.LogError("Exception were thrown while trying to interpret ShowIfConditionalValidator: " + cache.ErrorMessage);
                }

                return true;
            }
        }
    }
}