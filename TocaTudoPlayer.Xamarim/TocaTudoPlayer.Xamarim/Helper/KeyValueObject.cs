namespace TocaTudoPlayer.Xamarim.Helper
{
    public struct KeyValueObject<TKey, TValue>
    {
        public KeyValueObject(TKey key, TValue value) 
        {
            Key = key;
            Value = value;
        }
        public TKey Key { get; }
        public TValue Value { get; }
    }
}
