using System;
using UnityEngine;

namespace Assets.Code.Scripts {
    public class GlobalDisplay {
        private int minValue;
        private int maxValue;

        private int currentValue;

        public int CurrentValue {
            get { return this.currentValue; }
            set {
                if (value < this.MinValue || value > this.MaxValue) {
                    throw new Exception("Value must be between " + this.MinValue + " and " + this.MaxValue);
                }
                else {
                    this.currentValue = value;
                }
            }
        }

        public int MinValue {
            get; set;
        }

        public int MaxValue {
            get; set;
        }

        public GlobalDisplay(int minValue, int maxValue, int currentValue) {
            this.MinValue = minValue;
            this.MaxValue = maxValue;
            this.currentValue = currentValue;
        }

        public void AddCurrentValue(int add) {
            if (this.CurrentValue + add > this.MaxValue) {
                this.CurrentValue = this.MaxValue;
            }
            else {
                this.CurrentValue += add;
            }
        }

        public void SubtractCurrentValue(int subtract) {
            if (this.CurrentValue - subtract < this.MinValue) {
                throw new Exception("Subtraction exceeds minimum value " + this.MinValue);
            }
            else {
                this.CurrentValue -= subtract;
            }
        }
    }
}