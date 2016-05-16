using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TinBot.Web.ViewModels
{

    public class CheckinVM
    {
        public string subscriptionId { get; set; }
        public int notificationId { get; set; }
        public string id { get; set; }
        public string eventType { get; set; }
        public string publisherId { get; set; }
        public Message message { get; set; }
        public Detailedmessage detailedMessage { get; set; }
        public Resource resource { get; set; }
        public DateTime createdDate { get; set; }
    }

    public class Message
    {
        public string text { get; set; }
        public string html { get; set; }
        public string markdown { get; set; }
    }

    public class Detailedmessage
    {
        public string text { get; set; }
        public string html { get; set; }
        public string markdown { get; set; }
    }

    public class Resource
    {
        public int changesetId { get; set; }
        public string url { get; set; }
        public Author author { get; set; }
        public Checkedinby checkedInBy { get; set; }
        public DateTime createdDate { get; set; }
        public string comment { get; set; }
    }

    public class Author
    {
        public string id { get; set; }
        public string displayName { get; set; }
        public string uniqueName { get; set; }
    }

    public class Checkedinby
    {
        public string id { get; set; }
        public string displayName { get; set; }
        public string uniqueName { get; set; }
    }
}
