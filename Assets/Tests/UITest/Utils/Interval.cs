namespace Tests.UITest.Utils
{
    public struct Interval
    {
        public readonly float Origin;
        public readonly float End;

        public Interval(float origin, float end)
        {
            Origin = origin;
            End = end;
        }

        public override string ToString()
        {
            return $"{nameof(Origin)}: {Origin}, {nameof(End)}: {End}";
        }
    }
}