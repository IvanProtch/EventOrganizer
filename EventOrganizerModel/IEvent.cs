using System;
using System.Drawing;

namespace EventOrganizerModel
{
    public class Event : ICloneable
    {
        public int EventTypeId { get; set; }
        public int UserId { get; set; }

        public int Id { get; }
        public string Caption { get; set; }
        public string Description { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public EventType EventType { get; set; }
        public TimeSpan RepeatsTimeSpan { get; set; } = new TimeSpan();

        public bool IsRunning
        {
            get
            {
                return DateTime.Now <= End && DateTime.Now >= Start;
            }
        }

        public object Clone()
        {
            return new Event()
            {
                Caption = this.Caption,
                Description = this.Description,
                Start = this.Start + this.RepeatsTimeSpan,
                End = this.End + this.RepeatsTimeSpan,
                EventType = this.EventType,
                RepeatsTimeSpan = this.RepeatsTimeSpan
            };
        }
    }

    public class EventType
    {
        public enum Types
        {
            Meeting,
            Homework,
            Reminder
        }

        public int Id { get; set; }
        public Types Type { get; set; }
        public string Color { get; set; }
    }

}