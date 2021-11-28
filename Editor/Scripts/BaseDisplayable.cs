using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

namespace RealityProgrammer.OverseerInspector.Editors {
    public abstract class BaseDisplayable {
        protected List<BaseDisplayable> _childs = new List<BaseDisplayable>();
        public ReadOnlyCollection<BaseDisplayable> Childs => new ReadOnlyCollection<BaseDisplayable>(_childs);

        public void AddChild(BaseDisplayable child) {
            if (child.HasParent)
                return;
            if (ReferenceEquals(child, this))
                return;

            _childs.Add(child);
            child._parent = this;
        }

        public bool RemoveChild(BaseDisplayable child) {
            if (!child.HasParent)
                return false;

            bool removal = _childs.Remove(child);
            if (removal) {
                child._parent = null;
                return true;
            }

            return false;
        }

        public void RemoveChildAt(int index) {
            var c = _childs[index];
            _childs.RemoveAt(index);

            c._parent = null;
        }

        protected void DrawAllChildsLayout() {
            foreach (var child in _childs) {
                child.DrawLayout();
            }
        }

        private BaseDisplayable _parent = null;
        public bool HasParent => _parent != null;

        public abstract void DrawLayout();
    }
}