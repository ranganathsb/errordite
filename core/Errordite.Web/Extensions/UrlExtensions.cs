﻿
using System;
using System.Web.Mvc;
using Errordite.Core.Configuration;
using Errordite.Core.Domain;
using Errordite.Core.Domain.Error;
using Errordite.Core.Extensions;
using Errordite.Core.Identity;
using Errordite.Web.Models.Issues;
using System.Linq;

namespace Errordite.Web.Extensions
{
    public static class UrlExtensions
    {
        /// <summary>
        /// Request.Url will not work as we will get the port-mapped value when in live.
        /// Instead using Url.CurrentRequest() will ensure we get the correctly mapped one.
        /// </summary>
        public static string CurrentRequest(this UrlHelper helper, string query = null)
        {
            var uriBuilder = new UriBuilder(ErrorditeConfiguration.Current.SiteBaseUrl);

            var currentUri = helper.RequestContext.HttpContext.Request.Url;

            if (currentUri == null)
                return null;

            uriBuilder.Path = currentUri.AbsolutePath;

            if (query != null)
            {
                uriBuilder.Query = currentUri.Query.IsNullOrEmpty() ? query : currentUri.Query.MergeQueryStrings(query).Replace("?", "");
            }
            else
            {
                uriBuilder.Query = currentUri.Query.Replace("?", "");
            }

            return uriBuilder.Uri.ToString();
        }

        #region Dashboard

        public static string Dashboard(this UrlHelper helper)
        {
            return helper.Action("index", "dashboard", new { Area = string.Empty });
        }

        public static string Dashboard(this UrlHelper helper, string applicationId)
        {
            return helper.Action("index", "dashboard", new { Area = string.Empty, applicationId });
        }

        public static string ActivityLog(this UrlHelper helper, string applicationId = null)
        {
			return applicationId != null ? helper.Action("activity", "dashboard", new { applicationId, Area = string.Empty }) : helper.Action("activity", "dashboard", new { Area = string.Empty });
        }
        
        public static string Errors(this UrlHelper helper, string applicationId = null)
        {
            return applicationId != null ? helper.Action("index", "errors", new { applicationId, Area = string.Empty }) : helper.Action("index", "errors", new { Area = string.Empty });
        }

		public static string Test(this UrlHelper helper)
		{
			return helper.Action("index", "test", new { Area = string.Empty });
		}

		public static string ErrorSearch(this UrlHelper helper, string query)
		{
			return helper.Action("index", "errors", new { Area = string.Empty, query});
		}

		public static string Replay(this UrlHelper helper, string id)
		{
			return helper.Action("replay", "errors", new { id, Area = string.Empty });
		}

        public static string AllIssues(this UrlHelper helper)
        {
            string root = helper.Action("index", "issues", new { Area = string.Empty });
            string status = Enum.GetNames(typeof(IssueStatus)).Aggregate("?", (current, t) => current + ("Status={0}&".FormatWith(t)));
            return root + status;
        }

        public static string Issues(this UrlHelper helper, string applicationId = null)
        {
            return applicationId != null ? helper.Action("index", "issues", new { applicationId, Area = string.Empty }) : helper.Action("index", "issues", new { Area = string.Empty });
        }

		public static string IssueSearch(this UrlHelper helper, string name)
		{
			return helper.Action("index", "issues", new { Area = string.Empty, name });
		}

        public static string AddIssue(this UrlHelper helper)
        {
            return helper.Action("add", "issues", new { Area = string.Empty });
        }

        public static string Issue(this UrlHelper helper, string id, IssueTab? tab = null, string token = null)
        {
            if (token != null)
            {
                return helper.Action("public", "issue", tab != null ? (object)new { token, tab = tab.ToString().ToLowerInvariant(), Area = string.Empty } : new { id, Area = string.Empty });
            }

            id = IdHelper.GetFriendlyId(id);
            return helper.Action("index", "issue", tab != null ? (object)new { id, tab = tab.ToString().ToLowerInvariant(), Area = string.Empty } : new { id, Area = string.Empty });
        }

		public static string IssueNotFound(this UrlHelper helper, string id)
		{
			return helper.Action("notfound", "issue", new { id = id.GetFriendlyId(), Area = string.Empty });
		}

        public static string BaseIssueUrl(this UrlHelper helper)
        {
            return helper.Action("index", "issue", new {id = "{0}", Area = string.Empty});
        }

        public static string Issues(this UrlHelper helper, IssueStatus status, string applicationId = null)
        {
            return applicationId == null
                ? helper.Action("index", "issues", new {Status = status, Area = string.Empty})
                : helper.Action("index", "issues", new {Status = status, ApplicationId = applicationId, Area = string.Empty});
        }

        public static string MyIssues(this UrlHelper helper, string userId, string applicationId = null)
        {
            return applicationId == null
                ? helper.Action("index", "issues", new { AssignedTo = userId, Area = string.Empty })
                : helper.Action("index", "issues", new { AssignedTo = userId, Area = string.Empty, ApplicationId = applicationId });
        }

        public static string Rules(this UrlHelper helper, string id)
        {
            return helper.Action("adjustrules", "issue", new { id, Area = string.Empty });
        }

        #endregion

        #region Home

        public static string Home(this UrlHelper helper, AppContext context = null)
        {
            return context == null || context.AuthenticationStatus == AuthenticationStatus.Anonymous ? "/" : helper.Dashboard();
        }

        public static string Contact(this UrlHelper helper)
        {
            return "/contact";
        }

        #endregion

        #region Help 

        public static string DotNetClientGitHub(this UrlHelper helper)
        {
            return "https://github.com/errordite/dotnet-client";
        }

        public static string DjangoClientGitHub(this UrlHelper helper)
        {
            return "https://github.com/hugorodgerbrown/django-errordite";
        }

        public static string PythonClientGitHub(this UrlHelper helper)
        {
            return "https://github.com/hugorodgerbrown/python-errordite";
        }

        public static string RubyClientGitHub(this UrlHelper helper)
        {
            return "https://github.com/errordite/errordite-ruby";
        }

        public static string DotNetClientNuget(this UrlHelper helper)
        {
            return "http://nuget.org/packages?q=errordite";
        }

        public static string Clients(this UrlHelper helper)
        {
            return helper.Action("clients", "docs", new { Area = string.Empty });
        }

		public static string JsonClient(this UrlHelper helper)
		{
			return "{0}?tab=other".FormatWith(helper.Action("clients", "docs", new { Area = string.Empty }));
		}

        public static string Api(this UrlHelper helper)
        {
            return helper.Action("api", "docs", new { Area = string.Empty });
        }

        public static string About(this UrlHelper helper)
        {
            return helper.Action("about", "docs", new { Area = string.Empty });
        }

        public static string QuickStart(this UrlHelper helper)
        {
            return helper.Action("quickstart", "docs", new { Area = string.Empty });
        }

        public static string Pricing(this UrlHelper helper)
        {
            return helper.Action("pricing", "docs", new { Area = string.Empty });
        }

        public static string Privacy(this UrlHelper helper)
        {
            return helper.Action("privacy", "docs", new { Area = string.Empty });
        }

        public static string Terms(this UrlHelper helper)
        {
            return helper.Action("terms", "docs", new { Area = string.Empty });
        }

        #endregion

        #region Authentication

        public static string SignUp(this UrlHelper helper)
        {
            return helper.Action("signup", "authentication", new { Area = string.Empty });
        }

		public static string Demo(this UrlHelper helper)
		{
			return helper.Action("demo", "authentication", new { Area = string.Empty });
		}

        public static string SignIn(this UrlHelper helper)
        {
            return helper.Action("signin", "authentication", new { Area = string.Empty });
        }

        public static string SignOut(this UrlHelper helper)
        {
            return helper.Action("signout", "authentication", new { Area = string.Empty });
        }

        public static string ResetPassword(this UrlHelper helper)
        {
            return helper.Action("resetpassword", "authentication", new { Area = string.Empty });
        }

        #endregion

        #region Subscription

        public static string Complete(this UrlHelper helper, int subscriptionId, string reference)
        {
            return helper.Action("complete", "subscription", new { Area = string.Empty, subscriptionId, reference });
        }

        public static string SubscriptionSignUp(this UrlHelper helper)
        {
            return helper.Action("signup", "subscription", new { Area = string.Empty });
        }

		public static string ChangeSubscription(this UrlHelper helper, string planId)
        {
			return helper.Action("change", "subscription", new { Area = string.Empty, planId });
        }

        public static string Cancel(this UrlHelper helper)
        {
            return helper.Action("cancel", "subscription", new { Area = string.Empty });
        }

        public static string Subscription(this UrlHelper helper)
        {
            return helper.Action("index", "subscription", new { Area = string.Empty });
        }

        public static string BillingHistory(this UrlHelper helper)
        {
            return helper.Action("billinghistory", "subscription", new { Area = string.Empty });
        }

        #endregion

        #region Users

        public static string Users(this UrlHelper helper, string groupId = null)
        {
            return groupId != null ? helper.Action("index", "users", new { groupId, Area = string.Empty }) : helper.Action("index", "users", new { Area = string.Empty });
        }

        public static string AddUser(this UrlHelper helper)
        {
            return helper.Action("add", "users", new { Area = string.Empty });
        }

        public static string Profile(this UrlHelper helper)
        {
            return helper.Action("myprofile", "users", new { Area = string.Empty });
        }

        public static string EditUser(this UrlHelper helper, string userId)
        {
            return helper.Action("edit", "users", new { userId, Area = string.Empty });
        }

        #endregion
        
        #region Groups

        public static string Groups(this UrlHelper helper)
        {
            return helper.Action("index", "groups", new { Area = string.Empty });
        }

        public static string AddGroup(this UrlHelper helper)
        {
            return helper.Action("add", "groups", new { Area = string.Empty });
        }

        public static string EditGroup(this UrlHelper helper, string groupId)
        {
            return helper.Action("edit", "groups", new { groupId, Area = string.Empty });
        }

        #endregion

        #region Admin

		public static string Organisation(this UrlHelper helper)
        {
			return helper.Action("organisation", "account", new { Area = string.Empty });
        }

		public static string HipChat(this UrlHelper helper)
		{
			return helper.Action("hipchat", "account", new { Area = string.Empty });
		}

		public static string Campfire(this UrlHelper helper)
		{
			return helper.Action("campfire", "account", new { Area = string.Empty });
		}

		public static string ApiAccess(this UrlHelper helper)
		{
			return helper.Action("apiaccess", "account", new { Area = string.Empty });
		}

		public static string ReplayReplacements(this UrlHelper helper)
		{
			return helper.Action("replayreplacements", "account", new { Area = string.Empty });
		}

        #endregion

        #region Applications

        public static string Applications(this UrlHelper helper)
        {
            return helper.Action("index", "applications", new { Area = string.Empty });
        }

        public static string AddApplication(this UrlHelper helper, bool? newOrganisation = null)
        {
            return helper.Action("add", "applications", new { Area = string.Empty, newOrganisation });
        }

        public static string EditApplication(this UrlHelper helper, string applicationId)
        {
            return helper.Action("edit", "applications", new { applicationId, Area = string.Empty });
        }

        #endregion

        #region Administration

        public static string Cache(this UrlHelper helper, string cacheProfile)
        {
            return helper.Action("index", "cache", new { id = cacheProfile, Area = WebConstants.AreaNames.System });
        }

        public static string FlushAllCaches(this UrlHelper helper)
        {
            return helper.Action("flushallcaches", "cache", new { Area = WebConstants.AreaNames.System });
        }

        public static string SystemAdmin(this UrlHelper helper)
        {
            return helper.Action("index", "system", new { Area = WebConstants.AreaNames.System });
        }

        public static string Organisations(this UrlHelper helper)
        {
            return helper.Action("index", "organisations", new {Area = WebConstants.AreaNames.System});
        }

        public static string SyncIndexes(this UrlHelper helper)
        {
            return helper.Action("syncindexes", "system", new { Area = WebConstants.AreaNames.System });
        }

        public static string MessageFailures(this UrlHelper helper)
        {
            return helper.Action("index", "monitoring", new { Area = WebConstants.AreaNames.System });
        }

		public static string StopPolling(this UrlHelper helper)
		{
			return helper.Action("stoppolling", "system", new { Area = WebConstants.AreaNames.System });
		}

        public static string Impersonate(this UrlHelper helper, string userId = null, string organisationId = null)
        {
            return helper.Action("index", "impersonation", new { userId, organisationId, Area = WebConstants.AreaNames.System });
        }

        public static string OrganisationUsers(this UrlHelper helper, string organisationId)
        {
            return helper.Action("users", "organisations", new { organisationId, Area = WebConstants.AreaNames.System});
        }

        public static string OrganisationApplications(this UrlHelper helper, string organisationId)
        {
            return helper.Action("applications", "organisations", new { organisationId, Area = WebConstants.AreaNames.System });
        }

        #endregion
    }
}