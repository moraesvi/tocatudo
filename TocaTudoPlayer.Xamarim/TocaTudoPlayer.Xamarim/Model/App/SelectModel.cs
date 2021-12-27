using System;

namespace TocaTudoPlayer.Xamarim
{
    public class SelectModel : IEquatable<SelectModel>
    {
        public SelectModel(int id, string value)
        {
            Id = id;
            Value = value;
        }
        public int Id { get; }
        public string Value { get; set; }
        public bool Equals(SelectModel other)
        {
            if (other == null) return false;
            return Id == other.Id;
        }
    }
}
