﻿using BrewLib.UserInterface.Skinning.Styles;
using OpenTK;
using OpenTK.Input;
using System;

namespace BrewLib.UserInterface
{
    public class Slider : ProgressBar
    {
        private bool hovered;
        private bool dragged;

        public float Step;

        private bool disabled;
        public bool Disabled
        {
            get { return disabled; }
            set
            {
                if (disabled == value) return;
                disabled = value;
                dragged = false;
                RefreshStyle();
            }
        }

        public event EventHandler OnValueCommited;

        public Slider(WidgetManager manager) : base(manager)
        {
            OnHovered += (sender, e) =>
            {
                hovered = e.Hovered;
                if (!disabled) RefreshStyle();
            };
            OnClickDown += (sender, e) =>
            {
                if (disabled) return false;
                if (e.Button != MouseButton.Left) return false;
                dragged = true;
                Value = GetValueForPosition(new Vector2(e.X, e.Y));
                DragStart();
                return true;
            };
            OnClickUp += (sender, e) =>
            {
                if (disabled || !dragged) return;
                if (e.Button != MouseButton.Left) return;
                dragged = false;
                RefreshStyle();
                DragEnd();
                OnValueCommited?.Invoke(this, e);
            };
            OnDrag += (sender, e) =>
            {
                if (disabled || !dragged) return;
                Value = GetValueForPosition(new Vector2(e.X, e.Y));
                DragUpdate();
            };
        }

        public float GetValueForPosition(Vector2 position)
        {
            var bounds = Bounds;
            var mouseX = Manager.Camera.FromScreen(position).X;
            var value = (MaxValue - MinValue) * (mouseX - bounds.Left) / bounds.Width;
            if (Step != 0) value = Math.Min((int)(value / Step) * Step, MaxValue);
            return value;
        }

        protected virtual void DragStart()
        {
        }

        protected virtual void DragUpdate()
        {
        }

        protected virtual void DragEnd()
        {
        }

        protected override WidgetStyle Style => Manager.Skin.GetStyle<ProgressBarStyle>(BuildStyleName(disabled ? "disabled" : (dragged || hovered) ? "hover" : null));
    }
}
