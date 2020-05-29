using System.Runtime.InteropServices;

namespace QuickJS.Native
{
    [StructLayout(LayoutKind.Sequential)]
    public struct JSAtom
    {
        private int _value;

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return _value.GetHashCode();
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return _value.ToString();
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            if (obj is JSAtom)
            {
                return ((JSAtom) obj)._value == _value;
            }

            return false;
        }
    }
}