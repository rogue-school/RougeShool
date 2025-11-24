using System;

namespace Game.Domain.Stage.ValueObjects
{
    /// <summary>
    /// 스테이지 ID를 나타내는 값 객체입니다.
    /// </summary>
    public readonly struct StageId : IEquatable<StageId>
    {
        /// <summary>
        /// ID 문자열입니다.
        /// </summary>
        public string Value { get; }

        /// <summary>
        /// 스테이지 ID를 생성합니다.
        /// </summary>
        /// <param name="value">ID 문자열 (null/공백 불가)</param>
        public StageId(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException("스테이지 ID는 null이거나 공백일 수 없습니다.", nameof(value));
            }

            Value = value;
        }

        public override string ToString() => Value;

        public bool Equals(StageId other) => string.Equals(Value, other.Value, StringComparison.Ordinal);

        public override bool Equals(object obj) => obj is StageId other && Equals(other);

        public override int GetHashCode() => Value != null ? Value.GetHashCode(StringComparison.Ordinal) : 0;

        public static bool operator ==(StageId left, StageId right) => left.Equals(right);

        public static bool operator !=(StageId left, StageId right) => !left.Equals(right);
    }
}


