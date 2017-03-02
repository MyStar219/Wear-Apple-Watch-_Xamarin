using Newtonsoft.Json;
using System;

namespace Stylist.Wear
{
    public enum ServiceTypes
    {
        Appointment,
        WalkIn
    }

    public enum ServiceStatuses
    {
        READY,
        CHAIR,
        COMPLETE,
        CANCEL
    }

    public class ServiceItem
    {
        public ServiceItem()
        {
            AppId = "TEMP" + RandomGenerator.RandomString(10);
            ScheduleDate = DateTime.Now;
        }

        [JsonProperty(PropertyName = "appId")]
        public string AppId { get; set; }

		[JsonProperty(PropertyName = "isDirty")]
		public bool IsDirty { get; set; }

        [JsonProperty(PropertyName = "firstname")]
        public string FirstName { get; set; }

        [JsonProperty(PropertyName = "lastname")]
        public string LastName { get; set; }

        [JsonIgnore]
        public string FullName { get { return string.Format("{0} {1}", this.FirstName, this.LastName); } }

        [JsonProperty(PropertyName = "scheduleDate")]
        private string scheduleDate;

        [JsonIgnore]
        public DateTime ScheduleDate
        {
            get { return string.IsNullOrWhiteSpace(scheduleDate) ? default(DateTime) : DateTime.Parse(scheduleDate); }
            set { scheduleDate = (value == null ? default(DateTime).ToString() : value.ToString()); }
        }

        [JsonProperty(PropertyName = "startServiceDate")]
        private string startServiceDate;

        [JsonIgnore]
        public DateTime? StartServiceDate
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(startServiceDate))
                    return DateTime.Parse(startServiceDate);

                return null;
            }
            set { startServiceDate = value.ToString(); }
        }

        [JsonProperty(PropertyName = "endServiceDate")]
        private string endServiceDate;

        [JsonIgnore]
        public DateTime? EndServiceDate
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(endServiceDate))
                    return DateTime.Parse(endServiceDate);

                return null;
            }
            set { endServiceDate = value.ToString(); }
        }

        [JsonProperty(PropertyName = "status")]
        private string status;

        [JsonIgnore]
        public ServiceStatuses Status
        {
            get { return (ServiceStatuses)Enum.Parse(typeof(ServiceStatuses), status); }
            set { status = value.ToString(); }
        }

        [JsonProperty(PropertyName = "listType")]
        private string listType;

        [JsonIgnore]
        public string ListTypeChar { get { return ListType.ToString().Substring(0, 1).ToUpper(); } }

        [JsonIgnore]
        public ServiceTypes ListType
        {
            get { return (ServiceTypes)Enum.Parse(typeof(ServiceTypes), listType); }
            set { listType = value.ToString(); }
        }

    }
}