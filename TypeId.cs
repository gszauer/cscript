
namespace CScript {
    class TypeId {
        public int Index { get; protected set; }
        public string DebugName { get; protected set; }

        public TypeId(int index, string debugName) {
            Index = index;
            DebugName = debugName;
        }

        public static bool operator ==(TypeId lhs, TypeId rhs) {
            bool leftNull = System.Object.ReferenceEquals(lhs, null);
            bool rightNull = System.Object.ReferenceEquals(rhs, null);
            if (leftNull && rightNull) {
                return true; // If both null, the same
            }
            if (leftNull || rightNull) {
                return false; // If one null, different
            }
            return rhs.Index == lhs.Index; // No nulls, valid
        }
        public static bool operator !=(TypeId lhs, TypeId rhs) {
            return !(lhs == rhs);
        }
    }
}