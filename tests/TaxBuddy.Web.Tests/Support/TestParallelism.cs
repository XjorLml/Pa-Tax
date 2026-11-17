// Each test class starts its own app host with its own throwaway schema. With F09 there are nine such classes, and running
// them all at once (one per core) used up the development database's connection slots (PostgreSQL 53300, SETUP
// troubleshooting). Four at a time keeps the run well inside the limit.
[assembly: CollectionBehavior(MaxParallelThreads = 4)]
