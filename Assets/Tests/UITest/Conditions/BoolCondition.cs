using System;

namespace Tests.UITest.Conditions
{
    internal class BoolCondition : Condition<string>
    {
        private readonly Func<bool> _getter;

        public BoolCondition(Func<bool> getter)
        {
            _getter = getter;
        }

        public override bool Satisfied()
        {
            return _getter != null && _getter();
        }

        public override string ToString()
        {
            return "BoolCondition(" + _getter + ")";
        }
    }
}