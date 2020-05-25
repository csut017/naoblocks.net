using Raven.Client.Documents.Conventions;
using Raven.Client.Documents.Indexes.Spatial;
using Raven.Client.Documents.Linq;
using Raven.Client.Documents.Queries;
using Raven.Client.Documents.Queries.Explanation;
using Raven.Client.Documents.Queries.Facets;
using Raven.Client.Documents.Queries.Highlighting;
using Raven.Client.Documents.Queries.MoreLikeThis;
using Raven.Client.Documents.Queries.Spatial;
using Raven.Client.Documents.Queries.Suggestions;
using Raven.Client.Documents.Queries.Timings;
using Raven.Client.Documents.Session;
using Sparrow.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace RavenDB.Mocks
{
    public class FakeAsyncDocumentQuery<TItem>
        : IAsyncDocumentQuery<TItem>
    {
        private readonly DocumentOperations<TItem> _operations;
        private readonly IQueryable<TItem> _source;

        public FakeAsyncDocumentQuery(DocumentOperations<TItem> operations, IQueryable<TItem> source)
        {
            this._operations = operations;
            this._source = source;
        }

        public DocumentConventions Conventions => throw new NotImplementedException();
        public string IndexName => throw new NotImplementedException();

        public bool IsDistinct => throw new NotImplementedException();
        public IAsyncDocumentQuery<TItem> Not => throw new NotImplementedException();

        public IAsyncDocumentQuery<TItem> AddOrder(string fieldName, bool descending = false, OrderingType ordering = OrderingType.String)
        {
            throw new NotImplementedException();
        }

        public IAsyncDocumentQuery<TItem> AddOrder<TValue>(Expression<Func<TItem, TValue>> propertySelector, bool descending = false, OrderingType ordering = OrderingType.String)
        {
            throw new NotImplementedException();
        }

        public IAsyncDocumentQuery<TItem> AddParameter(string name, object value)
        {
            throw new NotImplementedException();
        }

        public void AfterQueryExecuted(Action<QueryResult> action)
        {
            throw new NotImplementedException();
        }

        public void AfterStreamExecuted(Action<BlittableJsonReaderObject> action)
        {
            throw new NotImplementedException();
        }

        public IAsyncAggregationDocumentQuery<TItem> AggregateBy(Action<IFacetBuilder<TItem>> builder)
        {
            throw new NotImplementedException();
        }

        public IAsyncAggregationDocumentQuery<TItem> AggregateBy(FacetBase facet)
        {
            throw new NotImplementedException();
        }

        public IAsyncAggregationDocumentQuery<TItem> AggregateBy(IEnumerable<Facet> facets)
        {
            throw new NotImplementedException();
        }

        public IAsyncAggregationDocumentQuery<TItem> AggregateUsing(string facetSetupDocumentId)
        {
            throw new NotImplementedException();
        }

        public IAsyncDocumentQuery<TItem> AndAlso()
        {
            throw new NotImplementedException();
        }

        public Task<bool> AnyAsync(CancellationToken token = default)
        {
            var value = this._operations.Any(this._source);
            return Task.FromResult(value);
        }

        public IAsyncDocumentQuery<TItem> BeforeQueryExecuted(Action<IndexQuery> beforeQueryExecuted)
        {
            throw new NotImplementedException();
        }

        public IAsyncDocumentQuery<TItem> Boost(decimal boost)
        {
            throw new NotImplementedException();
        }

        public IAsyncDocumentQuery<TItem> CloseSubclause()
        {
            throw new NotImplementedException();
        }

        public IAsyncDocumentQuery<TItem> ContainsAll(string fieldName, IEnumerable<object> values)
        {
            throw new NotImplementedException();
        }

        public IAsyncDocumentQuery<TItem> ContainsAll<TValue>(Expression<Func<TItem, TValue>> propertySelector, IEnumerable<TValue> values)
        {
            throw new NotImplementedException();
        }

        public IAsyncDocumentQuery<TItem> ContainsAll<TValue>(Expression<Func<TItem, IEnumerable<TValue>>> propertySelector, IEnumerable<TValue> values)
        {
            throw new NotImplementedException();
        }

        public IAsyncDocumentQuery<TItem> ContainsAny(string fieldName, IEnumerable<object> values)
        {
            throw new NotImplementedException();
        }

        public IAsyncDocumentQuery<TItem> ContainsAny<TValue>(Expression<Func<TItem, TValue>> propertySelector, IEnumerable<TValue> values)
        {
            throw new NotImplementedException();
        }

        public IAsyncDocumentQuery<TItem> ContainsAny<TValue>(Expression<Func<TItem, IEnumerable<TValue>>> propertySelector, IEnumerable<TValue> values)
        {
            throw new NotImplementedException();
        }

        public int Count(Func<TItem, bool> predicate)
        {
            throw new NotImplementedException();
        }

        public Task<int> CountAsync(CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        public Lazy<Task<int>> CountLazilyAsync(CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        public IAsyncDocumentQuery<TItem> Distinct()
        {
            throw new NotImplementedException();
        }

        public Task<TItem> FirstAsync(CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        public Task<TItem> FirstOrDefaultAsync(CancellationToken token = default)
        {
            var value = this._operations.FirstOrDefault(this._source);
            return Task.FromResult(value);
        }

        public IAsyncDocumentQuery<TItem> Fuzzy(decimal fuzzy)
        {
            throw new NotImplementedException();
        }

        public IndexQuery GetIndexQuery()
        {
            throw new NotImplementedException();
        }

        public Task<QueryResult> GetQueryResultAsync(CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        public IAsyncGroupByDocumentQuery<TItem> GroupBy(string fieldName, params string[] fieldNames)
        {
            throw new NotImplementedException();
        }

        public IAsyncGroupByDocumentQuery<TItem> GroupBy((string Name, GroupByMethod Method) field, params (string Name, GroupByMethod Method)[] fields)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<System.Linq.IGrouping<TKey, TItem>> GroupBy<TKey>(Func<TItem, TKey> keySelector)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<System.Linq.IGrouping<TKey, TElement>> GroupBy<TKey, TElement>(Func<TItem, TKey> keySelector, Func<TItem, TElement> elementSelector)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<System.Linq.IGrouping<TKey, TItem>> GroupBy<TKey>(Func<TItem, TKey> keySelector, IEqualityComparer<TKey> comparer)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<System.Linq.IGrouping<TKey, TElement>> GroupBy<TKey, TElement>(Func<TItem, TKey> keySelector, Func<TItem, TElement> elementSelector, IEqualityComparer<TKey> comparer)
        {
            throw new NotImplementedException();
        }

        public IAsyncDocumentQuery<TItem> Highlight(string fieldName, int fragmentLength, int fragmentCount, out Highlightings highlightings)
        {
            throw new NotImplementedException();
        }

        public IAsyncDocumentQuery<TItem> Highlight(string fieldName, int fragmentLength, int fragmentCount, HighlightingOptions options, out Highlightings highlightings)
        {
            throw new NotImplementedException();
        }

        public IAsyncDocumentQuery<TItem> Highlight(Expression<Func<TItem, object>> path, int fragmentLength, int fragmentCount, out Highlightings highlightings)
        {
            throw new NotImplementedException();
        }

        public IAsyncDocumentQuery<TItem> Highlight(Expression<Func<TItem, object>> path, int fragmentLength, int fragmentCount, HighlightingOptions options, out Highlightings highlightings)
        {
            throw new NotImplementedException();
        }

        public IAsyncDocumentQuery<TItem> Include(string path)
        {
            throw new NotImplementedException();
        }

        public IAsyncDocumentQuery<TItem> Include(Expression<Func<TItem, object>> path)
        {
            throw new NotImplementedException();
        }

        public IAsyncDocumentQuery<TItem> IncludeExplanations(out Explanations explanations)
        {
            throw new NotImplementedException();
        }

        public IAsyncDocumentQuery<TItem> IncludeExplanations(ExplanationOptions options, out Explanations explanations)
        {
            throw new NotImplementedException();
        }

        public IAsyncDocumentQuery<TItem> Intersect()
        {
            throw new NotImplementedException();
        }

        public void InvokeAfterQueryExecuted(QueryResult result)
        {
            throw new NotImplementedException();
        }

        public void InvokeAfterStreamExecuted(BlittableJsonReaderObject result)
        {
            throw new NotImplementedException();
        }

        public Lazy<Task<IEnumerable<TItem>>> LazilyAsync(Action<IEnumerable<TItem>> onEval = null)
        {
            throw new NotImplementedException();
        }

        public IAsyncDocumentQuery<TItem> MoreLikeThis(Action<IMoreLikeThisBuilderForAsyncDocumentQuery<TItem>> builder)
        {
            throw new NotImplementedException();
        }

        public IAsyncDocumentQuery<TItem> MoreLikeThis(MoreLikeThisBase moreLikeThis)
        {
            throw new NotImplementedException();
        }

        public void NegateNext()
        {
            throw new NotImplementedException();
        }

        public IAsyncDocumentQuery<TItem> NoCaching()
        {
            throw new NotImplementedException();
        }

        public IAsyncDocumentQuery<TItem> NoTracking()
        {
            throw new NotImplementedException();
        }

        public IAsyncDocumentQuery<TResult> OfType<TResult>()
        {
            throw new NotImplementedException();
        }

        public IAsyncDocumentQuery<TItem> OpenSubclause()
        {
            throw new NotImplementedException();
        }

        public IAsyncDocumentQuery<TItem> OrderBy(string field, string sorterName)
        {
            throw new NotImplementedException();
        }

        public IAsyncDocumentQuery<TItem> OrderBy(string field, OrderingType ordering = OrderingType.String)
        {
            throw new NotImplementedException();
        }

        public IAsyncDocumentQuery<TItem> OrderBy<TValue>(Expression<Func<TItem, TValue>> propertySelector)
        {
            throw new NotImplementedException();
        }

        public IAsyncDocumentQuery<TItem> OrderBy<TValue>(Expression<Func<TItem, TValue>> propertySelector, string sorterName)
        {
            throw new NotImplementedException();
        }

        public IAsyncDocumentQuery<TItem> OrderBy<TValue>(Expression<Func<TItem, TValue>> propertySelector, OrderingType ordering)
        {
            throw new NotImplementedException();
        }

        public IAsyncDocumentQuery<TItem> OrderBy<TValue>(params Expression<Func<TItem, TValue>>[] propertySelectors)
        {
            throw new NotImplementedException();
        }

        public IAsyncDocumentQuery<TItem> OrderByDescending(string field, string sorterName)
        {
            throw new NotImplementedException();
        }

        public IAsyncDocumentQuery<TItem> OrderByDescending(string field, OrderingType ordering = OrderingType.String)
        {
            throw new NotImplementedException();
        }

        public IAsyncDocumentQuery<TItem> OrderByDescending<TValue>(Expression<Func<TItem, TValue>> propertySelector)
        {
            throw new NotImplementedException();
        }

        public IAsyncDocumentQuery<TItem> OrderByDescending<TValue>(Expression<Func<TItem, TValue>> propertySelector, string sorterName)
        {
            throw new NotImplementedException();
        }

        public IAsyncDocumentQuery<TItem> OrderByDescending<TValue>(Expression<Func<TItem, TValue>> propertySelector, OrderingType ordering)
        {
            throw new NotImplementedException();
        }

        public IAsyncDocumentQuery<TItem> OrderByDescending<TValue>(params Expression<Func<TItem, TValue>>[] propertySelectors)
        {
            throw new NotImplementedException();
        }

        public IAsyncDocumentQuery<TItem> OrderByDistance(DynamicSpatialField field, double latitude, double longitude)
        {
            throw new NotImplementedException();
        }

        public IAsyncDocumentQuery<TItem> OrderByDistance(Func<DynamicSpatialFieldFactory<TItem>, DynamicSpatialField> field, double latitude, double longitude)
        {
            throw new NotImplementedException();
        }

        public IAsyncDocumentQuery<TItem> OrderByDistance(DynamicSpatialField field, string shapeWkt)
        {
            throw new NotImplementedException();
        }

        public IAsyncDocumentQuery<TItem> OrderByDistance(Func<DynamicSpatialFieldFactory<TItem>, DynamicSpatialField> field, string shapeWkt)
        {
            throw new NotImplementedException();
        }

        public IAsyncDocumentQuery<TItem> OrderByDistance(Expression<Func<TItem, object>> propertySelector, double latitude, double longitude)
        {
            throw new NotImplementedException();
        }

        public IAsyncDocumentQuery<TItem> OrderByDistance(string fieldName, double latitude, double longitude)
        {
            throw new NotImplementedException();
        }

        public IAsyncDocumentQuery<TItem> OrderByDistance(Expression<Func<TItem, object>> propertySelector, string shapeWkt)
        {
            throw new NotImplementedException();
        }

        public IAsyncDocumentQuery<TItem> OrderByDistance(string fieldName, string shapeWkt)
        {
            throw new NotImplementedException();
        }

        public IAsyncDocumentQuery<TItem> OrderByDistance(Expression<Func<TItem, object>> propertySelector, double latitude, double longitude, double roundFactor)
        {
            throw new NotImplementedException();
        }

        public IAsyncDocumentQuery<TItem> OrderByDistance(string fieldName, double latitude, double longitude, double roundFactor)
        {
            throw new NotImplementedException();
        }

        public IAsyncDocumentQuery<TItem> OrderByDistance(Expression<Func<TItem, object>> propertySelector, string shapeWkt, double roundFactor)
        {
            throw new NotImplementedException();
        }

        public IAsyncDocumentQuery<TItem> OrderByDistance(string fieldName, string shapeWkt, double roundFactor)
        {
            throw new NotImplementedException();
        }

        public IAsyncDocumentQuery<TItem> OrderByDistanceDescending(DynamicSpatialField field, double latitude, double longitude)
        {
            throw new NotImplementedException();
        }

        public IAsyncDocumentQuery<TItem> OrderByDistanceDescending(Func<DynamicSpatialFieldFactory<TItem>, DynamicSpatialField> field, double latitude, double longitude)
        {
            throw new NotImplementedException();
        }

        public IAsyncDocumentQuery<TItem> OrderByDistanceDescending(DynamicSpatialField field, string shapeWkt)
        {
            throw new NotImplementedException();
        }

        public IAsyncDocumentQuery<TItem> OrderByDistanceDescending(Func<DynamicSpatialFieldFactory<TItem>, DynamicSpatialField> field, string shapeWkt)
        {
            throw new NotImplementedException();
        }

        public IAsyncDocumentQuery<TItem> OrderByDistanceDescending(Expression<Func<TItem, object>> propertySelector, double latitude, double longitude)
        {
            throw new NotImplementedException();
        }

        public IAsyncDocumentQuery<TItem> OrderByDistanceDescending(string fieldName, double latitude, double longitude)
        {
            throw new NotImplementedException();
        }

        public IAsyncDocumentQuery<TItem> OrderByDistanceDescending(Expression<Func<TItem, object>> propertySelector, string shapeWkt)
        {
            throw new NotImplementedException();
        }

        public IAsyncDocumentQuery<TItem> OrderByDistanceDescending(string fieldName, string shapeWkt)
        {
            throw new NotImplementedException();
        }

        public IAsyncDocumentQuery<TItem> OrderByDistanceDescending(Expression<Func<TItem, object>> propertySelector, double latitude, double longitude, double roundFactor)
        {
            throw new NotImplementedException();
        }

        public IAsyncDocumentQuery<TItem> OrderByDistanceDescending(string fieldName, double latitude, double longitude, double roundFactor)
        {
            throw new NotImplementedException();
        }

        public IAsyncDocumentQuery<TItem> OrderByDistanceDescending(Expression<Func<TItem, object>> propertySelector, string shapeWkt, double roundFactor)
        {
            throw new NotImplementedException();
        }

        public IAsyncDocumentQuery<TItem> OrderByDistanceDescending(string fieldName, string shapeWkt, double roundFactor)
        {
            throw new NotImplementedException();
        }

        public IAsyncDocumentQuery<TItem> OrderByScore()
        {
            throw new NotImplementedException();
        }

        public IAsyncDocumentQuery<TItem> OrderByScoreDescending()
        {
            throw new NotImplementedException();
        }

        public IAsyncDocumentQuery<TItem> OrElse()
        {
            throw new NotImplementedException();
        }

        public IAsyncDocumentQuery<TItem> Proximity(int proximity)
        {
            throw new NotImplementedException();
        }

        public IAsyncDocumentQuery<TItem> RandomOrdering()
        {
            throw new NotImplementedException();
        }

        public IAsyncDocumentQuery<TItem> RandomOrdering(string seed)
        {
            throw new NotImplementedException();
        }

        public IAsyncDocumentQuery<TItem> RelatesToShape<TValue>(Expression<Func<TItem, TValue>> propertySelector, string shapeWkt, SpatialRelation relation, double distanceErrorPct = 0.025)
        {
            throw new NotImplementedException();
        }

        public IAsyncDocumentQuery<TItem> RelatesToShape<TValue>(Expression<Func<TItem, TValue>> propertySelector, string shapeWkt, SpatialRelation relation, SpatialUnits units, double distanceErrorPct = 0.025)
        {
            throw new NotImplementedException();
        }

        public IAsyncDocumentQuery<TItem> RelatesToShape(string fieldName, string shapeWkt, SpatialRelation relation, double distanceErrorPct = 0.025)
        {
            throw new NotImplementedException();
        }

        public IAsyncDocumentQuery<TItem> RelatesToShape(string fieldName, string shapeWkt, SpatialRelation relation, SpatialUnits units, double distanceErrorPct = 0.025)
        {
            throw new NotImplementedException();
        }

        public IAsyncDocumentQuery<TItem> Search(string fieldName, string searchTerms, SearchOperator @operator = SearchOperator.Or)
        {
            throw new NotImplementedException();
        }

        public IAsyncDocumentQuery<TItem> Search<TValue>(Expression<Func<TItem, TValue>> propertySelector, string searchTerms, SearchOperator @operator = SearchOperator.Or)
        {
            throw new NotImplementedException();
        }

        public IAsyncDocumentQuery<TProjection> SelectFields<TProjection>(params string[] fields)
        {
            throw new NotImplementedException();
        }

        public IAsyncDocumentQuery<TProjection> SelectFields<TProjection>(QueryData queryData)
        {
            throw new NotImplementedException();
        }

        public IAsyncDocumentQuery<TProjection> SelectFields<TProjection>()
        {
            throw new NotImplementedException();
        }

        public Task<TItem> SingleAsync(CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        public Task<TItem> SingleOrDefaultAsync(CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        public IAsyncDocumentQuery<TItem> Skip(int count)
        {
            throw new NotImplementedException();
        }

        public IAsyncDocumentQuery<TItem> Spatial(Expression<Func<TItem, object>> path, Func<SpatialCriteriaFactory, SpatialCriteria> clause)
        {
            throw new NotImplementedException();
        }

        public IAsyncDocumentQuery<TItem> Spatial(string fieldName, Func<SpatialCriteriaFactory, SpatialCriteria> clause)
        {
            throw new NotImplementedException();
        }

        public IAsyncDocumentQuery<TItem> Spatial(DynamicSpatialField field, Func<SpatialCriteriaFactory, SpatialCriteria> clause)
        {
            throw new NotImplementedException();
        }

        public IAsyncDocumentQuery<TItem> Spatial(Func<DynamicSpatialFieldFactory<TItem>, DynamicSpatialField> field, Func<SpatialCriteriaFactory, SpatialCriteria> clause)
        {
            throw new NotImplementedException();
        }

        public IAsyncDocumentQuery<TItem> Statistics(out QueryStatistics stats)
        {
            throw new NotImplementedException();
        }

        public IAsyncSuggestionDocumentQuery<TItem> SuggestUsing(SuggestionBase suggestion)
        {
            throw new NotImplementedException();
        }

        public IAsyncSuggestionDocumentQuery<TItem> SuggestUsing(Action<ISuggestionBuilder<TItem>> builder)
        {
            throw new NotImplementedException();
        }

        public IAsyncDocumentQuery<TItem> Take(int count)
        {
            throw new NotImplementedException();
        }

        public IAsyncDocumentQuery<TItem> Timings(out QueryTimings timings)
        {
            throw new NotImplementedException();
        }

        public Task<TItem[]> ToArrayAsync(CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        public Task<List<TItem>> ToListAsync(CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        public IRavenQueryable<TItem> ToQueryable()
        {
            throw new NotImplementedException();
        }

        public IAsyncDocumentQuery<TItem> UsingDefaultOperator(QueryOperator queryOperator)
        {
            throw new NotImplementedException();
        }

        public IAsyncDocumentQuery<TItem> WaitForNonStaleResults(TimeSpan? waitTimeout = null)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<TItem> Where(Func<TItem, bool> predicate)
        {
            throw new NotImplementedException();
        }

        public IAsyncDocumentQuery<TItem> WhereBetween(string fieldName, object start, object end, bool exact = false)
        {
            throw new NotImplementedException();
        }

        public IAsyncDocumentQuery<TItem> WhereBetween<TValue>(Expression<Func<TItem, TValue>> propertySelector, TValue start, TValue end, bool exact = false)
        {
            throw new NotImplementedException();
        }

        public IAsyncDocumentQuery<TItem> WhereEndsWith(string fieldName, object value)
        {
            throw new NotImplementedException();
        }

        public IAsyncDocumentQuery<TItem> WhereEndsWith(string fieldName, object value, bool exact)
        {
            throw new NotImplementedException();
        }

        public IAsyncDocumentQuery<TItem> WhereEndsWith<TValue>(Expression<Func<TItem, TValue>> propertySelector, TValue value)
        {
            throw new NotImplementedException();
        }

        public IAsyncDocumentQuery<TItem> WhereEndsWith<TValue>(Expression<Func<TItem, TValue>> propertySelector, TValue value, bool exact)
        {
            throw new NotImplementedException();
        }

        public IAsyncDocumentQuery<TItem> WhereEquals(string fieldName, object value, bool exact = false)
        {
            throw new NotImplementedException();
        }

        public IAsyncDocumentQuery<TItem> WhereEquals(string fieldName, MethodCall value, bool exact = false)
        {
            throw new NotImplementedException();
        }

        public IAsyncDocumentQuery<TItem> WhereEquals<TValue>(Expression<Func<TItem, TValue>> propertySelector, TValue value, bool exact = false)
        {
            throw new NotImplementedException();
        }

        public IAsyncDocumentQuery<TItem> WhereEquals<TValue>(Expression<Func<TItem, TValue>> propertySelector, MethodCall value, bool exact = false)
        {
            throw new NotImplementedException();
        }

        public IAsyncDocumentQuery<TItem> WhereEquals(WhereParams whereParams)
        {
            throw new NotImplementedException();
        }

        public IAsyncDocumentQuery<TItem> WhereExists<TValue>(Expression<Func<TItem, TValue>> propertySelector)
        {
            throw new NotImplementedException();
        }

        public IAsyncDocumentQuery<TItem> WhereExists(string fieldName)
        {
            throw new NotImplementedException();
        }

        public IAsyncDocumentQuery<TItem> WhereGreaterThan(string fieldName, object value, bool exact = false)
        {
            throw new NotImplementedException();
        }

        public IAsyncDocumentQuery<TItem> WhereGreaterThan<TValue>(Expression<Func<TItem, TValue>> propertySelector, TValue value, bool exact = false)
        {
            throw new NotImplementedException();
        }

        public IAsyncDocumentQuery<TItem> WhereGreaterThanOrEqual(string fieldName, object value, bool exact = false)
        {
            throw new NotImplementedException();
        }

        public IAsyncDocumentQuery<TItem> WhereGreaterThanOrEqual<TValue>(Expression<Func<TItem, TValue>> propertySelector, TValue value, bool exact = false)
        {
            throw new NotImplementedException();
        }

        public IAsyncDocumentQuery<TItem> WhereIn(string fieldName, IEnumerable<object> values, bool exact = false)
        {
            throw new NotImplementedException();
        }

        public IAsyncDocumentQuery<TItem> WhereIn<TValue>(Expression<Func<TItem, TValue>> propertySelector, IEnumerable<TValue> values, bool exact = false)
        {
            throw new NotImplementedException();
        }

        public IAsyncDocumentQuery<TItem> WhereLessThan(string fieldName, object value, bool exact = false)
        {
            throw new NotImplementedException();
        }

        public IAsyncDocumentQuery<TItem> WhereLessThan<TValue>(Expression<Func<TItem, TValue>> propertySelector, TValue value, bool exact = false)
        {
            throw new NotImplementedException();
        }

        public IAsyncDocumentQuery<TItem> WhereLessThanOrEqual(string fieldName, object value, bool exact = false)
        {
            throw new NotImplementedException();
        }

        public IAsyncDocumentQuery<TItem> WhereLessThanOrEqual<TValue>(Expression<Func<TItem, TValue>> propertySelector, TValue value, bool exact = false)
        {
            throw new NotImplementedException();
        }

        public IAsyncDocumentQuery<TItem> WhereLucene(string fieldName, string whereClause)
        {
            throw new NotImplementedException();
        }

        public IAsyncDocumentQuery<TItem> WhereLucene(string fieldName, string whereClause, bool exact)
        {
            throw new NotImplementedException();
        }

        public IAsyncDocumentQuery<TItem> WhereNotEquals(string fieldName, object value, bool exact = false)
        {
            throw new NotImplementedException();
        }

        public IAsyncDocumentQuery<TItem> WhereNotEquals(string fieldName, MethodCall value, bool exact = false)
        {
            throw new NotImplementedException();
        }

        public IAsyncDocumentQuery<TItem> WhereNotEquals<TValue>(Expression<Func<TItem, TValue>> propertySelector, TValue value, bool exact = false)
        {
            throw new NotImplementedException();
        }

        public IAsyncDocumentQuery<TItem> WhereNotEquals<TValue>(Expression<Func<TItem, TValue>> propertySelector, MethodCall value, bool exact = false)
        {
            throw new NotImplementedException();
        }

        public IAsyncDocumentQuery<TItem> WhereNotEquals(WhereParams whereParams)
        {
            throw new NotImplementedException();
        }

        public IAsyncDocumentQuery<TItem> WhereRegex<TValue>(Expression<Func<TItem, TValue>> propertySelector, string pattern)
        {
            throw new NotImplementedException();
        }

        public IAsyncDocumentQuery<TItem> WhereRegex(string fieldName, string pattern)
        {
            throw new NotImplementedException();
        }

        public IAsyncDocumentQuery<TItem> WhereStartsWith(string fieldName, object value)
        {
            throw new NotImplementedException();
        }

        public IAsyncDocumentQuery<TItem> WhereStartsWith(string fieldName, object value, bool exact)
        {
            throw new NotImplementedException();
        }

        public IAsyncDocumentQuery<TItem> WhereStartsWith<TValue>(Expression<Func<TItem, TValue>> propertySelector, TValue value)
        {
            throw new NotImplementedException();
        }

        public IAsyncDocumentQuery<TItem> WhereStartsWith<TValue>(Expression<Func<TItem, TValue>> propertySelector, TValue value, bool exact)
        {
            throw new NotImplementedException();
        }

        public IAsyncDocumentQuery<TItem> WithinRadiusOf<TValue>(Expression<Func<TItem, TValue>> propertySelector, double radius, double latitude, double longitude, SpatialUnits? radiusUnits = null, double distanceErrorPct = 0.025)
        {
            throw new NotImplementedException();
        }

        public IAsyncDocumentQuery<TItem> WithinRadiusOf(string fieldName, double radius, double latitude, double longitude, SpatialUnits? radiusUnits = null, double distanceErrorPct = 0.025)
        {
            throw new NotImplementedException();
        }
    }
}