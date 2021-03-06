﻿
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using Errordite.Web.Models.Groups;

namespace Errordite.Web.Models.Applications
{
    public class AddApplicationViewModel : AddApplicationPostModel
    {
        public IEnumerable<SelectListItem> ErrorConfigurations { get; set; }
        public IEnumerable<SelectListItem> Users { get; set; }
        public IEnumerable<SelectListItem> NotificationFrequencies { get; set; }
		public bool NewOrganisation { get; set; }
		public bool CampfireEnabled { get; set; }
		public bool HipChatEnabled { get; set; }

        public AddApplicationViewModel()
        {
            ErrorConfigurations = new List<SelectListItem>();
            Users = new List<SelectListItem>();
        }
    }

    public class AddApplicationPostModel
    {
        public AddApplicationPostModel()
        {
            NotificationGroups = new List<GroupViewModel>();
        }

        [Required(ErrorMessageResourceType = typeof(Resources.Application), ErrorMessageResourceName = "Name_Required")]
        public string Name { get; set; }
        public string MatchRuleFactoryId { get; set; }
        public string UserId { get; set; }
        public string Version { get; set; }
        public string NotificationFrequency { get; set; }
        public bool Active { get; set; }
        public int? HipChatRoomId { get; set; }
		public int? CampfireRoomId { get; set; }
        public List<GroupViewModel> NotificationGroups { get; set; }
    }
}