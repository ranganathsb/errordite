﻿using System;
using System.Collections.Generic;
using System.Linq;
using Errordite.Core.Extensions;
using Errordite.Core.Interfaces;
using Errordite.Core.Domain.Error;
using Errordite.Core.Indexing;
using Errordite.Core.Organisations;
using Errordite.Core.Session;

namespace Errordite.Core.Issues.Queries
{
    public class GetIssueReportDataQuery : SessionAccessBase, IGetIssueReportDataQuery
    {
        public GetIssueReportDataResponse Invoke(GetIssueReportDataRequest request)
        {
            Trace("Starting...");

            var data = new Dictionary<string, object>();
            var hourlyCount = Session.Raven.Load<IssueHourlyCount>("IssueHourlyCount/{0}".FormatWith(request.IssueId.GetFriendlyId()));

            data.Add("ByHour", new
            {
                x = hourlyCount.HourlyCount.Select(h => h.Key.ToString("0")),
                y = hourlyCount.HourlyCount.Select(h => h.Value)
            });

            var startDate = request.StartDate.Date;
            var endDate = request.EndDate.Date;

            var dateResults = Query<IssueDailyCount, IssueDailyCounts>()
                .Where(i => i.IssueId == Issue.GetId(request.IssueId))
                .Where(i => i.Historical == false)
                .Where(i => i.Date >= startDate && i.Date <= endDate)
                .OrderBy(i => i.Date)
                .ToList();

            if (dateResults.Any())
            {
                var range = Enumerable.Range(0, (endDate - startDate).Days + 1).ToList();
                data.Add("ByDate", new
                {
                    x = range.Select(index => FindIssueCount(dateResults, startDate.AddDays(index)).Date.ConvertToUnixTimestamp()),
                    y = range.Select(index => FindIssueCount(dateResults, startDate.AddDays(index)).Count)
                });
            }
            else
            {
                var range = Enumerable.Range(0, (endDate - startDate).Days + 1).ToList();
                data.Add("ByDate", new
                {
                    x = range.Select(d => startDate.AddDays(d).ConvertToUnixTimestamp()),
                    y = range.Select(d => 0)
                });
            }

            return new GetIssueReportDataResponse
            {
                Data = data
            };
        }

        private IssueDailyCount FindIssueCount(IEnumerable<IssueDailyCount> results, DateTime date)
        {
            var result = results.FirstOrDefault(r => r.Date == date);

            if (result == null)
            {
                return new IssueDailyCount
                {
                    Count = 0,
                    Date = date
                };
            }

            return result;
        }
    }

    public interface IGetIssueReportDataQuery : IQuery<GetIssueReportDataRequest, GetIssueReportDataResponse>
    { }

    public class GetIssueReportDataResponse
    {
        public Dictionary<string, object> Data { get; set; }
    }

    public class GetIssueReportDataRequest : OrganisationRequestBase
    {
        public string IssueId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}
