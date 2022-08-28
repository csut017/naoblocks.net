using NaoBlocks.Common;
using NaoBlocks.Engine.Data;
using Raven.Client.Documents;
using Raven.Client.Documents.Indexes;
using Raven.Client.Documents.Session;
using Raven.TestDriver;
using System;
using System.Threading.Tasks;

namespace NaoBlocks.Engine.Tests
{
    public class DatabaseHelper : RavenTestDriver
    {
        protected readonly DateTime now = new(2021, 3, 4, 5, 16, 27);

        public IDocumentStore InitialiseDatabase(Action<IDocumentSession>? initialiseData = null)
        {
            var store = GetDocumentStore();
            if (initialiseData == null) return store;

            using var session = store.OpenSession();
            initialiseData(session);
            session.SaveChanges();
            WaitForIndexing(store);
            return store;
        }

        public IDocumentStore InitialiseDatabase(params object[] entities)
        {
            var store = GetDocumentStore();
            using var session = store.OpenSession();
            foreach (var entity in entities)
            {
                session.Store(entity);
            }
            session.SaveChanges();
            WaitForIndexing(store);
            return store;
        }

        protected static TGenerator InitialiseGenerator<TGenerator>(IAsyncDocumentSession session)
            where TGenerator : ReportGenerator, new()
        {
            var query = new TGenerator();
            query.InitialiseSession(WrapSession(session));
            return query;
        }

        protected static TQuery InitialiseQuery<TQuery>(IAsyncDocumentSession session)
            where TQuery : DataQuery, new()
        {
            var query = new TQuery();
            query.InitialiseSession(WrapSession(session));
            return query;
        }

        protected static IDatabaseSession WrapSession(IAsyncDocumentSession session)
        {
            return new MockingRavenDbWrapper(session);
        }

        protected (RobotLog, int) GenerateLog(Conversation conversation, Robot robot, int timeOffset, DateTime logTime, params string[] messages)
        {
            var log = new RobotLog
            {
                Conversation = conversation,
                RobotId = robot.Id,
                WhenAdded = logTime.AddMinutes(-1),
                WhenLastUpdated = logTime
            };

            var count = timeOffset;
            foreach (var message in messages)
            {
                var line = new RobotLogLine
                {
                    Description = message,
                    SourceMessageType = ClientMessageType.RobotDebugMessage,
                    WhenAdded = logTime.AddMinutes(timeOffset),
                };
                log.Lines.Add(line);
            }
            return (log, timeOffset);
        }

        protected (RobotLog, int) GenerateLog(Conversation conversation, Robot robot, int timeOffset, params string[] messages)
        {
            var log = new RobotLog
            {
                Conversation = conversation,
                RobotId = robot.Id,
                WhenAdded = now.AddMinutes(-1),
                WhenLastUpdated = now
            };

            var count = timeOffset;
            foreach (var message in messages)
            {
                var line = new RobotLogLine
                {
                    Description = message,
                    SourceMessageType = ClientMessageType.RobotDebugMessage,
                    WhenAdded = now.AddMinutes(timeOffset),
                };
                log.Lines.Add(line);
            }
            return (log, timeOffset);
        }

        protected async Task InitialiseIndicesAsync(IDocumentStore store, params IAbstractIndexCreationTask[] indices)
        {
            foreach (var index in indices)
            {
                await index.ExecuteAsync(store);
            }

            WaitForIndexing(store);
        }
    }
}