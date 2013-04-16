﻿
using System.Collections.Generic;
using CodeTrip.Core.Extensions;
using Errordite.Core.Domain.Error;

namespace Errordite.Web.Models.Errors
{
    public class ExceptionViewModel
    {
        public ExceptionInfo Info { get; set; }

        public List<ExtraDataItemViewModel> ExtraData { get; set; }
        public string Url { get; set; }
        public string UserAgent { get; set; }
        public bool InnerException { get; set; }
        public string MachineName { get; set; }

        public ExceptionViewModel(ExceptionInfo info, string url, string userAgent, string machineName,
            bool innerException,
            List<ExtraDataItemViewModel> extraData)
        {
            Info = info;
            Url = url;
            UserAgent = userAgent;
            InnerException = innerException;
            MachineName = machineName;
            ExtraData = extraData;
        }

        
        public bool DisplayInfoTable()
		{
			if(InnerException)
			{
				return Info.MethodName.IsNotNullOrEmpty() || Info.Module.IsNotNullOrEmpty() || (Info.ExtraData != null && Info.ExtraData.Count > 0);
			}

			return Url.IsNotNullOrEmpty() || UserAgent.IsNotNullOrEmpty() || Info.MethodName.IsNotNullOrEmpty() || Info.Module.IsNotNullOrEmpty() || (Info.ExtraData != null && Info.ExtraData.Count > 0);
		}
    }

    public class ExtraDataItemViewModel
    {
        public string Key { get; set; }
        public string Value { get; set; }
        public bool CanMakeRule { get; set; }
    }
}