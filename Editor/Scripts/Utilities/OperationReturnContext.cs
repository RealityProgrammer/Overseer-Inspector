using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RealityProgrammer.OverseerInspector.Editors.Utility {
    public class OperationReturnContext {
        public OperationReturnCode Code { get; private set; }
        public string Message => _messages[Code] + AdditionalMessage;
        public string AdditionalMessage { get; set; }

        public OperationReturnContext(OperationReturnCode code) {
            Code = code;
            AdditionalMessage = string.Empty;
        }

        public static implicit operator OperationReturnContext(OperationReturnCode code) {
            return new OperationReturnContext(code);
        }

        public static implicit operator OperationReturnContext(bool validate) {
            return new OperationReturnContext(validate ? OperationReturnCode.Success : OperationReturnCode.NotSuccess_Safe);
        }

        public bool IsSuccess() => Code == OperationReturnCode.Success;

        public override string ToString() {
            return "OperationReturnContext(" + Code + ")";
        }

        public static string GrabErrorMessage(OperationReturnCode code) {
            return _messages[code];
        }
        private static readonly Dictionary<OperationReturnCode, string> _messages = new Dictionary<OperationReturnCode, string>() {
            { OperationReturnCode.Success, "Operation success." },
            { OperationReturnCode.NotSuccess_Safe, "Operation is not success (Safe)." },
            { OperationReturnCode.UnexpectedReturnValue, "Operation return unexpected value." },
            { OperationReturnCode.Custom, string.Empty },
        };
    }
}