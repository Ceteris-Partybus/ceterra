using System;
using UnityEngine;

namespace Assets.Code.Scripts {
    public class GlobalDisplay {
        private int initialValue;
        private int minValue;
        private int maxValue;

        private int currentValue;

        public int InitialValue {
            get { return initialValue; }
            set {
                if (value < minValue || value > maxValue) {
                    throw new Exception("Value must be between " + minValue + " and " + maxValue);
                }
                else {
                    initialValue = value;
                }
            }
        }

        public int MinValue {
            get; set;
        }

        public int MaxValue {
            get; set;
        }

        public int CurrentValue {
            get { return currentValue; }
            set { currentValue = value; }
        }

        public GlobalDisplay(int initialValue, int minValue, int maxValue) {
            MinValue = minValue;
            MaxValue = maxValue;
            CurrentValue = initialValue;
            InitialValue = initialValue;
        }

        public void AddCurrentVaule(int add) {
            if (currentValue + add > maxValue) {
                currentValue = maxValue;
            }
            else {
                currentValue += add;
            }
        }

        public void SubtractCurrentValue(int subtract) {
            if (currentValue - subtract < minValue) {
                throw new Exception("Subtraction exceeds minimum value " + minValue);
            }
            else {
                currentValue -= subtract;
            }
        }
    }
}