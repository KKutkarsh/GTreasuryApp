namespace GTreasury.Api.Utilities.Helpers
{
    public static class BatchHelper
    {
        public static IEnumerable<List<T>> Chunk<T>(IEnumerable<T> source, int size)
        {
            var batch = new List<T>(size);

            foreach (var item in source)
            {
                batch.Add(item);
                if (batch.Count == size)
                {
                    yield return batch;
                    batch = new List<T>(size);
                }
            }

            if (batch.Count > 0)
                yield return batch;
        }
    }
}
