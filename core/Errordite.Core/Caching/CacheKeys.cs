using Errordite.Core.Extensions;
using Errordite.Core.Domain.Organisation;

namespace Errordite.Core.Caching
{
    public static class CacheKeys
    {
        public static class Organisations
        {
            public static string Key(int? pageNumber = null, int? pageSize = null)
            {
                return "organisations{0}{1}".FormatWith(pageNumber.HasValue ? pageNumber.Value.ToString() : string.Empty, pageSize.HasValue ? pageSize.Value.ToString() : string.Empty);
            }

            public static string Key(string organisationId)
            {
                return Organisation.GetId(organisationId);
            }

            public static string Email(string email)
            {
                return "o-{0}".FormatWith(email);
            }

            public static string Statistics(string organisationId)
            {
                return "o-{0}-stats".FormatWith(Organisation.GetId(organisationId));
            }
        }

        public static class Groups
        {
            public static string PerOrganisationPrefix(string organisationId)
            {
                return "g-{0}".FormatWith(Organisation.GetId(organisationId));
            }

            public static string Key(string organisationId, string groupId)
            {
                return "{0}-{1}".FormatWith(PerOrganisationPrefix(organisationId), Group.GetId(groupId));
            }
        }

        public static class Applications
        {
            public static string PerOrganisationPrefix(string organisationId)
            {
                return "a-{0}".FormatWith(Organisation.GetId(organisationId));
            }

            public static string Key(string organisationId, string applicationId)
            {
                return "{0}-{1}".FormatWith(PerOrganisationPrefix(organisationId), Application.GetId(applicationId));
            }
        }

        public static class Users
        {
            public static string PerOrganisationPrefix(string organisationId)
            {
                return "u-{0}".FormatWith(Organisation.GetId(organisationId));
            }

            public static string Email(string organisationId, string email)
            {
				return "{0}-{1}".FormatWith(organisationId, email);
            }
        }
    }
}