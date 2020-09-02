using System;

namespace QuickJS
{
    public class ParameterException : Exception
    {
        public Type type { get; set; }
        public int index { get; set; }

        public ParameterException(string message, Type type, int index)
        : base(message)
        {
            this.type = type;
            this.index = index;
        }

        public ParameterException(Type type, int index)
        : base("parameter error")
        {
            this.type = type;
            this.index = index;
        }

        public override string ToString()
        {
            return string.Format("{0} [expect {1} at {2}]", Message, type, index);
        }
    }
}
