namespace Tests.UITest.Conditions
{
    public interface ICondition
    {
        bool Satisfied();
    }

    public abstract class Condition<T> : ICondition
    {
        protected readonly string ObjectName;
        protected readonly T Param;

        protected Condition()
        {
        }

        protected Condition(T param)
        {
            Param = param;
        }

        protected Condition(string objectName, T param)
        {
            Param = param;
            ObjectName = objectName;
        }

        public abstract bool Satisfied();

        public override string ToString()
        {
            return GetType() + " '" + Param + "'";
        }
    }
}