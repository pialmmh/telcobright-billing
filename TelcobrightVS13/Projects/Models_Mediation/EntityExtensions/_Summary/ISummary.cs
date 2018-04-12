namespace MediationModel
{
    public interface ISummary
    {

    }
    public interface ISummary<TEntity, TKey> : ISummary 
    {
        long id { get; set; }
        TKey GetTupleKey();
        void Merge(TEntity newSummary);
        void Multiply(int value);

        //summaries are merged in Cache, cache.Insert without value copy will have the same reference 
        //for source summary entity & the mergable version in cache
        //when a next summary instance to update the merged cache, the first source object will also get changed.
        //to prevent that, use GetValueCopy();
        TEntity CloneWithFakeId();
    }
}
